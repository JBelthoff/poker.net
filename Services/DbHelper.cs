using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using poker.net.Models;
using System.Data;

namespace poker.net.Services
{
    /// <summary>
    /// Provides database operations using Dapper for the poker.net application.
    /// </summary>
    public class DbHelper
    {
        private readonly IDbConnection _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DbHelper> _logger;

        private const string RawDeckCacheKey = "DbHelper.RawDeck";

        public DbHelper(IDbConnection db, IMemoryCache cache, ILogger<DbHelper> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the full deck of cards from the database or cache.
        /// </summary>
        public async Task<IReadOnlyList<Card>> RawDeckAsync(CancellationToken ct = default)
        {
            _logger.LogDebug("DbHelper.RawDeckAsync: Start");

            if (_cache.TryGetValue(RawDeckCacheKey, out IReadOnlyList<Card>? cachedDeck) && cachedDeck is not null)
            {
                _logger.LogInformation("DbHelper.RawDeckAsync: Returning deck from cache");
                return cachedDeck;
            }

            if (_db.State != ConnectionState.Open)
                await (_db as SqlConnection)!.OpenAsync(ct);

            try
            {
                var cards = (await _db.QueryAsync<Card>(
                    "dbo.GameDeck_GetRawDeck",
                    commandType: CommandType.StoredProcedure
                )).AsList();

                _cache.Set(RawDeckCacheKey, cards, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });

                _logger.LogInformation("DbHelper.RawDeckAsync: Deck loaded from database and cached");

                return cards;
            }
            finally
            {
                if (_db.State == ConnectionState.Open)
                    _db.Close();
            }
        }

        /// <summary>
        /// Inserts a new game record into the database and returns its GameID.
        /// </summary>
        public async Task<Game> RecordNewGameAsync(Game game, string ip, CancellationToken ct = default)
        {
            _logger.LogDebug("DbHelper.RecordNewGameAsync: Start");

            if (game is null)
                throw new ArgumentNullException(nameof(game));

            var parameters = new DynamicParameters();
            parameters.Add("@Return", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
            parameters.Add("@CreateIP", ip, DbType.String, size: 100, direction: ParameterDirection.Input);
            parameters.Add("@Array", game.CardIds, DbType.String, size: 8000, direction: ParameterDirection.Input);
            parameters.Add("@GameID", dbType: DbType.Guid, direction: ParameterDirection.Output);

            if (_db.State != ConnectionState.Open)
                await (_db as SqlConnection)!.OpenAsync(ct);

            try
            {
                await _db.ExecuteAsync(
                    "dbo.Game_InsertNewGame2",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                game.GameID = parameters.Get<Guid>("@GameID");
                _logger.LogInformation(
                    "DbHelper.RecordNewGameAsync: New game recorded with ID {GameId} (Dealer {DealerID})",
                    game.GameID, game.DealerID
                );

                return game;
            }
            finally
            {
                if (_db.State == ConnectionState.Open)
                    _db.Close();
            }
        }

    }
}
