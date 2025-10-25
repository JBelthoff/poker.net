// FiveCardBench.cs
// Apples-to-apples with https://github.com/bwedding/PokerEvalMultiThread
// - Pre-generate random hands (outside timing) with deterministic seed
// - Time ONLY the evaluation pass
// - Contention-free parallel range partitioning (no Interlocked in hot path)
// - 5-card kernel and 7-card → best-of-21 (21 × eval_5cards_fast)
// - Cactus-Kev-compatible card encoding for the 52-card deck

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using static poker.net.Services.PokerLib; // expects eval_5cards_fast(int,int,int,int,int)

namespace PokerBenchmarks
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    public class FiveCardBench
    {
        // Match the C++ harness: evaluate a large flat array of hands
        [Params(10_000_000)]
        public int N;

        // Pre-generated, encoded, flat arrays:
        //  - 5-card: length = N * 5
        //  - 7-card: length = N * 7
        private int[] _hands5Flat;
        private int[] _hands7Flat;

        private ulong _lastChecksum;

        // Deterministic RNG seed for reproducibility
        private const int Seed = unchecked((int)0xC0FFEE);

        // ======== Setup ========

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Build a Cactus-Kev encoded deck of 52 cards
            var kevDeck = BuildKevDeck();

            _hands5Flat = new int[N * 5];
            _hands7Flat = new int[N * 7];

            // We keep generation OUTSIDE the timed region (like the C++ project)
            var rng = new Random(Seed);

            // Use a reusable index array [0..51] and do partial Fisher–Yates
            int[] idx = new int[52];
            for (int i = 0; i < 52; i++) idx[i] = i;

            // Generate N 5-card hands (distinct cards)
            for (int h = 0; h < N; h++)
            {
                // reset index array
                for (int i = 0; i < 52; i++) idx[i] = i;

                // draw 5 by partial Fisher–Yates
                for (int i = 0; i < 5; i++)
                {
                    int j = i + rng.Next(52 - i);
                    (idx[i], idx[j]) = (idx[j], idx[i]);
                }

                int o = h * 5;
                _hands5Flat[o + 0] = kevDeck[idx[0]];
                _hands5Flat[o + 1] = kevDeck[idx[1]];
                _hands5Flat[o + 2] = kevDeck[idx[2]];
                _hands5Flat[o + 3] = kevDeck[idx[3]];
                _hands5Flat[o + 4] = kevDeck[idx[4]];
            }

            // Generate N 7-card hands (distinct cards)
            for (int h = 0; h < N; h++)
            {
                // reset index array
                for (int i = 0; i < 52; i++) idx[i] = i;

                // draw 7 by partial Fisher–Yates
                for (int i = 0; i < 7; i++)
                {
                    int j = i + rng.Next(52 - i);
                    (idx[i], idx[j]) = (idx[j], idx[i]);
                }

                int o = h * 7;
                _hands7Flat[o + 0] = kevDeck[idx[0]];
                _hands7Flat[o + 1] = kevDeck[idx[1]];
                _hands7Flat[o + 2] = kevDeck[idx[2]];
                _hands7Flat[o + 3] = kevDeck[idx[3]];
                _hands7Flat[o + 4] = kevDeck[idx[4]];
                _hands7Flat[o + 5] = kevDeck[idx[5]];
                _hands7Flat[o + 6] = kevDeck[idx[6]];
            }
        }

        // ======== 5-card kernel ========

        [Benchmark(Description = "5-card kernel: single-thread")]
        public ulong Eval5_SingleThread()
        {
            ulong sum = 0UL;
            var hands = _hands5Flat;
            for (int o = 0; o < hands.Length; o += 5)
            {
                sum += (ulong)EvalEncoded5(
                    hands[o + 0], hands[o + 1], hands[o + 2], hands[o + 3], hands[o + 4]);
            }
            _lastChecksum = sum;
            return sum;
        }

        [Benchmark(Description = "5-card kernel: Parallel.ForEach(range)")]
        public ulong Eval5_Parallel()
        {
            ulong total = 0UL;
            var hands = _hands5Flat;

            // Partition by hand index [0..N), convert to flat ranges inside
            var ranges = Partitioner.Create(0, N,
                Math.Max(50_000, N / (Environment.ProcessorCount * 8)));

            object gate = new object();

            Parallel.ForEach(ranges, range =>
            {
                ulong local = 0UL;
                int start = range.Item1 * 5;
                int end = range.Item2 * 5;

                for (int o = start; o < end; o += 5)
                {
                    local += (ulong)EvalEncoded5(
                        hands[o + 0], hands[o + 1], hands[o + 2], hands[o + 3], hands[o + 4]);
                }

                lock (gate) total += local;
            });

            _lastChecksum = total;
            return total;
        }

        // ======== 7-card → best-of-21 (choose 5 of 7) ========

        [Benchmark(Description = "7→best-of-21: single-thread")]
        public ulong Eval7BestOf21_SingleThread()
        {
            ulong sum = 0UL;
            var hands = _hands7Flat;

            for (int o = 0; o < hands.Length; o += 7)
            {
                int best = int.MaxValue;

                // iterate 21 combos of 5 indices from {0..6}
                for (int k = 0; k < Comb21Count; k++)
                {
                    ref readonly Combo5 c = ref Comb21[k];

                    int v = EvalEncoded5(
                        hands[o + c.A], hands[o + c.B], hands[o + c.C],
                        hands[o + c.D], hands[o + c.E]);

                    if (v < best) best = v;
                }

                sum += (ulong)best;
            }

            _lastChecksum = sum;
            return sum;
        }

        [Benchmark(Description = "7→best-of-21: Parallel.ForEach(range)")]
        public ulong Eval7BestOf21_Parallel()
        {
            ulong total = 0UL;
            var hands = _hands7Flat;

            var ranges = Partitioner.Create(0, N,
                Math.Max(25_000, N / (Environment.ProcessorCount * 8)));

            object gate = new object();

            Parallel.ForEach(ranges, range =>
            {
                ulong local = 0UL;
                int start = range.Item1 * 7;
                int end = range.Item2 * 7;

                for (int o = start; o < end; o += 7)
                {
                    int best = int.MaxValue;

                    for (int k = 0; k < Comb21Count; k++)
                    {
                        ref readonly Combo5 c = ref Comb21[k];

                        int v = EvalEncoded5(
                            hands[o + c.A], hands[o + c.B], hands[o + c.C],
                            hands[o + c.D], hands[o + c.E]);

                        if (v < best) best = v;
                    }

                    local += (ulong)best;
                }

                lock (gate) total += local;
            });

            _lastChecksum = total;
            return total;
        }

        // ======== Helpers ========

        // Cactus-Kev encoding:
        //   card = prime[rank]                 // bits 0..7
        //        | (rank << 8)                 // bits 8..11
        //        | (1 << (16 + rank))          // bits 16..28 (rank bit mask)
        //        | (suitMask << 12);           // bits 12..15 (1,2,4,8)
        // suit: 0=♣,1=♦,2=♥,3=♠  (mask = 1<<suit => 1,2,4,8)
        private static int[] BuildKevDeck()
        {
            var deck = new int[52];
            for (int id = 0; id < 52; id++)
            {
                int rank = id % 13;    // 0..12
                int suit = id / 13;    // 0..3
                deck[id] = EncodeKev(rank, suit);
            }
            return deck;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int EncodeKev(int rank, int suit)
        {
            // rank primes per classic Cactus Kev ordering (2..A)
            // ranks: 0..12 => deuce..ace
            // primes: 2,3,5,7,11,13,17,19,23,29,31,37,41
            int prime = RankPrimes[rank];
            int suitMask = 1 << suit; // 1,2,4,8

            int rBit = 1 << (16 + rank); // straight detection bit
            int rNib = rank << 8;        // rank nibble
            int sNib = suitMask << 12;   // suit nibble

            return rBit | sNib | rNib | prime;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int EvalEncoded5(int c0, int c1, int c2, int c3, int c4)
            => eval_5cards_fast(c0, c1, c2, c3, c4);

        // Choose(7,5) = 21 combos, hard-coded for speed (indices 0..6)
        private readonly struct Combo5
        {
            public readonly int A, B, C, D, E;
            public Combo5(int a, int b, int c, int d, int e) { A = a; B = b; C = c; D = d; E = e; }
        }

        private const int Comb21Count = 21;
        private static readonly Combo5[] Comb21 = new Combo5[Comb21Count]
        {
            new(0,1,2,3,4), new(0,1,2,3,5), new(0,1,2,3,6),
            new(0,1,2,4,5), new(0,1,2,4,6), new(0,1,2,5,6),
            new(0,1,3,4,5), new(0,1,3,4,6), new(0,1,3,5,6),
            new(0,1,4,5,6), new(0,2,3,4,5), new(0,2,3,4,6),
            new(0,2,3,5,6), new(0,2,4,5,6), new(0,3,4,5,6),
            new(1,2,3,4,5), new(1,2,3,4,6), new(1,2,3,5,6),
            new(1,2,4,5,6), new(1,3,4,5,6), new(2,3,4,5,6),
        };

        private static readonly int[] RankPrimes =
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41
        };
    }
}
