using poker.net.Models;

namespace poker.net.Services
{
    public static class EvalEngine
    {
        // Evaluates a full 9-player Texas Hold’em showdown.
        // Returns (scores[], ranks[], bestHandIndexes[], bestFiveHands[])
        public static (ushort[] scores, int[] ranks, int[] bestIndexes, List<List<Card>> bestHands)
            EvaluateRiverNinePlayers(IReadOnlyList<Card> deck)
        {
            var scores = new ushort[9];
            var ranks = new int[9];
            var bestIndexes = new int[9];
            var bestHands = new List<List<Card>>(9);

            // Reuse local buffers
            var seven = new Card[7];
            var tmp5 = new Card[5];

            for (int p = 0; p < 9; p++)
            {
                // Build this player's 7-card set: 2 hole + 5 community
                seven[0] = deck[p];        // hole 1
                seven[1] = deck[p + 9];    // hole 2
                for (int x = 0; x < 5; x++)
                    seven[x + 2] = deck[18 + x]; // board

                // Best-of-21 combo index over perm7
                bestIndexes[p] = GetBestOf7_NoAllocs(seven, tmp5);

                // Copy the winning 5 cards into tmp5 (for eval + display)
                FillBest5(seven, bestIndexes[p], tmp5);

                // Fast evaluator on 5 ints (avoids making a List<Card>)
                scores[p] = PokerLib.eval_5cards_fast(
                    tmp5[0].Value, tmp5[1].Value, tmp5[2].Value, tmp5[3].Value, tmp5[4].Value);

                // Rank bucket (1..9): 1=Straight Flush .. 9=High Card
                ranks[p] = PokerLib.hand_rank_jb(scores[p]);

                // For UI only: create a sorted copy to show nicely
                bestHands.Add(SortHand(tmp5));
            }

            return (scores, ranks, bestIndexes, bestHands);
        }

        /// <summary>
        /// Returns the perm7 row (0..20) of the best 5-card subhand from the 7-card set.
        /// Reuses tmp5 array to avoid allocations.
        /// </summary>
        private static int GetBestOf7_NoAllocs(IReadOnlyList<Card> seven, Card[] tmp5)
        {
            ushort best = ushort.MaxValue;
            int bestRow = 0;

            for (int row = 0; row < 21; row++)
            {
                // Map 5 indices via PokerLib.perm7
                tmp5[0] = seven[PokerLib.perm7[row, 0]];
                tmp5[1] = seven[PokerLib.perm7[row, 1]];
                tmp5[2] = seven[PokerLib.perm7[row, 2]];
                tmp5[3] = seven[PokerLib.perm7[row, 3]];
                tmp5[4] = seven[PokerLib.perm7[row, 4]];

                var v = PokerLib.eval_5cards_fast(
                    tmp5[0].Value, tmp5[1].Value, tmp5[2].Value, tmp5[3].Value, tmp5[4].Value);

                if (v < best) { best = v; bestRow = row; }
            }

            return bestRow;
        }

        /// <summary>
        /// Copies the 5 winning cards (identified by perm7[row,*]) from seven -> dst5.
        /// </summary>
        private static void FillBest5(IReadOnlyList<Card> seven, int row, Card[] dst5)
        {
            dst5[0] = seven[PokerLib.perm7[row, 0]];
            dst5[1] = seven[PokerLib.perm7[row, 1]];
            dst5[2] = seven[PokerLib.perm7[row, 2]];
            dst5[3] = seven[PokerLib.perm7[row, 3]];
            dst5[4] = seven[PokerLib.perm7[row, 4]];
        }

        /// <summary>
        /// UI-only: stable sort by Value then Face; returns a new List&lt;Card&gt;.
        /// </summary>
        private static List<Card> SortHand(IEnumerable<Card> hand) =>
            hand.OrderBy(c => c.Value).ThenBy(c => c.Face).ToList();
    }
}
