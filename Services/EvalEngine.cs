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

            // Hoist board VALUES once
            int b0 = deck[18].Value, b1 = deck[19].Value, b2 = deck[20].Value, b3 = deck[21].Value, b4 = deck[22].Value;

            // 🔥 Hoist hot tables ONCE per showdown (ReadOnlySpan<ushort>)
            var flushes = PokerLib.Flushes;
            var unique5 = PokerLib.Unique5;
            var hashes = PokerLib.HashValues;

            for (int p = 0; p < 9; p++)
            {
                // 7 VALUES for this player (stack-allocated, no GC)
                Span<int> sevenVals = stackalloc int[7];
                sevenVals[0] = deck[p].Value;       // hole 1
                sevenVals[1] = deck[p + 9].Value;   // hole 2
                sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

                // One-pass best-of-21 search on VALUES ONLY (allocation-free)
                ushort bestScore = ushort.MaxValue;
                int bestRow = 0;

                for (int row = 0; row < 21; row++)
                {
                    ushort v = EvalFiveFromPerm(sevenVals, row, flushes, unique5, hashes);
                    if (v < bestScore) { bestScore = v; bestRow = row; }
                }

                scores[p] = bestScore;
                ranks[p] = PokerLib.hand_rank_jb(bestScore);
                bestIndexes[p] = bestRow;

                if (includeBestHands)
                    bestHands.Add(BuildBest5FromDeck(deck, p, bestRow));
            }

            return (scores, ranks, bestIndexes, bestHands);
        }

        /// <summary>
        /// Evaluates a 7-card set using a given permutation row index (allocation-free).
        /// Tables are passed explicitly to avoid capturing ReadOnlySpan (ref struct) in closures.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort EvalFiveFromPerm(
            ReadOnlySpan<int> sevenVals,
            int permRow,
            ReadOnlySpan<ushort> flushes,
            ReadOnlySpan<ushort> unique5,
            ReadOnlySpan<ushort> hashes)
        {
            var perm = PokerLib.Perm7Indices; // ReadOnlySpan<byte>; 21 rows × 5 cols (flattened)
            int baseIdx = permRow * 5;

            int v0 = sevenVals[perm[baseIdx + 0]];
            int v1 = sevenVals[perm[baseIdx + 1]];
            int v2 = sevenVals[perm[baseIdx + 2]];
            int v3 = sevenVals[perm[baseIdx + 3]];
            int v4 = sevenVals[perm[baseIdx + 4]];

            return PokerLib.Eval5With(flushes, unique5, hashes, v0, v1, v2, v3, v4);
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
        /// Kept for parity with older call sites; avoids allocating by reading VALUES directly.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBestOf7_NoAllocs(IReadOnlyList<Card> seven, Card[] tmp5 /* kept for signature parity */)
        {
            ushort best = ushort.MaxValue;
            int bestRow = 0;

            var perm = PokerLib.Perm7Indices; // ReadOnlySpan<byte>; 21 rows × 5 cols (flattened)

            for (int row = 0; row < 21; row++)
            {
                int i = row * 5;

                // Read 5 VALUES directly
                int v0 = seven[perm[i + 0]].Value;
                int v1 = seven[perm[i + 1]].Value;
                int v2 = seven[perm[i + 2]].Value;
                int v3 = seven[perm[i + 3]].Value;
                int v4 = seven[perm[i + 4]].Value;

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
            var perm = PokerLib.Perm7Indices;
            int i = row * 5;

            dst5[0] = seven[perm[i + 0]];
            dst5[1] = seven[perm[i + 1]];
            dst5[2] = seven[perm[i + 2]];
            dst5[3] = seven[perm[i + 3]];
            dst5[4] = seven[perm[i + 4]];
        }

        /// <summary>
        /// Map faces for UI sort. (Evaluation uses scores; this is just display order.)
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

            int key(Card c) => (isWheel && c.Face == "A") ? 1 : RankValue[c.Face];

            return cards
                .OrderBy(key)
                .ThenBy(c => c.Suit)
                .ToList();
        }
    }
}
