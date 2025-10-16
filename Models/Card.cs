namespace poker.net.Models
{
    /// <summary>
    /// Represents a playing card with color, face, suit, and numeric value.
    /// </summary>
    [Serializable]
    public class Card
    {
        /// <summary>
        /// Gets or sets the unique identifier for the card.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the color of the card (e.g., Red or Black).
        /// </summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the face of the card (e.g., Ace, King, Queen, Jack).
        /// </summary>
        public string Face { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the suit of the card (e.g., Hearts, Spades, Clubs, Diamonds).
        /// </summary>
        public string Suit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the numeric value of the card.
        /// </summary>
        public int Value { get; set; }
    }
}
