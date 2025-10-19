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
        public int DealerID { get; set; } = 4;

        [BindProperty]
        public string CardIDs { get; set; }

        //public string sWinners { get; set; }

        public IReadOnlyList<Card> Deck { get; set; } = Array.Empty<Card>();

        public List<Card> ShuffledDeck { get; set; } = new();

        public List<List<Card>> lPlayerHands = new();

        public List<List<Card>> lWinners = new();

        public Int32 iWinValue { get; set; }



        //public List<List<Card>> BestHands { get; set; } = new();   // 9 x 5
        //public bool[] IsWinner { get; set; } = new bool[9];        // handles ties
        //public string[] HandNames { get; set; } = new string[9];   // "Pair", "2 Pair", etc.
        //public int[] BestHandIndex { get; set; } = new int[9];     // optional: for debugging





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
            iWinIndex = GetPlayersHandWinIndexes(lPlayerHands);

            // Get Table Winner
            h = new Int32[9];
            r = new Int32[9];
            for (Int32 i = 0; i < lPlayerHands.Count; i++)
            {
                h[i] = PokerLib.eval_5hand_fast_jb(GetSubHand(lPlayerHands[i], iWinIndex[i]));
                r[i] = PokerLib.hand_rank_jb(h[i]);
            }

            //
            //lWinners = new List<List<Card>>();
            for (Int32 i = 0; i < lPlayerHands.Count; i++)
                lWinners.Add(GetSubHand(lPlayerHands[i], iWinIndex[i]));

            iWinValue = h.Min();


            //

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = false;
            ShowWinnerPanel = true;

            _logger.LogInformation("DoRiver: Deck restored from hidden field ({Count} cards).", ShuffledDeck.Count);

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

        private List<Card> GetSubHand(List<Card> l, Int32 iIndex)
        {
            List<Card> lSubHand = new List<Card>();
            for (Int32 j = 0; j < 5; j++)
                lSubHand.Add(l[PokerLib.perm7[iIndex, j]]);

            return lSubHand;
        }

        private String GetRiverDisplay(List<List<Card>> l, Int32[] iWIndex)
        {
            // Get Only the Winning Hands of Each Player
            List<List<Card>> lWinners = new List<List<Card>>();
            for (Int32 i = 0; i < l.Count; i++)
                lWinners.Add(GetSubHand(l[i], iWIndex[i]));

            // Get the Winning Value of the Winning Hand. There maybe ties...!
            Int32 iWinValue = h.Min();

            // Loop the results into a string and return it.
            StringBuilder objStr = new StringBuilder();
            objStr.AppendLine("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">");

            objStr.AppendLine("  <tr>");
            for (Int32 i = 0; i < lWinners.Count; i++)
            {
                if (h[PokerLib.DealOrder[DealerID, i]].Equals(iWinValue))
                    objStr.Append("    <th style=\"background-color: #bfb;\">");
                else
                    objStr.Append("    <th>");

                if (h[PokerLib.DealOrder[DealerID, i]].Equals(iWinValue))
                    objStr.Append("WINNER!");
                else
                {
                    objStr.Append("Player");
                    objStr.Append(" ");
                    objStr.Append((i + 1));
                }

                objStr.AppendLine("</th>");
            }
            objStr.AppendLine("  </tr>");

            objStr.AppendLine("  <tr>");
            for (Int32 i = 0; i < 9; i++)
            {
                objStr.Append("    <td align=\"center\">");
                for (Int32 x = 0; x < lWinners[i].Count; x++)
                {
                    objStr.Append("<span style=\"color:#");
                    objStr.Append(lWinners[PokerLib.DealOrder[DealerID, i]][x].Color);
                    objStr.Append(";\">");
                    objStr.Append(lWinners[PokerLib.DealOrder[DealerID, i]][x].Face);
                    objStr.Append(lWinners[PokerLib.DealOrder[DealerID, i]][x].Suit);
                    objStr.Append("</span>");
                    if (x < lWinners[i].Count - 1)
                        objStr.Append(" ");
                }
                objStr.AppendLine("</td>");
            }
            objStr.AppendLine("  </tr>");

            objStr.AppendLine("  <tr>");
            for (Int32 i = 0; i < 9; i++)
            {
                if (h[PokerLib.DealOrder[DealerID, i]].Equals(iWinValue))
                    objStr.Append("    <td align=\"center\" style=\"background-color: #bfb;\">");
                else
                    objStr.Append("    <td align=\"center\">");
                if (h[PokerLib.DealOrder[DealerID, i]].Equals(iWinValue))
                    objStr.Append("<strong>");
                if (h[PokerLib.DealOrder[DealerID, i]].Equals(1))
                    objStr.Append("Royal Flush");
                else
                {
                    if (!h[PokerLib.DealOrder[DealerID, i]].Equals(iWinValue))
                        objStr.Append("<em>");
                    objStr.Append(GetNameOfHand(r[PokerLib.DealOrder[DealerID, i]]));
                    if (!h[PokerLib.DealOrder[DealerID, i]].Equals(iWinValue))
                        objStr.Append("</em>");
                }
                if (h[PokerLib.DealOrder[DealerID, i]].Equals(iWinValue))
                    objStr.Append("</strong>");
                objStr.AppendLine("</td>");
            }
            objStr.AppendLine("  </tr>");

            objStr.AppendLine("</table>");

            return objStr.ToString();

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

    }
}