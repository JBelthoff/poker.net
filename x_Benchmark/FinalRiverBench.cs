using BenchmarkDotNet.Attributes;
using poker.net.Interfaces;
using poker.net.Models;    // Card
using poker.net.Services;  // StaticDeckService / SqlDeckService, PokerLib, EvalEngine

namespace PokerBenchmarks
{
    [MemoryDiagnoser]
    public class FinalRiverBench
    {
        // Allow DI but also work out-of-the-box for BenchmarkDotNet
        private IDeckService _deckService;
        public FinalRiverBench() : this(new StaticDeckService()) { }
        public FinalRiverBench(IDeckService deckService) => _deckService = deckService;

        // Add more CardIds via [Params(..., ...)] if you want to average across different boards.
        [Params(
            "6|27|12|18|16|8|37|23|9|22|42|19|2|48|47|26|11|41|20|17|44|51|33|28|7|30|34|13|29|31|52|21|24|14|10|46|1|4|32|25|35|15|39|43|49|3|50|5|38|45|36|40"
        )]
        public string CardIds { get; set; } = string.Empty;

        // ---------- Deck / data ----------
        private List<Card> _orderedDeck = default!;
        private List<Card> _shuffled = default!;

        // ---------- Setup ----------
        [GlobalSetup]
        public async Task Setup()
        {
            _orderedDeck = (await _deckService.RawDeckAsync()).ToList();
            _shuffled = RestoreShuffledFromIds(_orderedDeck, CardIds);
        }

        // ============================================================
        // End-to-end measurement using EvalEngine
        //   Builds 9 × 7-card hands (2 hole + 5 board) from _shuffled
        //   Runs EvaluateRiverNinePlayers and returns the min eval
        // ============================================================
        [Benchmark(Description = "End-to-End (EvalEngine): 9 players • best-of-7 • winner")]
        public int EndToEnd_EvalEngine_9Players()
        {
            var (scores, _, _, _) = EvalEngine.EvaluateRiverNinePlayers(_shuffled);
            // Reduce to a scalar to avoid dead-code elimination
            int min = scores[0];
            for (int i = 1; i < 9; i++)
                if (scores[i] < min) min = scores[i];
            return min;
        }

        // (Optional) apples-to-apples: low-level loop using PokerLib directly.
        [Benchmark(Description = "Engine-only: 9 × (7-card → best-of-21)")]
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

        [Benchmark(Description = "Engine-only: values-only, flattened Perm7, no allocs")]
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

        [Benchmark(Description = "End-to-End (EvalEngine): includeBestHands=true")]
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

        [Params(10_000_000)]
        public int N;

        // CHAMPION throughput benchmark: minimal overhead, values-only, no allocations on hot path.
        [Benchmark(Description = "Throughput: Parallel 9-player evals (values-only, flattened Perm7)")]
        public int Parallel_Throughput_ValuesOnly()
        {
            int b0 = _shuffled[18].Value, b1 = _shuffled[19].Value, b2 = _shuffled[20].Value,
                b3 = _shuffled[21].Value, b4 = _shuffled[22].Value;

            int global = 0;

            Parallel.For(0, N,
                () => 0, // thread-local init
                (iter, _, local) =>
                {
                    // Span lives inside the lambda; safe and fast.
                    var perm = PokerLib.Perm7Indices; // flattened 21×5

                    int sum = 0;

                    for (int p = 0; p < 9; p++)
                    {
                        Span<int> sevenVals = stackalloc int[7];
                        sevenVals[0] = _shuffled[p].Value;
                        sevenVals[1] = _shuffled[p + 9].Value;
                        sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

                        ushort best = ushort.MaxValue;

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

                        sum += best;
                    }

                    return local + sum;
                },
                local => Interlocked.Add(ref global, local)
            );

            return global;
        }

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
    }
}
