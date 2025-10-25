namespace poker.net.Models
{
    public static class RawDeck
    {
        // A single, read-only deck for the lifetime of the app.
        // Use DeckHelper.GetDeepCopyOfDeck(RawDeck.All) before shuffling/mutating.
        public static readonly IReadOnlyList<Card> All = new List<Card>
        {
            new Card { ID = 1,  Color = "000", Face = "A",  Suit = "&#9824;", Value = 268442665 },
            new Card { ID = 2,  Color = "F00", Face = "A",  Suit = "&#9829;", Value = 268446761 },
            new Card { ID = 3,  Color = "F00", Face = "A",  Suit = "&#9830;", Value = 268454953 },
            new Card { ID = 4,  Color = "000", Face = "A",  Suit = "&#9827;", Value = 268471337 },

            new Card { ID = 5,  Color = "000", Face = "2",  Suit = "&#9824;", Value = 69634 },
            new Card { ID = 6,  Color = "F00", Face = "2",  Suit = "&#9829;", Value = 73730 },
            new Card { ID = 7,  Color = "F00", Face = "2",  Suit = "&#9830;", Value = 81922 },
            new Card { ID = 8,  Color = "000", Face = "2",  Suit = "&#9827;", Value = 98306 },

            new Card { ID = 9,  Color = "000", Face = "3",  Suit = "&#9824;", Value = 135427 },
            new Card { ID = 10, Color = "F00", Face = "3",  Suit = "&#9829;", Value = 139523 },
            new Card { ID = 11, Color = "F00", Face = "3",  Suit = "&#9830;", Value = 147715 },
            new Card { ID = 12, Color = "000", Face = "3",  Suit = "&#9827;", Value = 164099 },

            new Card { ID = 13, Color = "000", Face = "4",  Suit = "&#9824;", Value = 266757 },
            new Card { ID = 14, Color = "F00", Face = "4",  Suit = "&#9829;", Value = 270853 },
            new Card { ID = 15, Color = "F00", Face = "4",  Suit = "&#9830;", Value = 279045 },
            new Card { ID = 16, Color = "000", Face = "4",  Suit = "&#9827;", Value = 295429 },

            new Card { ID = 17, Color = "000", Face = "5",  Suit = "&#9824;", Value = 529159 },
            new Card { ID = 18, Color = "F00", Face = "5",  Suit = "&#9829;", Value = 533255 },
            new Card { ID = 19, Color = "F00", Face = "5",  Suit = "&#9830;", Value = 541447 },
            new Card { ID = 20, Color = "000", Face = "5",  Suit = "&#9827;", Value = 557831 },

            new Card { ID = 21, Color = "000", Face = "6",  Suit = "&#9824;", Value = 1053707 },
            new Card { ID = 22, Color = "F00", Face = "6",  Suit = "&#9829;", Value = 1057803 },
            new Card { ID = 23, Color = "F00", Face = "6",  Suit = "&#9830;", Value = 1065995 },
            new Card { ID = 24, Color = "000", Face = "6",  Suit = "&#9827;", Value = 1082379 },

            new Card { ID = 25, Color = "000", Face = "7",  Suit = "&#9824;", Value = 2102541 },
            new Card { ID = 26, Color = "F00", Face = "7",  Suit = "&#9829;", Value = 2106637 },
            new Card { ID = 27, Color = "F00", Face = "7",  Suit = "&#9830;", Value = 2114829 },
            new Card { ID = 28, Color = "000", Face = "7",  Suit = "&#9827;", Value = 2131213 },

            new Card { ID = 29, Color = "000", Face = "8",  Suit = "&#9824;", Value = 4199953 },
            new Card { ID = 30, Color = "F00", Face = "8",  Suit = "&#9829;", Value = 4204049 },
            new Card { ID = 31, Color = "F00", Face = "8",  Suit = "&#9830;", Value = 4212241 },
            new Card { ID = 32, Color = "000", Face = "8",  Suit = "&#9827;", Value = 4228625 },

            new Card { ID = 33, Color = "000", Face = "9",  Suit = "&#9824;", Value = 8394515 },
            new Card { ID = 34, Color = "F00", Face = "9",  Suit = "&#9829;", Value = 8398611 },
            new Card { ID = 35, Color = "F00", Face = "9",  Suit = "&#9830;", Value = 8406803 },
            new Card { ID = 36, Color = "000", Face = "9",  Suit = "&#9827;", Value = 8423187 },

            new Card { ID = 37, Color = "000", Face = "10", Suit = "&#9824;", Value = 16783383 },
            new Card { ID = 38, Color = "F00", Face = "10", Suit = "&#9829;", Value = 16787479 },
            new Card { ID = 39, Color = "F00", Face = "10", Suit = "&#9830;", Value = 16795671 },
            new Card { ID = 40, Color = "000", Face = "10", Suit = "&#9827;", Value = 16812055 },

            new Card { ID = 41, Color = "000", Face = "J",  Suit = "&#9824;", Value = 33560861 },
            new Card { ID = 42, Color = "F00", Face = "J",  Suit = "&#9829;", Value = 33564957 },
            new Card { ID = 43, Color = "F00", Face = "J",  Suit = "&#9830;", Value = 33573149 },
            new Card { ID = 44, Color = "000", Face = "J",  Suit = "&#9827;", Value = 33589533 },

            new Card { ID = 45, Color = "000", Face = "Q",  Suit = "&#9824;", Value = 67115551 },
            new Card { ID = 46, Color = "F00", Face = "Q",  Suit = "&#9829;", Value = 67119647 },
            new Card { ID = 47, Color = "F00", Face = "Q",  Suit = "&#9830;", Value = 67127839 },
            new Card { ID = 48, Color = "000", Face = "Q",  Suit = "&#9827;", Value = 67144223 },

            new Card { ID = 49, Color = "000", Face = "K",  Suit = "&#9824;", Value = 134224677 },
            new Card { ID = 50, Color = "F00", Face = "K",  Suit = "&#9829;", Value = 134228773 },
            new Card { ID = 51, Color = "F00", Face = "K",  Suit = "&#9830;", Value = 134236965 },
            new Card { ID = 52, Color = "000", Face = "K",  Suit = "&#9827;", Value = 134253349 }
        }.AsReadOnly();
    }
}
