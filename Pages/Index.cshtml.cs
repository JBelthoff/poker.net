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
        public Game Game { get; set; } = new();

        public List<Card> ShuffledDeck { get; set; } = new();

        public List<List<Card>> lPlayerHands = new();

        public List<List<Card>> lWinners = new();

        public Int32 iWinValue { get; set; }

        private readonly List<Card> _tmp5 = new() { default!, default!, default!, default!, default! };

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
            if (Game == null) Game = new Game();
            if (Game.DealerID == 0) Game.DealerID = 8;

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

            Game.DealerID = (Game.DealerID % 9) + 1;
            ModelState.Clear();

        }

        public async Task DoDeal()
        {
            ShuffledDeck = DeckHelper.GetDeepCopyOfDeck([.. await _db.RawDeckAsync()]);
            DeckHelper.Shuffle(ShuffledDeck);

            Game.CardIds = DeckHelper.AssembleDeckIdsIntoString(ShuffledDeck);
            Game = await _db.RecordNewGameAsync(Game, NetworkHelper.GetIPAddress(HttpContext));

            ModelState.Clear();

            ShowTestPanel = true;
            ShowFlopPanel = true;

            _logger.LogInformation("DoDeal: Deck loaded, shuffled, and dealt");
        }

        public async Task DoFlop()
        {
            if (string.IsNullOrWhiteSpace(Game.CardIds))
            {
                _logger.LogWarning("DoFlop: No CardIDs found in bound property.");
                return;
            }

            ShuffledDeck = GetShuffledDeck(await _db.RawDeckAsync(), Game.CardIds);

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = true;

            _logger.LogInformation("DoFlop: Deck restored from hidden field");
        }

        public async Task DoTurn()
        {
            if (string.IsNullOrWhiteSpace(Game.CardIds))
            {
                _logger.LogWarning("DoTurn: No CardIDs found in bound property.");
                return;
            }

            ShuffledDeck = GetShuffledDeck(await _db.RawDeckAsync(), Game.CardIds);

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = true;

            _logger.LogInformation("DoTurn: Deck restored from hidden field");

        }

        public async Task DoRiver()
        {
            // reset UI state + arrays
            lPlayerHands.Clear();
            lWinners.Clear();
            h = new int[9];
            r = new int[9];

            if (string.IsNullOrWhiteSpace(Game.CardIds))
            {
                _logger.LogWarning("DoRiver: No CardIDs found in bound property.");
                return;
            }

            // 1) Rebuild shuffled deck from hidden field
            ShuffledDeck = GetShuffledDeck(await _db.RawDeckAsync(), Game.CardIds); // existing helper:contentReference[oaicite:3]{index=3}

            if (ShuffledDeck.Count < 23)
            {
                _logger.LogWarning("DoRiver: ShuffledDeck has {Count} cards; expected at least 23.", ShuffledDeck.Count);
                return;
            }

            // 2) Evaluate all nine players via the new engine
            var (scores, ranks, bestIdx, best5sSorted) = EvalEngine.EvaluateRiverNinePlayers(ShuffledDeck);

            // 3) Push results into your existing fields (for the view)
            for (int i = 0; i < 9; i++)
            {
                h[i] = scores[i];
                r[i] = ranks[i];
            }
            iWinIndex = bestIdx;
            lWinners = best5sSorted;

            // 4) Determine winners (support ties)
            var minVal = scores.Min();
            iWinValue = minVal;

            // 5) Panel visibility (consistent with your flow)
            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = false;
            ShowWinnerPanel = true;

            _logger.LogInformation("DoRiver: Evaluated winners using EvalEngine.");
        }


        #endregion

        #region Functions

        private static List<Card> GetShuffledDeck(IReadOnlyList<Card> deck, string cardIds)
        {
            if (deck is null)
                throw new ArgumentNullException(nameof(deck));
            if (string.IsNullOrWhiteSpace(cardIds))
                throw new ArgumentException("CardIDs cannot be null or empty.", nameof(cardIds));

            var deckLookup = deck.ToDictionary(c => c.ID);
            var shuffled = new List<Card>(deck.Count);

            foreach (var idStr in cardIds.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(idStr, out int id) || !deckLookup.TryGetValue(id, out var card))
                    throw new InvalidOperationException($"Invalid Card ID: {idStr}");

                // Deep copy (since Card is mutable)
                shuffled.Add(new Card
                {
                    ID = card.ID,
                    Face = card.Face,
                    Suit = card.Suit,
                    Color = card.Color,
                    Value = card.Value
                });
            }

            return shuffled;
        }

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

        private static int[] GetPlayersHandWinIndexes_NoAllocs(List<List<Card>> players7, int[] scratchIdx, List<Card> tmp5)
        {
            if (scratchIdx == null || scratchIdx.Length != players7.Count)
                scratchIdx = new int[players7.Count];

            for (int p = 0; p < players7.Count; p++)
            {
                var seven = players7[p];

                ushort best = ushort.MaxValue;
                int bestRow = 0;

                // Sweep the 21 combinations using perm7; reuse tmp5 and just repoint elements.
                for (int row = 0; row < 21; row++)
                {
                    tmp5[0] = seven[PokerLib.perm7[row, 0]];
                    tmp5[1] = seven[PokerLib.perm7[row, 1]];
                    tmp5[2] = seven[PokerLib.perm7[row, 2]];
                    tmp5[3] = seven[PokerLib.perm7[row, 3]];
                    tmp5[4] = seven[PokerLib.perm7[row, 4]];

                    var v = PokerLib.eval_5hand_fast_jb(tmp5);
                    if (v < best) { best = v; bestRow = row; }
                }

                scratchIdx[p] = bestRow;
            }

            return scratchIdx;
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

        private static List<Card> SortHand(IEnumerable<Card> hand)
        {
            return hand
                .OrderBy(c => c.Value)
                .ThenBy(c => c.Face) 
                .ToList();
        }

        #endregion

    }
}