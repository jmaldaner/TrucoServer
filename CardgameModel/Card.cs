using CardgameModel.Truco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;

namespace CardgameModel {
    public enum Suit
    {
        NONE = 0,
        HEARTS = 1,
        SPADES = 2,
        CLUBS = 3,
        DIAMONDS = 4
    }

    public class Card : IComparable<Card> {
        public static readonly List<Suit> Suits = new List<Suit>() { Suit.DIAMONDS, Suit.SPADES, Suit.HEARTS, Suit.CLUBS };

        public static readonly Card Blank = new Card(Suit.NONE, 0);

        public Suit Suit { get; set; }
        public int Value {  get; set; }

        public Card(Suit Suit, int Value) {
            this.Suit = Suit;
            this.Value = Value;
        }

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

        private static readonly Dictionary<Suit, char> SUIT_TO_CHAR = new Dictionary<Suit, char>() {
            { Suit.NONE, '-' },
            { Suit.HEARTS, '♡' },
            { Suit.SPADES, '♠' },
            { Suit.DIAMONDS, '♢' },
            { Suit.CLUBS, '♣' }
        };

        private static readonly Dictionary<char, Suit> CHAR_TO_SUIT = new Dictionary<char, Suit>() {
            {  '-', Suit.NONE },
            { '♡', Suit.HEARTS },
            { '♠', Suit.SPADES },
            { '♢', Suit.DIAMONDS },
            { '♣', Suit.CLUBS },
            { 'H', Suit.HEARTS },
            { 'S', Suit.SPADES },
            { 'D', Suit.DIAMONDS },
            { 'C', Suit.CLUBS },
            { 'h', Suit.HEARTS },
            { 's', Suit.SPADES },
            { 'd', Suit.DIAMONDS },
            { 'c', Suit.CLUBS }
        };

        private static Dictionary<int, string> VALUE_TO_STRING = new Dictionary<int, string>() {
            { 0, "-" },
            { 1, "A" },
            { 2, "2" },
            { 3, "3" },
            { 4, "4" },
            { 5, "5" },
            { 6, "6" },
            { 7, "7" },
            { 8, "8" },
            { 9, "9" },
            { 10, "10" },
            { 11, "J" },
            { 12, "Q" },
            { 13, "K" },
        };

        private static Dictionary<string, int> STRING_TO_VALUE = VALUE_TO_STRING.ToDictionary(x => x.Value, x => x.Key);

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (Card)obj;
            return Suit == other.Suit
                && Value == other.Value;
        }

        public override int GetHashCode() {
            return Suit.GetHashCode() ^ Value.GetHashCode();
        }

        public override string ToString()
        {
            return SUIT_TO_CHAR[Suit] + VALUE_TO_STRING[Value];
        }
    }

}
