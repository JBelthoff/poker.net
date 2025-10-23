using poker.net.Interfaces;
using poker.net.Models;

namespace poker.net.Services
{
    public class StaticDeckService : IDeckService
    {
        public Task<IReadOnlyList<Card>> RawDeckAsync()
            => Task.FromResult<IReadOnlyList<Card>>(RawDeck.All);
    }
}
