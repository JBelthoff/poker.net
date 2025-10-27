using poker.net.Models;
using System.Runtime.CompilerServices;

namespace poker.net.Services
{
    public static class EvalEngine
    {
        // ------------------------------------------------------------
        //  PUBLIC API — EXISTING OVERLOADS (unchanged)
        // ------------------------------------------------------------

        /// <summary>
        /// Evaluates a full 9-player Texas Hold'em showdown at the river using a fixed deck order.
        /// Expects the first 23 cards of <paramref name="deck"/> to be laid out as:
        /// P1..P9 hole1 (9), P1..P9 hole2 (9), then 5 board cards (total 23). The remaining cards are ignored.
        /// Returns hand scores (lower is better), rank buckets (1..9),
        /// perm7 row indexes for each player's best 5-card hand, and a UI-sorted copy of those 5 cards.
        /// </summary>
        public static (ushort[] scores, int[] ranks, int[] bestIndexes, List<List<Card>> bestHands)
            EvaluateRiverNinePlayers(IReadOnlyList<Card> deck, bool includeBestHands = false)
        {
            var scores = new ushort[9];
            var ranks = new int[9];
            var bestIndexes = new int[9];
            var bestHands = includeBestHands ? new List<List<Card>>(9) : new List<List<Card>>(0);

            // Hoist board VALUES once
            int b0 = deck[18].Value, b1 = deck[19].Value, b2 = deck[20].Value, b3 = deck[21].Value, b4 = deck[22].Value;

            // Hot tables once
            var flushes = PokerLib.Flushes;
            var unique5 = PokerLib.Unique5;
            var hashes = PokerLib.HashValues;

            for (int p = 0; p < 9; p++)
            {
                Span<int> sevenVals = stackalloc int[7];
                sevenVals[0] = deck[p].Value;
                sevenVals[1] = deck[p + 9].Value;
                sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

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
        /// Array-specialized overload (no interface dispatch). Optionally returns UI-sorted best-5 hands as List.
        /// </summary>
        public static (ushort[] scores, int[] ranks, int[] bestIndexes, List<List<Card>> bestHands)
            EvaluateRiverNinePlayers(Card[] deck, bool includeBestHands = false)
        {
            if (deck is null) throw new ArgumentNullException(nameof(deck));
            if (deck.Length < 23) throw new ArgumentException("Deck must have at least 23 cards.", nameof(deck));

            var scores = new ushort[9];
            var ranks = new int[9];
            var bestIndexes = new int[9];
            var bestHands = includeBestHands ? new List<List<Card>>(9) : new List<List<Card>>(0);

            int b0 = deck[18].Value, b1 = deck[19].Value, b2 = deck[20].Value, b3 = deck[21].Value, b4 = deck[22].Value;

            var flushes = PokerLib.Flushes;
            var unique5 = PokerLib.Unique5;
            var hashes = PokerLib.HashValues;
            var perm = PokerLib.Perm7Indices;

            for (int p = 0; p < 9; p++)
            {
                Span<int> sevenVals = stackalloc int[7];
                sevenVals[0] = deck[p].Value;
                sevenVals[1] = deck[p + 9].Value;
                sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

                ushort bestScore = ushort.MaxValue;
                int bestRow = 0;

                for (int row = 0; row < 21; row++)
                {
                    int i = row * 5;
                    ushort score = PokerLib.Eval5With(
                        flushes, unique5, hashes,
                        sevenVals[perm[i + 0]], sevenVals[perm[i + 1]],
                        sevenVals[perm[i + 2]], sevenVals[perm[i + 3]],
                        sevenVals[perm[i + 4]]);
                    if (score < bestScore) { bestScore = score; bestRow = row; }
                }

                scores[p] = bestScore;
                ranks[p] = PokerLib.hand_rank_jb(bestScore);
                bestIndexes[p] = bestRow;

                if (includeBestHands)
                {
                    int baseIdx = bestRow * 5;
                    var best5 = new List<Card>(5)
                    {
                        PickFromSeven(deck, p, perm[baseIdx + 0]),
                        PickFromSeven(deck, p, perm[baseIdx + 1]),
                        PickFromSeven(deck, p, perm[baseIdx + 2]),
                        PickFromSeven(deck, p, perm[baseIdx + 3]),
                        PickFromSeven(deck, p, perm[baseIdx + 4]),
                    };
                    bestHands.Add(SortHand(best5));
                }
            }

            return (scores, ranks, bestIndexes, bestHands);
        }

        /// <summary>
        /// Array-specialized, returns Card[][] (for existing call-sites that expect materialized cards).
        /// 
        /// UI Is using this!
        /// 
        /// </summary>
        public static (ushort[] scores, int[] ranks, int[] bestIndexes, Card[][] bestHands)
            EvaluateRiverNinePlayersArrays(Card[] deck, bool includeBestHands = false)
        {
            if (deck is null) throw new ArgumentNullException(nameof(deck));
            if (deck.Length < 23) throw new ArgumentException("Deck must have at least 23 cards.", nameof(deck));

            var scores = new ushort[9];
            var ranks = new int[9];
            var bestIndexes = new int[9];
            var bestHands = includeBestHands ? new Card[9][] : Array.Empty<Card[]>();

            int b0 = deck[18].Value, b1 = deck[19].Value, b2 = deck[20].Value, b3 = deck[21].Value, b4 = deck[22].Value;
            var flushes = PokerLib.Flushes;
            var unique5 = PokerLib.Unique5;
            var hashes = PokerLib.HashValues;
            var perm = PokerLib.Perm7Indices;

            for (int p = 0; p < 9; p++)
            {
                Span<int> seven = stackalloc int[7];
                seven[0] = deck[p].Value;
                seven[1] = deck[p + 9].Value;
                seven[2] = b0; seven[3] = b1; seven[4] = b2; seven[5] = b3; seven[6] = b4;

                ushort bestScore = ushort.MaxValue;
                int bestRow = 0;

                for (int row = 0; row < 21; row++)
                {
                    int i = row * 5;
                    ushort s = PokerLib.Eval5With(
                        flushes, unique5, hashes,
                        seven[perm[i + 0]], seven[perm[i + 1]],
                        seven[perm[i + 2]], seven[perm[i + 3]],
                        seven[perm[i + 4]]);
                    if (s < bestScore) { bestScore = s; bestRow = row; }
                }

                scores[p] = bestScore;
                ranks[p] = PokerLib.hand_rank_jb(bestScore);
                bestIndexes[p] = bestRow;

                if (includeBestHands)
                {
                    int baseIdx = bestRow * 5;
                    var a = new Card[5]
                    {
                        PickFromSeven(deck, p, perm[baseIdx + 0]),
                        PickFromSeven(deck, p, perm[baseIdx + 1]),
                        PickFromSeven(deck, p, perm[baseIdx + 2]),
                        PickFromSeven(deck, p, perm[baseIdx + 3]),
                        PickFromSeven(deck, p, perm[baseIdx + 4]),
                    };
                    bestHands[p] = a; // sort later at UI boundary if desired
                }
            }

            return (scores, ranks, bestIndexes, bestHands);
        }

        // ------------------------------------------------------------
        //  NEW PUBLIC API — INDICES PATH (max throughput, min allocs)
        // ------------------------------------------------------------

        /// <summary>
        /// Fastest UI-ready shape: returns indices for the best 5 cards per player.
        /// Materialize cards (and sort) only at the UI boundary when actually needed.
        /// </summary>
        public static (ushort[] scores, int[] ranks, int[] bestIndexes, byte[][] bestIdx5)
            EvaluateRiverNinePlayersIdx(Card[] deck, bool includeBestIndices = false)
        {
            if (deck is null) throw new ArgumentNullException(nameof(deck));
            if (deck.Length < 23) throw new ArgumentException("Deck must have at least 23 cards.", nameof(deck));

            var scores = new ushort[9];
            var ranks = new int[9];
            var bestIndexes = new int[9];
            var bestIdx5 = includeBestIndices ? new byte[9][] : Array.Empty<byte[]>();

            int b0 = deck[18].Value, b1 = deck[19].Value, b2 = deck[20].Value, b3 = deck[21].Value, b4 = deck[22].Value;
            var flushes = PokerLib.Flushes;
            var unique5 = PokerLib.Unique5;
            var hashes = PokerLib.HashValues;
            var perm = PokerLib.Perm7Indices;

            for (int p = 0; p < 9; p++)
            {
                Span<int> sevenVals = stackalloc int[7];
                sevenVals[0] = deck[p].Value;
                sevenVals[1] = deck[p + 9].Value;
                sevenVals[2] = b0; sevenVals[3] = b1; sevenVals[4] = b2; sevenVals[5] = b3; sevenVals[6] = b4;

                ushort bestScore = ushort.MaxValue;
                int bestRow = 0;

                for (int row = 0; row < 21; row++)
                {
                    int i = row * 5;
                    ushort s = PokerLib.Eval5With(
                        flushes, unique5, hashes,
                        sevenVals[perm[i + 0]], sevenVals[perm[i + 1]],
                        sevenVals[perm[i + 2]], sevenVals[perm[i + 3]],
                        sevenVals[perm[i + 4]]);
                    if (s < bestScore) { bestScore = s; bestRow = row; }
                }

                scores[p] = bestScore;
                ranks[p] = PokerLib.hand_rank_jb(bestScore);
                bestIndexes[p] = bestRow;

                if (includeBestIndices)
                {
                    int baseIdx = bestRow * 5;
                    bestIdx5[p] = new byte[5] {
                        (byte)perm[baseIdx + 0],
                        (byte)perm[baseIdx + 1],
                        (byte)perm[baseIdx + 2],
                        (byte)perm[baseIdx + 3],
                        (byte)perm[baseIdx + 4]
                    };
                }
            }

            return (scores, ranks, bestIndexes, bestIdx5);
        }

        /// <summary>
        /// UI boundary helper: materialize a 5-card array for a specific player from 7-card indices (0..6).
        /// Indices map as: 0=hole1, 1=hole2, 2..6 = board cards (18..22 in the deck array).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Card[] MaterializeBest5(Card[] deck, int player, ReadOnlySpan<byte> idx5)
        {
            if (deck is null) throw new ArgumentNullException(nameof(deck));
            if ((uint)player >= 9) throw new ArgumentOutOfRangeException(nameof(player));

            return new Card[5] {
                PickFromSeven(deck, player, idx5[0]),
                PickFromSeven(deck, player, idx5[1]),
                PickFromSeven(deck, player, idx5[2]),
                PickFromSeven(deck, player, idx5[3]),
                PickFromSeven(deck, player, idx5[4]),
            };
        }

        /// <summary>
        /// Determine winner(s) by minimal score. Returns one or more player indices on ties.
        /// </summary>
        public static int[] GetWinners(ReadOnlySpan<ushort> scores)
        {
            ushort best = ushort.MaxValue;
            for (int i = 0; i < scores.Length; i++) if (scores[i] < best) best = scores[i];

            // count
            int count = 0;
            for (int i = 0; i < scores.Length; i++) if (scores[i] == best) count++;

            var winners = new int[count];
            for (int i = 0, w = 0; i < scores.Length; i++)
                if (scores[i] == best) winners[w++] = i;

            return winners;
        }

        // ------------------------------------------------------------
        //  PRIVATE HELPERS
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort EvalFiveFromPerm(
            ReadOnlySpan<int> sevenVals,
            int permRow,
            ReadOnlySpan<ushort> flushes,
            ReadOnlySpan<ushort> unique5,
            ReadOnlySpan<ushort> hashes)
        {
            var perm = PokerLib.Perm7Indices; // 21×5 flattened
            int baseIdx = permRow * 5;

            int v0 = sevenVals[perm[baseIdx + 0]];
            int v1 = sevenVals[perm[baseIdx + 1]];
            int v2 = sevenVals[perm[baseIdx + 2]];
            int v3 = sevenVals[perm[baseIdx + 3]];
            int v4 = sevenVals[perm[baseIdx + 4]];

            return PokerLib.Eval5With(flushes, unique5, hashes, v0, v1, v2, v3, v4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Card PickFromSeven(IReadOnlyList<Card> d, int p, int sevenIdx) => sevenIdx switch
        {
            0 => d[p],       // hole 1
            1 => d[p + 9],   // hole 2
            2 => d[18],
            3 => d[19],
            4 => d[20],
            5 => d[21],
            6 => d[22],
            _ => throw new ArgumentOutOfRangeException(nameof(sevenIdx))
        };

        private static List<Card> BuildBest5FromDeck(IReadOnlyList<Card> deck, int playerIndex, int bestRow)
        {
            var perm = PokerLib.Perm7Indices;
            int baseIdx = bestRow * 5;

            var best5 = new List<Card>(5)
            {
                PickFromSeven(deck, playerIndex, perm[baseIdx + 0]),
                PickFromSeven(deck, playerIndex, perm[baseIdx + 1]),
                PickFromSeven(deck, playerIndex, perm[baseIdx + 2]),
                PickFromSeven(deck, playerIndex, perm[baseIdx + 3]),
                PickFromSeven(deck, playerIndex, perm[baseIdx + 4]),
            };

            return SortHand(best5);
        }

        /// <summary>
        /// (Legacy helper retained for parity; reads Values directly.)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBestOf7_NoAllocs(IReadOnlyList<Card> seven, Card[] tmp5 /* signature parity */)
        {
            ushort best = ushort.MaxValue;
            int bestRow = 0;
            var perm = PokerLib.Perm7Indices;

            for (int row = 0; row < 21; row++)
            {
                int i = row * 5;

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
        /// Return a new list sorted for display (rank ascending, A low in wheel; then suit).
        /// </summary>
        private static List<Card> SortHand(IEnumerable<Card> hand)
        {
            var cards = hand.ToList();

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
