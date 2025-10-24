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
            EvaluateRiverNinePlayers(IReadOnlyList<Card> deck)
        {
            var scores = new ushort[9];
            var ranks = new int[9];
            var bestIndexes = new int[9];
            var bestHands = new List<List<Card>>(9);

            // Reuse local buffers to minimize allocations
            var seven = new Card[7];
            var tmp5 = new Card[5];

            for (int p = 0; p < 9; p++)
            {
                // Build this player's 7-card set: 2 hole + 5 community
                seven[0] = deck[p];        // hole 1
                seven[1] = deck[p + 9];    // hole 2
                for (int x = 0; x < 5; x++)
                    seven[x + 2] = deck[18 + x]; // community cards

                // Best-of-21 5-card combinations (perm7 rows 0..20)
                bestIndexes[p] = GetBestOf7_NoAllocs(seven, tmp5);

                // Copy the winning 5 cards into tmp5 (for eval + display)
                FillBest5(seven, bestIndexes[p], tmp5);

                // Fast evaluator on 5 values (no List allocations)
                scores[p] = PokerLib.eval_5cards_fast(
                    tmp5[0].Value, tmp5[1].Value, tmp5[2].Value, tmp5[3].Value, tmp5[4].Value);

                // Rank bucket (1..9): 1=Straight Flush .. 9=High Card
                ranks[p] = PokerLib.hand_rank_jb(scores[p]);

                // UI only: add a sorted copy for display
                bestHands.Add(SortHand(tmp5));
            }

            return (scores, ranks, bestIndexes, bestHands);
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
