using System.Text.Json.Serialization;

namespace poker.net.Models
{
    public class Game
    {
        public int DealerID { get; set; }

        public Guid GameID { get; set; }

        public string CardIds { get; set; } = string.Empty;
        
    }
}
