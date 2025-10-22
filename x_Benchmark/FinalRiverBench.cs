using System; 
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using poker.net.Models;    // Card
using poker.net.Services;  // PokerLib, EvalEngine

namespace PokerBenchmarks
{
    [MemoryDiagnoser]
    public class FinalRiverBench
    {
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
        public void Setup()
        {
            _orderedDeck = LoadOrderedDeckKevEncoding();
            _shuffled = RestoreShuffledFromIds(_orderedDeck, CardIds);
        }

        // ============================================================
        // NEW: End-to-end measurement using EvalEngine
        //     Builds 9 × 7-card hands (2 hole + 5 board) from _shuffled
        //     Runs EvaluateRiverNinePlayers and returns the min eval
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

        // (Optional) Keep your previous microbenchmark for apples-to-apples:
        // It exercises the low-level loop using PokerLib directly.
        [Benchmark(Description = "Engine-only: 9 × (7-card → best-of-21)")]
        public int EngineOnly_SevenCardBestOf21_9Players()
        {
            // Rebuild players' 7-card VALUES on the fly
            var board = new[]
            {
                _shuffled[18].Value, _shuffled[19].Value, _shuffled[20].Value,
                _shuffled[21].Value, _shuffled[22].Value
            };

            // Reusable 5-card buffer (Cards, since eval_5hand_fast_jb takes List<Card>)
            var c0 = new Card(); var c1 = new Card(); var c2 = new Card(); var c3 = new Card(); var c4 = new Card();
            var tmp5List = new List<Card>(5) { c0, c1, c2, c3, c4 };

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

                for (int row = 0; row < 21; row++)
                {
                    c0.Value = sevenVals[PokerLib.perm7[row, 0]];
                    c1.Value = sevenVals[PokerLib.perm7[row, 1]];
                    c2.Value = sevenVals[PokerLib.perm7[row, 2]];
                    c3.Value = sevenVals[PokerLib.perm7[row, 3]];
                    c4.Value = sevenVals[PokerLib.perm7[row, 4]];

                    var v = PokerLib.eval_5hand_fast_jb(tmp5List);
                    if (v < best) best = v;
                }

                acc ^= best;
            }

            return acc;
        }

        // ---------- deck building / restore ----------

        private static List<Card> LoadOrderedDeckKevEncoding()
        {
            var deck = new List<Card>(52);

            int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41 };
            string[] faces = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            var suits = new (string Name, int Mask, string Color)[]
            {
                ("Clubs",    PokerLib.CLUB,    "Black"),
                ("Diamonds", PokerLib.DIAMOND, "Red"),
                ("Hearts",   PokerLib.HEART,   "Red"),
                ("Spades",   PokerLib.SPADE,   "Black"),
            };

            int id = 1;
            foreach (var (name, mask, color) in suits)
            {
                for (int rank = PokerLib.Deuce; rank <= PokerLib.Ace; rank++)
                {
                    int value = primes[rank]
                              | (rank << 8)
                              | mask
                              | (1 << (16 + rank));

                    deck.Add(new Card
                    {
                        ID = id++,
                        Face = faces[rank],
                        Suit = name,
                        Color = color,
                        Value = value
                    });
                }
            }

            if (deck.Count != 52) throw new InvalidOperationException("Deck build failed.");
            return deck;
        }

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
