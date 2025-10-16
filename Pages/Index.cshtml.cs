using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using poker.net.Models;
using poker.net.Services;

namespace poker.net.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DbHelper _db;

        public IReadOnlyList<Card> Deck { get; private set; } = [];

        public IndexModel(ILogger<IndexModel> logger, DbHelper db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task OnGetAsync()
        {
            Deck = await _db.RawDeckAsync();
        }
    }
}
