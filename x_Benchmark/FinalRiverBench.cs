﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using poker.net.Interfaces; // IDeckService
using poker.net.Models;    // Card
using poker.net.Services;  // StaticDeckService / SqlDeckService, PokerLib, EvalEngine

namespace PokerBenchmarks
{
    /// <summary>
    /// Human-readable benchmarks for the poker evaluator.
    /// - “End-to-End” benchmarks simulate a full 9-player river showdown, including setup, scoring, and (optionally) building UI-ready best hands.
    /// - “Engine-only” benchmarks isolate just the core math that picks each player’s best 5-card hand from 7 cards (21 combos), to show raw algorithm speed.
    /// - The “values-only, zero-allocs” variant demonstrates maximum throughput by avoiding object allocations and using raw integer values on the stack.
    /// </summary>
    [MemoryDiagnoser]
    
    public class FinalRiverBench
    {
        // Allow DI but also work out-of-the-box for BenchmarkDotNet
        private IDeckService _deckService;
        public FinalRiverBench() : this(new StaticDeckService()) { }
        public FinalRiverBench(IDeckService deckService) => _deckService = deckService;

        /// <summary>
        /// Deck snapshot (by card IDs) used to rebuild a specific shuffled deal.
        /// You can add more scenarios via additional [Params(...)] values to average across multiple boards.
        /// Current deck produces a single Straight Flush win
        /// </summary>
        [Params(
            "30|8|12|19|23|27|31|48|16|26|35|47|51|36|43|39|40|3|22|18|14|52|15|1|2|4|5|6|7|9|10|11|13|17|20|21|24|25|28|29|32|33|34|37|38|41|42|44|45|46|49|50"
        //,
        // "6|27|12|18|16|8|37|23|9|22|42|19|2|48|47|26|11|41|20|17|44|51|33|28|7|30|34|13|29|31|52|21|24|14|10|46|1|4|32|25|35|15|39|43|49|3|50|5|38|45|36|40",
        //"46|50|20|25|12|42|36|5|43|2|26|33|13|8|31|16|4|35|45|11|22|10|21|24|41|37|30|18|27|15|40|34|19|52|44|7|39|48|23|1|47|38|51|3|9|32|49|6|14|29|28|17",
        //"17|21|46|27|32|31|38|19|26|9|1|52|42|51|4|18|39|35|34|41|43|33|29|30|49|28|2|14|20|37|25|8|7|11|40|6|15|50|12|5|47|16|44|36|13|48|3|24|22|10|23|45",
        //"7|48|34|27|10|6|31|4|16|20|18|45|33|49|50|32|19|35|2|40|51|43|22|26|11|42|38|14|9|17|36|25|24|28|52|30|41|21|15|1|3|12|44|46|5|37|29|39|8|23|13|47",
        //"38|39|43|23|12|31|36|15|28|18|44|4|11|5|13|25|32|22|41|3|33|37|24|26|10|8|30|17|16|7|40|20|2|27|52|1|46|9|48|14|50|29|42|35|47|49|21|19|34|45|51|6"
        )]
        public string CardIds { get; set; } = string.Empty;

        // ---------- Deck / data ----------
        private List<Card> _orderedDeck = default!;
        private List<Card> _shuffled = default!;

        /// <summary>
        /// Builds the in-memory deck and restores the chosen shuffled deal from the CardIds string.
        /// Layman’s take: “Set up the exact cards we want to test so each run measures the same scenario.”
        /// </summary>
        [GlobalSetup]
        public async Task Setup()
        {
            _orderedDeck = (await _deckService.RawDeckAsync()).ToList();
            _shuffled = RestoreShuffledFromIds(_orderedDeck, CardIds);
        }

        /// <summary>
        /// Full 9-player showdown: complete evaluation including setup and scoring.
        /// Layman’s take: “Simulate a real 9-player river hand, evaluate everyone’s best 5-card hand, and decide who wins.”
        /// Includes all typical overhead (data prep, scoring) you’d do in an app.
        /// </summary>
        [Benchmark(Description = "Full 9-player showdown: complete evaluation including setup and scoring")]
        public int EndToEnd_EvalEngine_9Players()
        {
            var (scores, _, _, _) = EvalEngine.EvaluateRiverNinePlayers(_shuffled);
            // Reduce to a scalar to avoid dead-code elimination
            int min = scores[0];
            for (int i = 1; i < 9; i++)
                if (scores[i] < min) min = scores[i];
            return min;
        }

        /// <summary>
        /// Core evaluator: 9 players × 21 combos each (best 5 of 7 cards).
        /// Layman’s take: “Ignore UI/output and just run the math: for each player, check all 21 ways to pick 5 from 7 and keep the best.”
        /// This isolates the algorithm to show raw evaluator speed with minimal overhead.
        /// </summary>
        [Benchmark(Description = "Core evaluator: 9 players × 21 combos each (best 5 of 7 cards)")]
        public int EngineOnly_SevenCardBestOf21_9Players()
        {
            // Community card VALUES once per benchmark op
            var board = new[]
            {
                _shuffled[18].Value, _shuffled[19].Value, _shuffled[20].Value,
                _shuffled[21].Value, _shuffled[22].Value
            };

            // Reusable 5-card buffer (Cards, since eval_5hand_fast_jb takes List<Card>)
            var c0 = new Card(); var c1 = new Card(); var c2 = new Card(); var c3 = new Card(); var c4 = new Card();
            var tmp5List = new List<Card>(5) { c0, c1, c2, c3, c4 };

            // Flattened perm table: length 105 (21 rows × 5 indices)
            var perm = PokerLib.Perm7Indices;

            int acc = 0;

            for (int p = 0; p < 9; p++)
            {
                ushort best = ushort.MaxValue;

                // Build this player's seven VALUES
                var sevenVals = new int[7]
                {
                    _shuffled[p].Value, _shuffled[p + 9].Value,
                    board[0], board[1], board[2], board[3], board[4]
                };

                // 21 × choose-5 combinations via flattened perm table
                for (int row = 0; row < 21; row++)
                {
                    int baseIdx = row * 5;

                    c0.Value = sevenVals[perm[baseIdx + 0]];
                    c1.Value = sevenVals[perm[baseIdx + 1]];
                    c2.Value = sevenVals[perm[baseIdx + 2]];
                    c3.Value = sevenVals[perm[baseIdx + 3]];
                    c4.Value = sevenVals[perm[baseIdx + 4]];

                    var v = PokerLib.eval_5hand_fast_jb(tmp5List);
                    if (v < best) best = v;
                }

                acc ^= best;
            }

            return acc;
        }


        /// <summary>
        /// Optimized core evaluator: values-only, zero allocations (max throughput).
        /// Layman’s take: “The same core math as above, but cranked to max speed by using raw ints on the stack
        /// and avoiding new objects. This shows the highest possible algorithm throughput.”
        /// </summary>
        [Benchmark(Description = "Optimized core evaluator: values-only, zero allocations (max throughput)")]
        public int EngineOnly_SevenCardBestOf21_9Players_ValuesOnly_NoAllocs()
        {
            // Board card VALUES once
            int b0 = _shuffled[18].Value, b1 = _shuffled[19].Value, b2 = _shuffled[20].Value,
                b3 = _shuffled[21].Value, b4 = _shuffled[22].Value;

            // Flattened perm table (21 * 5)
            var perm = PokerLib.Perm7Indices; // passthrough property

            int acc = 0;

            for (int p = 0; p < 9; p++)
            {
                ushort best = ushort.MaxValue;

                // Reuse a stack buffer for this player's seven VALUES (no heap alloc)
                Span<int> sevenVals = stackalloc int[7];
                sevenVals[0] = _shuffled[p].Value;
                sevenVals[1] = _shuffled[p + 9].Value;
                sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

                // 21 combos via flattened indices
                for (int row = 0; row < 21; row++)
                {
                    int i = row * 5;
                    ushort v = PokerLib.Eval5CardsFast(
                        sevenVals[perm[i + 0]],
                        sevenVals[perm[i + 1]],
                        sevenVals[perm[i + 2]],
                        sevenVals[perm[i + 3]],
                        sevenVals[perm[i + 4]]
                    );
                    if (v < best) best = v;
                }

                acc ^= best;
            }

            return acc;
        }

        /// <summary>
        /// Full 9-player evaluation + best-hand reconstruction (UI-ready output).
        /// Layman’s take: “Like the full simulation, but also build and sort the actual 5 cards each winner used,
        /// so it’s ready to display in the UI or logs. This includes the extra work of creating those card lists.”
        /// </summary>
        [Benchmark(Description = "Full 9-player evaluation + best-hand reconstruction (UI-ready output)")]
        public int EndToEnd_EvalEngine_IncludeBestHands()
        {
            var (scores, ranks, bestIdx, bestHands) =
                EvalEngine.EvaluateRiverNinePlayers(_shuffled, includeBestHands: true);

            // Prevent dead-code elimination by folding results
            int acc = 0;
            for (int i = 0; i < 9; i++)
            {
                acc ^= scores[i];
                acc ^= ranks[i];
                acc ^= bestIdx[i];
                acc ^= bestHands[i][0].Value; // touch UI hand data
            }
            return acc;
        }

        /// <summary>
        /// 9-player river evaluation (values-only, best-of-21) optimized for raw throughput.
        /// Layman’s take: “Simulates nine players facing a shared five-card board, finding each best 5-card hand
        /// using 21 possible combinations per player. Uses a batched Parallel.For loop for minimal scheduling overhead,
        /// so it measures the engine’s pure evaluation speed rather than UI or allocation costs.”
        /// </summary>
        [Params(10_000_000)]
        public int N;
        
        [Params(64)] // optional second throughput for comparison; try 32/64/128
        public int Batch;

        [Benchmark(Description = "Throughput: Parallel.For batched (values-only)")]
        public int Parallel_Batched_ValuesOnly()
        {
            int b0 = _shuffled[18].Value, b1 = _shuffled[19].Value, b2 = _shuffled[20].Value,
                b3 = _shuffled[21].Value, b4 = _shuffled[22].Value;

            // Capture an array (not a span) across the closure.
            var perm = PokerLib.Perm7Indices.ToArray();
            int groups = (N + Batch - 1) / Batch;

            int global = 0;

            Parallel.For(0, groups,
                () => 0,
                (g, _, local) =>
                {
                    int start = g * Batch;
                    int end = Math.Min(start + Batch, N);

                    int sum = 0;
                    for (int iter = start; iter < end; iter++)
                    {
                        for (int p = 0; p < 9; p++)
                        {
                            Span<int> seven = stackalloc int[7];
                            seven[0] = _shuffled[p].Value;
                            seven[1] = _shuffled[p + 9].Value;
                            seven[2] = b0; seven[3] = b1; seven[4] = b2; seven[5] = b3; seven[6] = b4;

                            ushort best = ushort.MaxValue;

                            for (int row = 0; row < 21; row++)
                            {
                                int i = row * 5;
                                ushort v = PokerLib.Eval5CardsFast(
                                    seven[perm[i + 0]],
                                    seven[perm[i + 1]],
                                    seven[perm[i + 2]],
                                    seven[perm[i + 3]],
                                    seven[perm[i + 4]]
                                );
                                if (v < best) best = v;
                            }

                            sum += best;
                        }
                    }

                    return local + sum;
                },
                local => Interlocked.Add(ref global, local)
            );
            return global;
        }



        #region Helper

            // ---------- deck restore from ID string ----------
        private static List<Card> RestoreShuffledFromIds(IReadOnlyList<Card> orderedDeck, string cardIds)
        {
            if (orderedDeck is null) throw new ArgumentNullException(nameof(orderedDeck));
            if (string.IsNullOrWhiteSpace(cardIds))
                throw new ArgumentException("CardIDs cannot be null or empty.", nameof(cardIds));

            var lookup = orderedDeck.ToDictionary(c => c.ID);
            var shuffled = new List<Card>(orderedDeck.Count);

            foreach (var idStr in cardIds.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(idStr, out int id) || !lookup.TryGetValue(id, out var card))
                    throw new InvalidOperationException($"Invalid Card ID: {idStr}");

                shuffled.Add(new Card
                {
                    ID = card.ID,
                    Face = card.Face,
                    Suit = card.Suit,
                    Color = card.Color,
                    Value = card.Value
                });
            }

            return shuffled;
        }

        #endregion
    }
}


#region Old Code

// Moved to CPlusPlus.cs
//
//[Params(10_000_000)]
//public int N;

//// CHAMPION throughput benchmark: minimal overhead, values-only, no allocations on hot path.
//[Benchmark(Description = "Throughput: Parallel 9-player evals (values-only, flattened Perm7)")]
//public int Parallel_Throughput_ValuesOnly()
//{
//    int b0 = _shuffled[18].Value, b1 = _shuffled[19].Value, b2 = _shuffled[20].Value,
//        b3 = _shuffled[21].Value, b4 = _shuffled[22].Value;

//    // Hoist once; give each worker a stable array (no Span capture issues).
//    var perm = PokerLib.Perm7Indices.ToArray();

//    int global = 0;
//    Parallel.For(0, N,
//        () => 0,
//        (iter, _, local) =>
//        {
//            int sum = 0;
//            for (int p = 0; p < 9; p++)
//            {
//                Span<int> sevenVals = stackalloc int[7];
//                sevenVals[0] = _shuffled[p].Value;
//                sevenVals[1] = _shuffled[p + 9].Value;
//                sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

//                ushort best = ushort.MaxValue;
//                for (int row = 0; row < 21; row++)
//                {
//                    int i = row * 5;
//                    ushort v = PokerLib.Eval5CardsFast(
//                        sevenVals[perm[i + 0]],
//                        sevenVals[perm[i + 1]],
//                        sevenVals[perm[i + 2]],
//                        sevenVals[perm[i + 3]],
//                        sevenVals[perm[i + 4]]);
//                    if (v < best) best = v;
//                }
//                sum += best;
//            }
//            return local + sum;
//        },
//        local => Interlocked.Add(ref global, local));
//    return global;
//}


//[Benchmark(Description = "Micro: Eval5CardsFast tight loop")]
//public int Micro_Eval5CardsFast()
//{
//    int acc = 0;
//    var v = new[] { _shuffled[0].Value, _shuffled[1].Value, _shuffled[18].Value, _shuffled[19].Value, _shuffled[20].Value };
//    for (int i = 0; i < 1_000_000; i++)
//    {
//        acc ^= PokerLib.Eval5CardsFast(v[(i + 0) % 5], v[(i + 1) % 5], v[(i + 2) % 5], v[(i + 3) % 5], v[(i + 4) % 5]);
//    }
//    return acc;
//}


//[Params(64)] // optional second throughput for comparison; try 32/64/128
//public int Batch;

//[Benchmark(Description = "Throughput: Parallel.For batched (values-only)")]
//public int Parallel_Batched_ValuesOnly()
//{
//    int b0 = _shuffled[18].Value, b1 = _shuffled[19].Value, b2 = _shuffled[20].Value,
//        b3 = _shuffled[21].Value, b4 = _shuffled[22].Value;

//    // Capture an array (not a span) across the closure.
//    var perm = PokerLib.Perm7Indices.ToArray();
//    int groups = (N + Batch - 1) / Batch;

//    int global = 0;

//    Parallel.For(0, groups,
//        () => 0,
//        (g, _, local) =>
//        {
//            int start = g * Batch;
//            int end = Math.Min(start + Batch, N);

//            int sum = 0;
//            for (int iter = start; iter < end; iter++)
//            {
//                for (int p = 0; p < 9; p++)
//                {
//                    Span<int> seven = stackalloc int[7];
//                    seven[0] = _shuffled[p].Value;
//                    seven[1] = _shuffled[p + 9].Value;
//                    seven[2] = b0; seven[3] = b1; seven[4] = b2; seven[5] = b3; seven[6] = b4;

//                    ushort best = ushort.MaxValue;

//                    for (int row = 0; row < 21; row++)
//                    {
//                        int i = row * 5;
//                        ushort v = PokerLib.Eval5CardsFast(
//                            seven[perm[i + 0]],
//                            seven[perm[i + 1]],
//                            seven[perm[i + 2]],
//                            seven[perm[i + 3]],
//                            seven[perm[i + 4]]
//                        );
//                        if (v < best) best = v;
//                    }

//                    sum += best;
//                }
//            }

//            return local + sum;
//        },
//        local => Interlocked.Add(ref global, local)
//    );

//    return global;
//}
#endregion