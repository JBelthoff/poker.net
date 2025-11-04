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


        public static Card[] GetShuffledDeckArray(IReadOnlyList<Card> deck, string cardIds)
        {
            if (deck is null) throw new ArgumentNullException(nameof(deck));
            if (string.IsNullOrWhiteSpace(cardIds))
                throw new ArgumentException("CardIDs cannot be null or empty.", nameof(cardIds));

            var lookup = new Dictionary<int, Card>(deck.Count);
            for (int i = 0; i < deck.Count; i++) lookup[deck[i].ID] = deck[i];

            var ids = cardIds.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var shuffled = new Card[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                if (!int.TryParse(ids[i], out int id) || !lookup.TryGetValue(id, out var c))
                    throw new InvalidOperationException($"Invalid Card ID: {ids[i]}");

                shuffled[i] = new Card
                {
                    ID = c.ID,
                    Face = c.Face,
                    Suit = c.Suit,
                    Color = c.Color,
                    Value = c.Value
                };
            }
            return shuffled;
        }

    }
}