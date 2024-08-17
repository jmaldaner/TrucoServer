using System.Collections.Immutable;

namespace CardgameModel {
    public enum Suit
    {
        NONE = 0,
        HEARTS = 1,
        SPADES = 2,
        CLUBS = 3,
        DIAMONDS = 4
    }

    public readonly record struct Card(Suit Suit, int Value) : IComparable<Card>
    {
        public static readonly List<Suit> Suits = [Suit.DIAMONDS, Suit.SPADES, Suit.HEARTS, Suit.CLUBS];

        public static readonly Card Blank = new(Suit.NONE, 0);

        public static Card Of(string shortCode)
        {
            return new Card(CHAR_TO_SUIT[shortCode[0]], STRING_TO_VALUE[shortCode.Substring(1)]);
        }

        public int CompareTo(Card other)
        {
            if (this.Value != other.Value)
            {
                return this.Value - other.Value;
            }
            else
            {
                return this.Suit.CompareTo(other.Suit);
            }
        }

        private static ImmutableDictionary<Suit, char> SUIT_TO_CHAR = ImmutableDictionary.CreateRange(
            new KeyValuePair<Suit, char>[] {
                KeyValuePair.Create(Suit.NONE, '-'),
                KeyValuePair.Create(Suit.HEARTS, '♡'),
                KeyValuePair.Create(Suit.SPADES, '♠'),
                KeyValuePair.Create(Suit.DIAMONDS, '♢'),
                KeyValuePair.Create(Suit.CLUBS, '♣')
            });

        private static ImmutableDictionary<char, Suit> CHAR_TO_SUIT = ImmutableDictionary.CreateRange(
            new KeyValuePair<char, Suit>[] {
                KeyValuePair.Create('-', Suit.NONE),
                KeyValuePair.Create('♡', Suit.HEARTS),
                KeyValuePair.Create('♠', Suit.SPADES),
                KeyValuePair.Create('♢', Suit.DIAMONDS),
                KeyValuePair.Create('♣', Suit.CLUBS),
                KeyValuePair.Create('H', Suit.HEARTS),
                KeyValuePair.Create('S', Suit.SPADES),
                KeyValuePair.Create('D', Suit.DIAMONDS),
                KeyValuePair.Create('C', Suit.CLUBS),
                KeyValuePair.Create('h', Suit.HEARTS),
                KeyValuePair.Create('s', Suit.SPADES),
                KeyValuePair.Create('d', Suit.DIAMONDS),
                KeyValuePair.Create('c', Suit.CLUBS),
            });

        private static ImmutableDictionary<int, string> VALUE_TO_STRING = ImmutableDictionary.CreateRange(
            new KeyValuePair<int, string>[] {
                KeyValuePair.Create(0, "-"),
                KeyValuePair.Create(1, "A"),
                KeyValuePair.Create(2, "2"),
                KeyValuePair.Create(3, "3"),
                KeyValuePair.Create(4, "4"),
                KeyValuePair.Create(5, "5"),
                KeyValuePair.Create(6, "6"),
                KeyValuePair.Create(7, "7"),
                KeyValuePair.Create(8, "8"),
                KeyValuePair.Create(9, "9"),
                KeyValuePair.Create(10, "10"),
                KeyValuePair.Create(11, "J"),
                KeyValuePair.Create(12, "Q"),
                KeyValuePair.Create(13, "K"),
            });

        private static ImmutableDictionary<string, int> STRING_TO_VALUE = VALUE_TO_STRING.ToImmutableDictionary(x => x.Value, x => x.Key);

        public override string ToString()
        {
            return SUIT_TO_CHAR[Suit] + VALUE_TO_STRING[Value];
        }
    }

}
