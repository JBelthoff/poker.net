namespace poker.net.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using poker.net.Models;

    /// <summary>
    /// Provides optimized static utility methods for working with decks of cards.
    /// Includes shuffling, cloning, and serializing deck data to compact string formats.
    /// </summary>
    public static class DeckHelper
    {
        /// <summary>
        /// Randomly shuffles a list of cards in-place using a cryptographically secure random generator.
        /// This version is suitable for cases where deterministic reproducibility is not required.
        /// </summary>
        /// <param name="cards">The list of <see cref="Card"/> objects to shuffle.</param>
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
        /// Randomly shuffles a <see cref="Card"/> array in-place using the Fisher–Yates algorithm.
        /// Uses <see cref="Random.Shared"/> for fast, thread-safe pseudo-random shuffling.
        /// </summary>
        /// <param name="a">The array of <see cref="Card"/> objects to shuffle.</param>
        public static void ShuffleInPlace(Card[] a)
        {
            // Fisher–Yates with Random.Shared; uses Span for JIT-friendly indexing
            var span = a.AsSpan();
            for (int i = span.Length - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                if (j != i)
                    (span[i], span[j]) = (span[j], span[i]);
            }
        }

        /// <summary>
        /// Converts a list of <see cref="Card"/> objects into a compact '|' (pipe)-delimited string
        /// representing the card IDs in order. Commonly used to persist or reconstruct deck order.
        /// </summary>
        /// <param name="cards">The list of cards to serialize.</param>
        /// <returns>A pipe-separated string of card IDs, or an empty string if <paramref name="cards"/> is null or empty.</returns>
        public static string AssembleDeckIdsIntoString(List<Card> cards)
        {
            if (cards is null || cards.Count == 0)
                return string.Empty;

            var sb = new StringBuilder(cards.Count * 3);
            sb.Append(cards[0].ID);
            for (int i = 1; i < cards.Count; i++)
                sb.Append('|').Append(cards[i].ID);

            return sb.ToString();
        }

        /// <summary>
        /// Converts a <see cref="Card"/> array into a compact '|' (pipe)-delimited string
        /// representing the card IDs in order. This version avoids List overhead and
        /// is optimized for performance-sensitive paths.
        /// </summary>
        /// <param name="cards">The array of cards to serialize.</param>
        /// <returns>A pipe-separated string of card IDs, or an empty string if <paramref name="cards"/> is null or empty.</returns>
        public static string AssembleDeckIdsIntoString(Card[] cards)
        {
            if (cards is null || cards.Length == 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder(cards.Length * 3);
            sb.Append(cards[0].ID);
            for (int i = 1; i < cards.Length; i++)
                sb.Append('|').Append(cards[i].ID);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a deep copy of a list of <see cref="Card"/> objects, cloning each card individually.
        /// Returns a new <see cref="List{Card}"/> instance with independent copies of all elements.
        /// </summary>
        /// <param name="cards">The source list of cards to copy.</param>
        /// <returns>A new list containing cloned <see cref="Card"/> instances, or an empty list if <paramref name="cards"/> is null.</returns>
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

        /// <summary>
        /// Creates a deep copy of a deck as a contiguous <see cref="Card"/> array.
        /// This is optimized for evaluator pipelines where contiguous memory layout
        /// and array-based iteration provide better performance than List<T>.
        /// </summary>
        /// <param name="src">The read-only source deck to copy from.</param>
        /// <returns>A new array of cloned <see cref="Card"/> objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="src"/> is null.</exception>
        public static Card[] GetDeepCopyOfDeckToArray(IReadOnlyList<Card> src)
        {
            if (src is null) throw new ArgumentNullException(nameof(src));
            var n = src.Count;
            var dst = new Card[n];
            for (int i = 0; i < n; i++)
            {
                var c = src[i];
                dst[i] = new Card
                {
                    ID = c.ID,
                    Face = c.Face,
                    Suit = c.Suit,
                    Color = c.Color,
                    Value = c.Value
                };
            }
            return dst;
        }
    }
}