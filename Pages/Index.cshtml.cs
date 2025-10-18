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

        public bool ShowShufflePanel { get; set; }

        public bool ShowDealPanel { get; set; }

        public bool ShowFlopPanel { get; set; }

        public bool ShowTurnPanel { get; set; }

        public bool ShowRiverPanel { get; set; }
        
        public bool ShowTestPanel { get; set; }

        [BindProperty]
        public int DealerID { get; set; } = 8;

        public string CardIDs { get; set; }

        public IReadOnlyList<Card> Deck { get; set; } = Array.Empty<Card>();

        public List<Card> ShuffledDeck { get; set; } = new();

        public List<List<Card>> lPlayerHands = new();

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
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var action = Request.Form["action"].ToString().ToLowerInvariant();

            switch (action)
            {
                case "shuffle":
                    DoShuffle();
                    ModelState.Remove(nameof(DealerID));
                    break;

                case "deal":
                    await DoDeal(); 
                    break;

                case "flop":
                    await DoFlop();
                    break;

                    // other actions
                    // case "shuffle":
                    //     await DoShuffle();
                    //     break;
            }

            return Page();
        }


        private void DoShuffle()
        {
            ShowShufflePanel = true;
            ShowDealPanel = false;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = false;
            ShowTestPanel = false;

            DealerID = (DealerID % 9) + 1;
            
        }



        public async Task DoDeal()
        {
            Deck = await _db.RawDeckAsync();
            ShuffledDeck = DeckHelper.GetDeepCopyOfDeck([.. Deck]);
            DeckHelper.Shuffle(ShuffledDeck);

            CardIDs = DeckHelper.AssembleDeckIdsIntoString(ShuffledDeck);

            // Use bound DealerID; no Request.Form
            await _db.RecordNewGameAsync(CardIDs, NetworkHelper.GetIPAddress(HttpContext));

            // Deal the Cards
            List<Card> lTemp;
            for (Int32 i = 0; i < 9; i++)
            {
                lTemp = new List<Card>();
                lTemp.Add(ShuffledDeck[i]);
                lTemp.Add(ShuffledDeck[i + 9]);
                for (Int32 x = 18; x < 23; x++)
                    lTemp.Add(ShuffledDeck[x]);
                lPlayerHands.Add(lTemp);
            }

            // show panels that follow a deal
            ShowTestPanel = true;
            ShowFlopPanel = true;

            _logger.LogInformation("DoDeal: Deck loaded & shuffled ({Count} cards).", ShuffledDeck.Count);
        }
        

        public async Task DoFlop()
        {
            //DealerID += Convert.ToInt32(Request.Form["dealerid"]);


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
