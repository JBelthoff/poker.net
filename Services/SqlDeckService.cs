using poker.net.Interfaces;
using poker.net.Models;

namespace poker.net.Services
{
    public class SqlDeckService : IDeckService
    {
        private readonly DbHelper _db;

        public SqlDeckService(DbHelper db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Card>> RawDeckAsync()
        {
            return await _db.RawDeckAsync();
        }
    }
}
