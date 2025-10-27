using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using poker.net.Helper;
using poker.net.Interfaces;
using poker.net.Models;
using poker.net.Services;

namespace poker.net.Pages
{
    public class IndexModel : PageModel 
    {
        #region Properties
        private readonly ILogger<IndexModel> _logger;
        
        private readonly DbHelper? _db;

        private readonly IDeckService _deckService;

        private readonly bool _useSql;

        public int[] h { get; set; }

        public int[] r { get; set; }

        //public int[] iWinIndex { get; set; }

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

        //public List<List<Card>> lPlayerHands = new();

        public List<List<Card>> lWinners = new();

        public Int32 iWinValue { get; set; }

        #endregion

        #region Contructor

        public IndexModel(ILogger<IndexModel> logger, IDeckService deckService, IConfiguration config, DbHelper? db = null)
        {
            _logger = logger;
            _deckService = deckService;
            _useSql = config.GetValue<bool>("UseSqlServer");
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
            var isTestMode = false;
            const int SelectedWinIndex = 6; // See GetFixedDeck() for values
            var sourceDeck = await _deckService.RawDeckAsync();

            if (!isTestMode)
            {
                var shuffled = DeckHelper.GetDeepCopyOfDeckToArray(sourceDeck);
                DeckHelper.ShuffleInPlace(shuffled);
                Game.CardIds = DeckHelper.AssembleDeckIdsIntoString(shuffled);

                // Maintain UI compatibility
                ShuffledDeck = new List<Card>(shuffled);

                if (_useSql && _db is not null)
                    Game = await _db.RecordNewGameAsync(Game, NetworkHelper.GetIPAddress(HttpContext));
                else
                    Game.GameID = Guid.NewGuid();
            }
            else
            {
                Game.CardIds = GetFixedDeck(SelectedWinIndex);
                var arr = GetShuffledDeckArray(sourceDeck, Game.CardIds);
                ShuffledDeck = new List<Card>(arr);
            }

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

            var source = await _deckService.RawDeckAsync();
            var deckArr = GetShuffledDeckArray(source, Game.CardIds);

            // Maintain UI compatibility
            ShuffledDeck = new List<Card>(deckArr);

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = true;

            _logger.LogInformation("DoFlop (array path): Deck restored from hidden field.");
        }

        public async Task DoTurn()
        {
            if (string.IsNullOrWhiteSpace(Game.CardIds))
            {
                _logger.LogWarning("DoTurn: No CardIDs found in bound property.");
                return;
            }

            var source = await _deckService.RawDeckAsync();
            var deckArr = GetShuffledDeckArray(source, Game.CardIds);

            // Maintain UI compatibility
            ShuffledDeck = new List<Card>(deckArr);

            ShowTestPanel = true;
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = true;

            _logger.LogInformation("DoTurn (array path): Deck restored from hidden field.");
        }

        public async Task DoRiver()
        {
            // --- Reset per-run state that feeds the UI ---
            //lPlayerHands.Clear();         // legacy: not used by the array evaluator, but keep cleared for UI
            lWinners.Clear();             // we'll rebuild this from the array results below
            h = new int[9];               // per-player raw scores (lower is better)
            r = new int[9];               // per-player rank category (1 = Straight Flush ... 9 = High Card)

            // --- Sanity: we must have a persisted shuffle order from earlier steps ---
            if (string.IsNullOrWhiteSpace(Game.CardIds))
            {
                _logger.LogWarning("DoRiver: No CardIDs found in bound property.");
                return;
            }

            // --- 1) Rebuild the shuffled deck as a contiguous array (fast path for evaluation) ---
            // We use the immutable source deck to deep-copy by ID into a Card[] according to Game.CardIds.
            var source = await _deckService.RawDeckAsync();
            var deckArr = GetShuffledDeckArray(source, Game.CardIds);

            // Also keep the existing Razor view contract that binds to a List<Card>.
            ShuffledDeck = new List<Card>(deckArr);

            // Minimum cards required:
            //   9 players × 2 hole = 18
            // + 5 board            = 23
            if (deckArr.Length < 23)
            {
                _logger.LogWarning("DoRiver: ShuffledDeck has {Count} cards; expected at least 23.", deckArr.Length);
                return;
            }

            // --- 2) Evaluate the full 9-player river using the zero-allocation array path ---
            // Returns:
            //  - scores   : ushort[9] raw evaluator scores (lower = stronger hand)
            //  - ranks    : int[9] rank categories (1..9) for display like "Straight", "Flush", etc.
            //  - bestHands: Card[9][] where bestHands[i] is the sorted 5-card best hand for player i (or null)
            var (scores, ranks, bestHandsArr) =
                EvalEngine.EvaluateRiverNinePlayersArrays(deckArr, includeBestHands: true);

            // --- 3) Rehydrate lWinners for the view (List<List<Card>>) and sort each hand for stable display ---
            lWinners = new List<List<Card>>(capacity: 9);
            for (int i = 0; i < 9; i++)
            {
                // Convert Card[] → List<Card> for Razor; empty list for null entries.
                var best = (bestHandsArr is { Length: >= 9 } && bestHandsArr[i] is not null)
                    ? new List<Card>(bestHandsArr[i])
                    : new List<Card>(capacity: 0);

                // Sort with A-low straight ("wheel") handling and suit tiebreaks.
                // This uses your existing helpers: IsWheelA2345 + SuitOrder via SortWinnerInPlace().
                SortWinnerInPlace(best);
                lWinners.Add(best);
            }

            // --- 4) Project numeric results into the arrays the view expects ---
            for (int i = 0; i < 9; i++)
            {
                h[i] = scores[i];
                r[i] = ranks[i];
            }

            //iWinIndex = bestIdx;

            // Lowest score is the winning hand value (for your existing UI).
            iWinValue = scores.Min();

            // --- 5) Flip UI panels: hide steps, show winner panel ---
            ShowTestPanel = true;   // keep the test panel visible through the flow
            ShowFlopPanel = false;
            ShowTurnPanel = false;
            ShowRiverPanel = false;
            ShowWinnerPanel = true;

            _logger.LogInformation("DoRiver: Evaluated winners using EvalEngine (array path).");
        }

        #endregion

        #region Functions

        private static Card[] GetShuffledDeckArray(IReadOnlyList<Card> deck, string cardIds)
        {
            if (deck is null) throw new ArgumentNullException(nameof(deck));
            if (string.IsNullOrWhiteSpace(cardIds))
                throw new ArgumentException("CardIDs cannot be null or empty.", nameof(cardIds));

            var lookup = new Dictionary<int, Card>(deck.Count);
            for (int i = 0; i < deck.Count; i++) lookup[deck[i].ID] = deck[i];

            var ids = cardIds.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var shuffled = new Card[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                if (!int.TryParse(ids[i], out int id) || !lookup.TryGetValue(id, out var c))
                    throw new InvalidOperationException($"Invalid Card ID: {ids[i]}");

                shuffled[i] = new Card
                {
                    ID = c.ID,
                    Face = c.Face,
                    Suit = c.Suit,
                    Color = c.Color,
                    Value = c.Value
                };
            }
            return shuffled;
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

        private static string GetFixedDeck(int selectedWin)
        {
            // Helper: compose a full 52-card deck string from a 23-card prefix
            static string ComposeFromTop23(IReadOnlyList<int> top23)
            {
                var used = new HashSet<int>(top23);
                var rest = new List<int>(52 - top23.Count);
                for (int id = 1; id <= 52; id++)
                    if (!used.Contains(id)) rest.Add(id);

                var all = new List<int>(52);
                all.AddRange(top23);
                all.AddRange(rest);
                return string.Join('|', all);
            }

            // Build first 23 cards: (P1–P9 hole 1, P1–P9 hole 2, 5 board cards)
            // P1 is the intended winner; case 11 ties P1–P3 with identical hands.
            List<int> t;

            switch (selectedWin)
            {
                // 0 = Royal Flush (Spades) — P1 only
                case 0:
                    t = new()
                    {
                        1, 6, 14, 18, 22, 26, 30, 15, 19,
                        49, 35, 31, 47, 51, 34, 8, 10, 46,
                        45, 41, 37, 7, 11
                    };
                    return ComposeFromTop23(t);

                // 1 = Straight Flush (Hearts 4–8) — P1 only
                case 1:
                    t = new()
                    {
                        30, 8, 12, 19, 23, 27, 31, 48, 16,
                        26, 35, 47, 51, 36, 43, 39, 31, 3
                    };
                    t[16] = 40; // fix duplicate (use 10♣)
                    t[17] = 3;  // P9 second card
                    t.AddRange(new[] { 22, 18, 14, 52, 15 });
                    return ComposeFromTop23(t);

                // 2 = Four of a Kind (Kings) — P1 only
                case 2:
                    t = new()
                    {
                        51, 12, 19, 27, 36, 43, 8, 16, 28,
                        52, 15, 24, 32, 47, 39, 11, 23, 31,
                        49, 50, 35, 20, 7
                    };
                    return ComposeFromTop23(t);

                // 3 = Full House (Aces over Sevens) — P1 only
                case 3:
                    t = new()
                    {
                        3, 51, 43, 39, 47, 36, 32, 52, 45,
                        26, 8, 12, 16, 20, 23, 40, 11, 24,
                        4, 2, 25, 35, 48
                    };
                    return ComposeFromTop23(t);

                // 4 = Flush (Clubs) — P1 only
                case 4:
                    t = new()
                    {
                        4, 3, 28, 24, 12, 16, 48, 23, 19,
                        36, 51, 6, 43, 50, 31, 27, 46, 30,
                        40, 20, 8, 47, 35
                    };
                    return ComposeFromTop23(t);

                // 5 = Straight (5–9 mixed) — P1 only
                case 5:
                    t = new()
                    {
                        17, 3, 47, 43, 51, 2, 39, 40, 24,
                        23, 8, 12, 16, 19, 48, 15, 11, 52,
                        26, 31, 36, 49, 7
                    };
                    return ComposeFromTop23(t);

                // 6 = Straight Ace-Low (A–2–3–4–5) — P1 only - Checking Display: See EvalEngine.SortHand()
                case 6:
                    t = new()
                    {
                        1, 36, 35, 51, 48, 42, 24, 23, 27,
                        7, 43, 40, 50, 31, 39, 32, 34, 46,
                        19, 16, 10, 52, 47
                    };
                    return ComposeFromTop23(t);

                // 7 = Three of a Kind (Queens) — P1 only
                case 7:
                    t = new()
                    {
                        45, 3, 43, 39, 27, 52, 31, 42, 40,
                        46, 32, 16, 12, 24, 15, 36, 8, 23,
                        48, 35, 6, 17, 51
                    };
                    return ComposeFromTop23(t);

                // 8 = Two Pair — P1 only
                case 8:
                    t = new()
                    {
                        1, 51, 43, 39, 47, 36, 32, 52, 46,
                        27, 8, 12, 16, 20, 23, 40, 11, 24,
                        3, 28, 6, 33, 48
                    };
                    return ComposeFromTop23(t);

                // 9 = Pair (pocket Jacks) — P1 only
                case 9:
                    t = new()
                    {
                        41, 3, 51, 39, 31, 24, 19, 12, 50,
                        42, 52, 40, 21, 29, 11, 20, 18, 2,
                        16, 35, 46, 25, 8
                    };
                    return ComposeFromTop23(t);

                // 10 = High Card (A-high) — P1 only
                case 10:
                    t = new()
                    {
                        1, 12, 39, 31, 19, 44, 30, 18, 41,
                        43, 26, 14, 11, 15, 29, 40, 16, 32,
                        51, 36, 22, 47, 8
                    };
                    return ComposeFromTop23(t);

                // 11 = Three-Player Tie (Aces & Kings) — P1–P3 tie
                case 11:
                    t = new()
                    {
                        49, 50, 51, 31, 24, 19, 40, 33, 36,
                        12, 11, 18, 29, 14, 15, 32, 41, 43,
                        4, 3, 52, 47, 25
                    };
                    return ComposeFromTop23(t);

                // Default fallback (Straight win)
                default:
                    t = new()
                    {
                        1, 47, 43, 39, 27, 40, 46, 38, 45,
                        35, 44, 48, 36, 20, 34, 10, 33, 41,
                        51, 32, 22, 13, 7
                    };
                    return ComposeFromTop23(t);
            }
        }

        private static readonly Dictionary<string, int> RankValue = new()
        {
            ["2"] = 2,
            ["3"] = 3,
            ["4"] = 4,
            ["5"] = 5,
            ["6"] = 6,
            ["7"] = 7,
            ["8"] = 8,
            ["9"] = 9,
            ["10"] = 10,
            ["J"] = 11,
            ["Q"] = 12,
            ["K"] = 13,
            ["A"] = 14
        };

        private static bool IsWheelA2345(List<Card> cards)
        {
            if (cards is null || cards.Count != 5) return false;
            bool a = false, b2 = false, b3 = false, b4 = false, b5 = false;
            for (int i = 0; i < 5; i++)
            {
                switch (cards[i].Face)
                {
                    case "A": a = true; break;
                    case "2": b2 = true; break;
                    case "3": b3 = true; break;
                    case "4": b4 = true; break;
                    case "5": b5 = true; break;
                }
            }
            return a && b2 && b3 && b4 && b5;
        }

        private static int SuitOrder(string suit)
        {
            if (string.IsNullOrEmpty(suit)) return 4;

            // Fast path: HTML entity numeric code like "&#9824;"
            if (suit.Length >= 6 && suit.StartsWith("&#") && suit.EndsWith(";"))
            {
                if (int.TryParse(suit.AsSpan(2, suit.Length - 3 - 1), out int code))
                {
                    // ♠ 9824, ♥ 9829, ♦ 9830, ♣ 9827
                    return code switch
                    {
                        9827 => 0, // clubs
                        9830 => 1, // diamonds
                        9829 => 2, // hearts
                        9824 => 3, // spades
                        _ => 4
                    };
                }
            }

            // Normalize other representations
            ReadOnlySpan<char> s = suit.AsSpan().Trim();
            if (s.Length == 1)
            {
                char c = char.ToUpperInvariant(s[0]);
                return c switch
                {
                    'C' or '♣' => 0,
                    'D' or '♦' => 1,
                    'H' or '♥' => 2,
                    'S' or '♠' => 3,
                    _ => 4
                };
            }

            // Words
            var upper = suit.Trim().ToUpperInvariant();
            return upper switch
            {
                "CLUBS" => 0,
                "DIAMONDS" => 1,
                "HEARTS" => 2,
                "SPADES" => 3,
                _ => 4
            };
        }

        private static void SortWinnerInPlace(List<Card> hand)
        {
            if (hand is null || hand.Count <= 1) return;
            bool wheel = IsWheelA2345(hand);

            hand.Sort((x, y) =>
            {
                int rx = (wheel && x.Face == "A") ? 1 : RankValue[x.Face];
                int ry = (wheel && y.Face == "A") ? 1 : RankValue[y.Face];

                int cmp = rx.CompareTo(ry);
                if (cmp != 0) return cmp;

                // Secondary key: suit (now robust to entity/char/word)
                cmp = SuitOrder(x.Suit).CompareTo(SuitOrder(y.Suit));
                if (cmp != 0) return cmp;

                // Tertiary: ID for total ordering stability
                return x.ID.CompareTo(y.ID);
            });
        }

        #endregion

    }
}