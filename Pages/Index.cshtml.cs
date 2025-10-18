using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using poker.net.Helper;
using poker.net.Models;
using poker.net.Services;
using System;
using System.Text;

namespace poker.net.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DbHelper _db;

        public bool ShowShufflePanel { get; private set; }

        public bool ShowDealPanel { get; private set; }

        public bool ShowFlopPanel { get; private set; }

        public bool ShowTurnPanel { get; private set; }

        public bool ShowRiverPanel { get; private set; }
        
        public bool ShowTestPanel { get; private set; }

        public int DealerID { get; private set; }

        public string CardIDs { get; private set; }

        public IReadOnlyList<Card> Deck { get; private set; } = Array.Empty<Card>();

        public List<Card> ShuffledDeck { get; private set; } = new();
                
        public IndexModel(ILogger<IndexModel> logger, DbHelper db)
        {
            _logger = logger;
            _db = db;
        }

        public void OnGet()
        {
            // Show pnlDeal on GET, hide pnlTest
            ShowShufflePanel = true;
            ShowDealPanel = false;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = false;
            ShowTestPanel = false;
            DealerID = 8;

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var action = Request.Form["action"].ToString().ToLowerInvariant();

            switch (action)
            {
                case "shuffle":
                    ShowShufflePanel = true;
                    ShowDealPanel = false;
                    ShowFlopPanel = false;
                    ShowTurnPanel = false;
                    ShowRiverPanel = false;
                    ShowTestPanel = false;

                    DealerID += Convert.ToInt32(Request.Form["dealerid"]);
                    DealerID += 1;
                    if (DealerID > 9)
                    {
                        DealerID = 1;
                    }
                    break;

                case "deal":
                    DealerID += Convert.ToInt32(Request.Form["dealerid"]);
                    await DoDeal(); 
                    break;

                case "flop":
                    DealerID += Convert.ToInt32(Request.Form["dealerid"]);
                    await DoFlop();
                    break;

                    // other actions
                    // case "shuffle":
                    //     await DoShuffle();
                    //     break;
            }

            return Page();
        }

        public async Task DoDeal()
        {
            Deck = await _db.RawDeckAsync();
            ShuffledDeck = DeckHelper.GetDeepCopyOfDeck([.. Deck]);
            DeckHelper.Shuffle(ShuffledDeck);
            
            CardIDs = DeckHelper.AssembleDeckIdsIntoString(ShuffledDeck);
            await _db.RecordNewGameAsync(CardIDs, NetworkHelper.GetIPAddress(HttpContext));

            ShowTestPanel = true;
            ShowFlopPanel = true;

            _logger.LogInformation("DoDeal: Deck loaded & shuffled ({Count} cards).", ShuffledDeck.Count);
        }

        public async Task DoFlop()
        {
            Deck = await _db.RawDeckAsync();
            ShuffledDeck = DeckHelper.GetDeepCopyOfDeck([.. Deck]);
            DeckHelper.Shuffle(ShuffledDeck);
            ShowTestPanel = true;
            

            _logger.LogInformation("DoDeal: Deck loaded & shuffled ({Count} cards).", ShuffledDeck.Count);
        }




        

        //public async Task<PartialViewResult> OnGetDealAsync()
        //{
        //    // 1) Get raw deck (cached/DB)
        //    Deck = await _db.RawDeckAsync();

        //    // 2) Deep copy, then shuffle
        //    ShuffledDeck = DeckHelper.GetDeepCopyOfDeck([.. Deck]);
        //    DeckHelper.Shuffle(ShuffledDeck);

        //    _logger.LogInformation("Index.OnGetDealAsync: Loaded deck & Shuffled (Count={Count})", ShuffledDeck.Count);

        //    // Return partial HTML for the "Deck Shuffled" section
        //    return new PartialViewResult
        //    {
        //        ViewName = "_ShuffledDeck",
        //        ViewData = new ViewDataDictionary<List<Card>>(ViewData, ShuffledDeck)
        //    };
        //}

        //public async Task OnGetAsync()
        //{
        //    // 1) Get raw deck (cached/DB)
        //    Deck = await _db.RawDeckAsync();

        //    // 2) Deep copy, then shuffle
        //    ShuffledDeck = DeckHelper.GetDeepCopyOfDeck([.. Deck]);
        //    DeckHelper.Shuffle(ShuffledDeck);

        //    _logger.LogInformation("Index.OnGetAsync: Loaded deck & Shuffled");
        //}

    }



}
