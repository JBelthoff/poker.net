using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using poker.net.Helper;
using poker.net.Models;
using poker.net.Services;
using System;
using System.Diagnostics;
using System.Text;

namespace poker.net.Pages
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly ILogger<IndexModel> _logger;
        private readonly DbHelper _db;

        public int[] h, r;

        public bool ShowShufflePanel { get; set; }

        public bool ShowDealPanel { get; set; }

        public bool ShowFlopPanel { get; set; }

        public bool ShowTurnPanel { get; set; }

        public bool ShowRiverPanel { get; set; }

        public bool ShowWinnerPanel { get; set; }

        public bool ShowTestPanel { get; set; }

        [BindProperty]
        public int DealerID { get; set; } = 8;

        [BindProperty]
        public string CardIDs { get; set; }

        public IReadOnlyList<Card> Deck { get; set; } = Array.Empty<Card>();

        public List<Card> ShuffledDeck { get; set; } = new();

        public List<List<Card>> lPlayerHands = new();

        #endregion

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
            ShowWinnerPanel = false;
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

                case "turn":
                    await DoTurn();
                    break;

                case "river":
                    await DoRiver();
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

            // Get Players Hands
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

            if (string.IsNullOrWhiteSpace(CardIDs))
            {
                _logger.LogWarning("DoFlop: No CardIDs found in bound property.");
                return;
            }

            Deck = await _db.RawDeckAsync();
            var deckLookup = Deck.ToDictionary(c => c.ID);

            ShuffledDeck = CardIDs
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(idStr =>
                {
                    if (int.TryParse(idStr, out int id) && deckLookup.TryGetValue(id, out var card))
                        return card;
                    return null;
                })
                .Where(c => c != null)
                .ToList()!;

            // Get Players Hands
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

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = true;

            _logger.LogInformation("DoFlop: Deck restored from hidden field ({Count} cards).", ShuffledDeck.Count);
        }

        public async Task DoTurn()
        {
            if (string.IsNullOrWhiteSpace(CardIDs))
            {
                _logger.LogWarning("DoFlop: No CardIDs found in bound property.");
                return;
            }

            Deck = await _db.RawDeckAsync();
            var deckLookup = Deck.ToDictionary(c => c.ID);

            ShuffledDeck = CardIDs
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(idStr =>
                {
                    if (int.TryParse(idStr, out int id) && deckLookup.TryGetValue(id, out var card))
                        return card;
                    return null;
                })
                .Where(c => c != null)
                .ToList()!;

            // Get Players Hands
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

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = true;

            _logger.LogInformation("DoTurn: Deck restored from hidden field ({Count} cards).", ShuffledDeck.Count);

        }

        public async Task DoRiver()
        {
            if (string.IsNullOrWhiteSpace(CardIDs))
            {
                _logger.LogWarning("DoFlop: No CardIDs found in bound property.");
                return;
            }

            Deck = await _db.RawDeckAsync();
            var deckLookup = Deck.ToDictionary(c => c.ID);

            ShuffledDeck = CardIDs
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(idStr =>
                {
                    if (int.TryParse(idStr, out int id) && deckLookup.TryGetValue(id, out var card))
                        return card;
                    return null;
                })
                .Where(c => c != null)
                .ToList()!;

            // Get Players Hands
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

            // The Index of the winnign hands from each players hand
            Int32[] iWinIndex = GetPlayersHandWinIndexes(lPlayerHands);

            // Get Table Winner
            h = new Int32[9];
            r = new Int32[9];
            for (Int32 i = 0; i < lPlayerHands.Count; i++)
            {
                h[i] = PokerLib.eval_5hand_fast_jb(GetSubHand(lPlayerHands[i], iWinIndex[i]));
                r[i] = PokerLib.hand_rank_jb(h[i]);
            }

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = false;
            ShowWinnerPanel = true;

            _logger.LogInformation("DoRiver: Deck restored from hidden field ({Count} cards).", ShuffledDeck.Count);

        }



        private Int32[] GetPlayersHandWinIndexes(List<List<Card>> l)
        {
            Trace.Write("GetPlayersHandWinIndexes()", "Started");
            Int32[] iWinIndex = new Int32[l.Count];
            for (Int32 x = 0; x < l.Count; x++)
            {
                UInt16[] iHandValues = new UInt16[21];
                List<List<Card>> lSubHands = new List<List<Card>>();
                for (Int32 i = 0; i < 21; i++)
                {
                    List<Card> lSubHand = new List<Card>();
                    for (Int32 j = 0; j < 5; j++)
                        lSubHand.Add(l[x][PokerLib.perm7[i, j]]);
                    lSubHands.Add(lSubHand);
                    iHandValues[i] = PokerLib.eval_5hand_fast_jb(lSubHand);
                }
                iWinIndex[x] = iHandValues.ToList().IndexOf(iHandValues.Min());
            }
            Trace.Write("GetPlayersHandWinIndexes()", "Returning");
            return iWinIndex;
        }

        private List<Card> GetSubHand(List<Card> l, Int32 iIndex)
        {
            List<Card> lSubHand = new List<Card>();
            for (Int32 j = 0; j < 5; j++)
                lSubHand.Add(l[PokerLib.perm7[iIndex, j]]);

            return lSubHand;
        }

    }
}