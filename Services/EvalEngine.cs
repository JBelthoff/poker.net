using poker.net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace poker.net.Services
{
    public static class EvalEngine
    {
        /// <summary>
        /// Evaluates a full 9-player Texas Hold'em showdown at the river using a fixed deck order.
        /// Expects the first 23 cards of <paramref name="deck"/> to be laid out as:
        /// P1..P9 hole1 (9), P1..P9 hole2 (9), then 5 board cards (total 23). The remaining cards are ignored.
        /// Returns hand scores (lower is better), rank buckets (1..9),
        /// perm7 row indexes for each player's best 5-card hand, and a UI-sorted copy of those 5 cards.
        /// </summary>
        /// <param name="deck">A 52-card list in dealing order (hole cards first, then the 5 board cards).</param>
        /// <returns>
        /// scores: ushort[9] (Cactus Kev score; lower is stronger),
        /// ranks: int[9] (1=Straight Flush .. 9=High Card),
        /// bestIndexes: int[9] (perm7 row 0..20 for best 5-card combo),
        /// bestHands: List<List<Card>> (UI-sorted 5-card best hands).
        /// </returns>
        public static (ushort[] scores, int[] ranks, int[] bestIndexes, List<List<Card>> bestHands)
    EvaluateRiverNinePlayers(IReadOnlyList<Card> deck, bool includeBestHands = false)
        {
            var scores = new ushort[9];
            var ranks = new int[9];
            var bestIndexes = new int[9];
            var bestHands = includeBestHands ? new List<List<Card>>(9) : new List<List<Card>>(0);

            // Board VALUES once
            int b0 = deck[18].Value, b1 = deck[19].Value, b2 = deck[20].Value, b3 = deck[21].Value, b4 = deck[22].Value;

            // Flattened 21x5 table (length 105)
            var perm = PokerLib.Perm7Indices;

            for (int p = 0; p < 9; p++)
            {
                // 7 VALUES for this player (stack-allocated, no GC)
                Span<int> sevenVals = stackalloc int[7];
                sevenVals[0] = deck[p].Value;       // hole 1
                sevenVals[1] = deck[p + 9].Value;   // hole 2
                sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

                // One-pass best-of-21 search on VALUES ONLY
                ushort bestScore = ushort.MaxValue;
                int bestRow = 0;

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
                    if (v < bestScore) { bestScore = v; bestRow = row; }
                }

                scores[p] = bestScore;
                ranks[p] = PokerLib.hand_rank_jb(bestScore);
                bestIndexes[p] = bestRow;

                // Only build/display hands when explicitly requested
                if (includeBestHands)
                    bestHands.Add(BuildBest5FromDeck(deck, p, bestRow));
            }

            return (scores, ranks, bestIndexes, bestHands);
        }

        // Build the 5 UI cards directly from the deck and best perm row (no 7-card array needed)
        private static List<Card> BuildBest5FromDeck(IReadOnlyList<Card> deck, int playerIndex, int bestRow)
        {
            var perm = PokerLib.Perm7Indices;
            int baseIdx = bestRow * 5;

            // Map Perm7's 7-slot indices back to deck positions: 0=p, 1=p+9, 2..6=board 18..22
            static Card Pick(IReadOnlyList<Card> d, int p, int sevenIdx) => sevenIdx switch
            {
                0 => d[p],
                1 => d[p + 9],
                2 => d[18],
                3 => d[19],
                4 => d[20],
                5 => d[21],
                6 => d[22],
                _ => throw new ArgumentOutOfRangeException(nameof(sevenIdx))
            };

            var best5 = new List<Card>(5)
            {
                Pick(deck, playerIndex, perm[baseIdx + 0]),
                Pick(deck, playerIndex, perm[baseIdx + 1]),
                Pick(deck, playerIndex, perm[baseIdx + 2]),
                Pick(deck, playerIndex, perm[baseIdx + 3]),
                Pick(deck, playerIndex, perm[baseIdx + 4]),
            };

            // Reuse your existing SortHand for UI consistency (kept out of hot path)
            return SortHand(best5);
        }

        /// <summary>
        /// Returns the perm7 row (0..20) of the best 5-card subhand from a 7-card set.
        /// Reuses <paramref name="tmp5"/> to avoid allocations while iterating all 21 combinations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBestOf7_NoAllocs(IReadOnlyList<Card> seven, Card[] tmp5 /* kept for signature parity */)
        {
            ushort best = ushort.MaxValue;
            int bestRow = 0;

            var perm = PokerLib.Perm7.Indices; // ReadOnlySpan<byte>; 21 rows × 5 cols (flattened)

            for (int row = 0; row < 21; row++)
            {
                int i = row * 5;

                // Read the 5 card VALUES directly (no copying Card structs into tmp5)
                int v0 = seven[perm[i + 0]].Value;
                int v1 = seven[perm[i + 1]].Value;
                int v2 = seven[perm[i + 2]].Value;
                int v3 = seven[perm[i + 3]].Value;
                int v4 = seven[perm[i + 4]].Value;

                // Hot-path 5-card evaluator (already inlined)
                var score = PokerLib.eval_5cards_fast(v0, v1, v2, v3, v4);
                if (score < best) { best = score; bestRow = row; }
            }

            return bestRow;
        }

        /// <summary>
        /// Copies the 5 winning cards (identified by Perm7 row) from the 7-card set into <paramref name="dst5"/>.
        /// </summary>
        private static void FillBest5(IReadOnlyList<Card> seven, int row, Card[] dst5)
        {
            var perm = PokerLib.Perm7.Indices;
            int i = row * 5;

            dst5[0] = seven[perm[i + 0]];
            dst5[1] = seven[perm[i + 1]];
            dst5[2] = seven[perm[i + 2]];
            dst5[3] = seven[perm[i + 3]];
            dst5[4] = seven[perm[i + 4]];
        }

        /// <summary>
        /// Maps card face strings ("2"–"A") to their numeric rank values (2–14).
        /// Used by <see cref="SortHand(System.Collections.Generic.IEnumerable{Card})"/> to create a consistent UI sort.
        /// (Evaluation uses <c>PokerLib</c> scores — this map does not affect hand strength.)
        /// </summary>
        private static readonly Dictionary<string, int> RankValue = new()
        {
            ["2"] = 2,
            ["3"] = 3,
            ["4"] = 4,
            ["5"] = 5,
            ["6"] = 6,
            ["7"] = 7,
            ["8"] = 8,
            ["9"] = 9,
            ["10"] = 10,
            ["J"] = 11,
            ["Q"] = 12,
            ["K"] = 13,
            ["A"] = 14
        };

        /// <summary>
        /// Returns a new list of cards sorted for display:
        /// primary ascending by rank (2..A), with A treated as low in a wheel (A-2-3-4-5);
        /// secondary ascending by suit to keep a stable, readable order.
        /// This is for UI only and does not influence evaluation.
        /// </summary>
        private static List<Card> SortHand(IEnumerable<Card> hand)
        {
            var cards = hand.ToList();

            // Detect the wheel straight (A,2,3,4,5). Only triggers when all five are present.
            bool isWheel =
                cards.Count == 5 &&
                cards.Any(c => c.Face == "A") &&
                cards.Any(c => c.Face == "2") &&
                cards.Any(c => c.Face == "3") &&
                cards.Any(c => c.Face == "4") &&
                cards.Any(c => c.Face == "5");

            int key(Card c)
            {
                // In a wheel, treat Ace as rank 1 so it sorts to the front: A 2 3 4 5
                if (isWheel && c.Face == "A") return 1;
                return RankValue[c.Face];
            }

            return cards
                .OrderBy(key)
                .ThenBy(c => c.Suit)
                .ToList();
        }
    }
}
