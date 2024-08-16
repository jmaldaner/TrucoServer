using CardgameServer.cards;
using CardgameServer.player;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Text;

namespace CardgameServer.game.truco
{
    
    public enum NotificationType
    {
        NONE,
        ADD_PLAYER,
        START_GAME,
        START_DEAL,
        START_ROUND,
        SET_HAND,
        SET_HAND_PUBLIC,
        PLAY_CARD,
        END_ROUND_WINNER,
        END_ROUND_DRAWN,
        END_DEAL,
        END_GAME,
        SET_ACTIVE_PLAYER,
        SET_DEALER,
        CALL_TRUCO,
        ACCEPT_TRUCO,
        FOLD
    }

    public record PublicNotification(int Id, TrucoNotification TrucoNotification);

    public record PrivateNotification(
        int Id,
        List<long> PlayerIds,
        TrucoNotification PrivatePart,
        TrucoNotification PublicPart);

    public abstract record TrucoNotification(NotificationType NotificationType)
    {
        protected bool DictionaryEqual<K, V>(Dictionary<K, V> one, Dictionary<K, V> two)
        {
            if (one.Keys.Count != two.Keys.Count) return false;
            foreach (K key in one.Keys)
            {
                V? twoValue = two[key];
                if (twoValue == null || !one[key].Equals(twoValue)) return false;
            }
            return true;
        }

        protected string ToString<K, V>(Dictionary<K, V> dictionary)
        {
            StringBuilder builder = new();
            foreach (KeyValuePair<K, V> pair in dictionary)
            {
                builder.Append("{ ");
                builder.Append(pair.Key);
                builder.Append(": ");
                builder.Append(pair.Value);
                builder.Append(" }");
            }
            return builder.ToString();
        }
    }

    public record Nothing()
    : TrucoNotification(NotificationType.NONE);

    public record AddPlayer(Player Player)
        : TrucoNotification(NotificationType.ADD_PLAYER);

    public record StartGame(List<Player> Players, List<long> Seating)
        : TrucoNotification(NotificationType.START_GAME)
    {
        public virtual bool Equals(StartGame? other)
        {
            return
                other != null
                && Players.SequenceEqual(other.Players)
                && Seating.SequenceEqual(other.Seating);
        }

        public override int GetHashCode()
        {
            int hash = NotificationType.GetHashCode();
            foreach (var p in Players)
            {
                hash ^= p.GetHashCode();
            }
            foreach (var l in Seating)
            {
                hash ^= l.GetHashCode();
            }
            return hash;
        }
    }

    public record StartDeal(long DealerId)
        : TrucoNotification(NotificationType.START_DEAL);

    public record StartRound(long StartingPlayerId)
        : TrucoNotification(NotificationType.START_ROUND);

    public record SetHand(long PlayerId, Card[] Cards)
        : TrucoNotification(NotificationType.SET_HAND)
    {
        public virtual bool Equals(SetHand? other)
        {
            if (other == null) return false;
            return Cards.SequenceEqual(other.Cards);
        }

        public override int GetHashCode()
        {
            int hash = NotificationType.GetHashCode();
            foreach (var card in Cards)
            {
                hash ^= card.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return "SetHand { PlayerId = " + PlayerId + " Cards = [" + string.Join(", ", Cards) + "] }";
        }
    }

    public record SetHandPublic(long PlayerId, int NumberOfCards)
        : TrucoNotification(NotificationType.SET_HAND_PUBLIC);

    public record PlayCard(long PlayerId, Card Card, Dictionary<long, Card> Round)
        : TrucoNotification(NotificationType.PLAY_CARD)
    {
        public virtual bool Equals(PlayCard? other)
        {
            return
                other != null
                && PlayerId == other.PlayerId
                && Card == other.Card
                && DictionaryEqual(Round, other.Round);
        }

        public override int GetHashCode()
        {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode()
                ^ Card.GetHashCode();
            foreach (var kvp in Round)
            {
                hash ^= kvp.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return "PlayCard { PlayerId = " + PlayerId +
                " Card = " + Card +
                " Round = { " + ToString(Round) + " }";
        }
    }

    public record EndRoundWinner(long WinnerPlayerId, Dictionary<long, Card> Round)
        : TrucoNotification(NotificationType.END_ROUND_WINNER)
    {
        public virtual bool Equals(EndRoundWinner? other)
        {
            return
                other != null
                && WinnerPlayerId == other.WinnerPlayerId
                && DictionaryEqual(Round, other.Round);
        }

        public override int GetHashCode()
        {
            int hash = NotificationType.GetHashCode()
                ^ WinnerPlayerId.GetHashCode();
            foreach (var kvp in Round)
            {
                hash ^= kvp.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return "EndRoundWinner { WinnerPlayerId = " + WinnerPlayerId +
                " Round = { " + ToString(Round) + " }";
        }
    }

    public record EndRoundDrawn(List<long> DrawnPlayerIds, Dictionary<long, Card> Round)
        : TrucoNotification(NotificationType.END_ROUND_DRAWN)
    {
        public virtual bool Equals(EndRoundDrawn? other) {
            return
                other != null
                && DrawnPlayerIds.SequenceEqual(other.DrawnPlayerIds)
                && DictionaryEqual(Round, other.Round);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode();
            foreach (var p in DrawnPlayerIds)
            {
                hash ^= p.GetHashCode();
            }
            foreach (var kvp in Round) {
                hash ^= kvp.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return "EndRoundWinner { DrawnPlayerIds = " + string.Join(", ", DrawnPlayerIds) +
                " Round = { " + ToString(Round) + " }";
        }
    }

    public record CallTruco(long PlayerId, int Points)
        : TrucoNotification(NotificationType.CALL_TRUCO);

    public record AcceptTruco(long PlayerId)
        : TrucoNotification(NotificationType.ACCEPT_TRUCO);

    public record Fold(long PlayerId)
        : TrucoNotification(NotificationType.FOLD);

    public record SetActivePlayer(long PlayerId)
        : TrucoNotification(NotificationType.SET_ACTIVE_PLAYER);

    public record SetDealerPlayer(long PlayerId)
    : TrucoNotification(NotificationType.SET_DEALER);

    public record EndDeal(List<long> WinnerPlayerIds)
        : TrucoNotification(NotificationType.END_DEAL) {
        public virtual bool Equals(EndDeal? other) {
            return
                other != null
                && WinnerPlayerIds.SequenceEqual(other.WinnerPlayerIds);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode();
            foreach (var p in WinnerPlayerIds) {
                hash ^= p.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return "EndDeal { WinnerPlayerIds = " + string.Join(", ", WinnerPlayerIds) + " }";
        }
    }


}
