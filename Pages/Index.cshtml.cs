using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using poker.net.Helper;
using poker.net.Models;
using poker.net.Services;

namespace poker.net.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DbHelper _db;

        public IReadOnlyList<Card> Deck { get; private set; } = Array.Empty<Card>();
        public List<Card> ShuffledDeck { get; private set; } = new();

        public IndexModel(ILogger<IndexModel> logger, DbHelper db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task OnGetAsync()
        {
            // 1) Get raw deck (cached/DB)
            Deck = await _db.RawDeckAsync();

            // 2) Deep copy, then shuffle
            ShuffledDeck = DeckHelper.GetDeepCopyOfDeck([.. Deck]);
            DeckHelper.Shuffle(ShuffledDeck);

            _logger.LogInformation("Index.OnGetAsync: Loaded deck ({Count}), shuffled copy.", Deck.Count);
        }

    }
}
