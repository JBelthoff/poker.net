namespace poker.net.Helper
{
    using poker.net.Models;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Provides utility methods for working with decks of cards,
    /// including shuffling, deep copying, and serializing IDs.
    /// </summary> 
    public static class DeckHelper
    {
        /// <summary>
        /// Randomly shuffles a list of cards using a cryptographically secure random generator.
        /// </summary>
        /// <param name="cards">The list of cards to shuffle.</param>
        public static void Shuffle(List<Card> cards)
        {
            if (cards is null || cards.Count < 2)
                return;

            using var rng = RandomNumberGenerator.Create();
            int n = cards.Count;

            while (n > 1)
            {
                Span<byte> box = stackalloc byte[1];
                do rng.GetBytes(box);
                while (box[0] >= n * (byte.MaxValue / n));

                int k = box[0] % n;
                n--;

                (cards[k], cards[n]) = (cards[n], cards[k]); // tuple swap
            }
        }

        /// <summary>
        /// Returns a string of card IDs separated by the pipe (|) character.
        /// </summary>
        /// <param name="cards">The list of cards.</param>
        /// <returns>A '|'-separated string of card IDs.</returns>
        public static string AssembleDeckIdsIntoString(List<Card> cards)
        {
            if (cards is null || cards.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();
            for (int i = 0; i < cards.Count; i++)
            {
                builder.Append(cards[i].ID);
                if (i < cards.Count - 1)
                    builder.Append('|');
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates a deep copy of the deck where each card is cloned individually.
        /// </summary>
        public static List<Card> GetDeepCopyOfDeck(List<Card> cards)
        {
            if (cards is null)
                return [];

            var copy = new List<Card>(cards.Count);
            foreach (var c in cards)
            {
                copy.Add(new Card
                {
                    ID = c.ID,
                    Color = c.Color,
                    Face = c.Face,
                    Suit = c.Suit,
                    Value = c.Value
                });
            }
            return copy;
        }
    }
}
