using poker.net.Models;

namespace poker.net.Interfaces
{
    public interface IDeckService
    {
        Task<IReadOnlyList<Card>> RawDeckAsync();
    }
}
