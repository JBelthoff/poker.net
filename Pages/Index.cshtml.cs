using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using poker.net.Helper;
using poker.net.Models;
using poker.net.Services;

namespace poker.net.Pages
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly ILogger<IndexModel> _logger;
        
        private readonly DbHelper _db;

        public Int32[] h { get; set; }

        public Int32[] r { get; set; }

        public Int32[] iWinIndex { get; set; }

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

        public List<List<Card>> lWinners = new();

        public Int32 iWinValue { get; set; }

        #endregion

        #region Contructor

        public IndexModel(ILogger<IndexModel> logger, DbHelper db)
        {
            _logger = logger;
            _db = db;
        }

        #endregion

        #region Methods

        public void OnGet()
        {
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

            await _db.RecordNewGameAsync(CardIDs, NetworkHelper.GetIPAddress(HttpContext));

            ShowTestPanel = true;
            ShowFlopPanel = true;

            _logger.LogInformation("DoDeal: Deck loaded, shuffled, and dealt");
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

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = true;

            _logger.LogInformation("DoFlop: Deck restored from hidden field");
        }

        public async Task DoTurn()
        {
            if (string.IsNullOrWhiteSpace(CardIDs))
            {
                _logger.LogWarning("DoTurn: No CardIDs found in bound property.");
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

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = true;

            _logger.LogInformation("DoTurn: Deck restored from hidden field");

        }

        public async Task DoRiver()
        {
            lPlayerHands.Clear();
            lWinners.Clear();
            h = new int[9];
            r = new int[9];

            if (string.IsNullOrWhiteSpace(CardIDs))
            {
                _logger.LogWarning("DoRiver: No CardIDs found in bound property.");
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
                .OfType<Card>() 
                .ToList();

            if (ShuffledDeck.Count < 23)
            {
                _logger.LogWarning("DoRiver: ShuffledDeck has {Count} cards; expected at least 23.", ShuffledDeck.Count);
                return;
            }

            for (int i = 0; i < 9; i++)
            {
                var hand = new List<Card>(7)
                {
                    ShuffledDeck[i],
                    ShuffledDeck[i + 9]
                };
                for (int x = 18; x < 23; x++)
                    hand.Add(ShuffledDeck[x]);

                lPlayerHands.Add(hand);
            }

            // 1) For each 7-card hand, find best 5-card sub-hand index (0..20)
            iWinIndex = GetPlayersHandWinIndexes(lPlayerHands);

            // 2) Evaluate each player's best hand -> h (score), r (category)
            // 3) Extract and store the actual best 5-card hands (for display): lWinners
            lWinners = new List<List<Card>>(capacity: 9);
            for (int i = 0; i < lPlayerHands.Count; i++)
            {
                var best5 = GetSubHand(lPlayerHands[i], iWinIndex[i]);
                h[i] = PokerLib.eval_5hand_fast_jb(best5);
                r[i] = PokerLib.hand_rank_jb(h[i]);
                lWinners.Add(best5);
            }

            // 4) Lowest eval value wins (ties are possible)
            iWinValue = h.Min();

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = false;
            ShowWinnerPanel = true;

            _logger.LogInformation("DoRiver: Deck restored from hidden field");
        }

        #endregion

        #region Functions

        private Int32[] GetPlayersHandWinIndexes(List<List<Card>> l)
        {
            iWinIndex = new Int32[l.Count];
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
            return iWinIndex;
        }

        private List<Card> GetSubHand(List<Card> l, Int32 iIndex)
        {
            List<Card> lSubHand = new List<Card>();
            for (Int32 j = 0; j < 5; j++)
                lSubHand.Add(l[PokerLib.perm7[iIndex, j]]);

            return lSubHand;
        }

        public string GetNameOfHand(int iRank)
        {
            String sReturn = String.Empty;
            switch (iRank)
            {
                case 1:
                    sReturn = ("Straight Flush");
                    break;
                case 2:
                    sReturn = ("4 of a Kind");
                    break;
                case 3:
                    sReturn = ("Full House");
                    break;
                case 4:
                    sReturn = ("Flush");
                    break;
                case 5:
                    sReturn = ("Straight");
                    break;
                case 6:
                    sReturn = ("3 of a Kind");
                    break;
                case 7:
                    sReturn = ("2 Pair");
                    break;
                case 8:
                    sReturn = ("Pair");
                    break;
                case 9:
                    sReturn = ("High Card");
                    break;
            }
            return sReturn;
        }

        #endregion

    }
}