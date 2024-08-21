using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace CardgameModel.Truco {

    public enum NotificationType {
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

    public class PublicNotification {
        public int Id { get; set; }
        public TrucoNotification TrucoNotification { get; set; }

        public PublicNotification(int id, TrucoNotification trucoNotification) {
            Id = id;
            TrucoNotification = trucoNotification;
        }

        public override string ToString() {
            return string.Format("PrivateNotification {{ id={0} notification={1} }}",
                Id, TrucoNotification);
        }
        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != typeof(PublicNotification)) return false;
            PublicNotification pn = (PublicNotification)obj;
            return Id.Equals(pn.Id)
                && TrucoNotification.Equals(pn.TrucoNotification);
        }

        public override int GetHashCode() {
            return Id.GetHashCode() ^ TrucoNotification.GetHashCode();
        }
    }

    public class PrivateNotification {

        public int Id { get; set; }
        public List<long> PlayerIds { get; set; }
        public TrucoNotification PrivatePart { get; set; }
        public TrucoNotification PublicPart { get; set; }

        public PrivateNotification(int id, List<long> playerIds, TrucoNotification privatePart, TrucoNotification publicPart) {
            Id = id;
            PlayerIds = playerIds;
            PrivatePart = privatePart;
            PublicPart = publicPart;
        }

        public override string ToString() {
            return string.Format("PrivateNotification {{ id={0} players={1} private={2} public={3} }}",
                Id, string.Join<long>(", ", PlayerIds), PrivatePart, PublicPart);
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            PrivateNotification pv = (PrivateNotification)obj;
            return Id.Equals(pv.Id)
                && PlayerIds.SequenceEqual(pv.PlayerIds)
                && PrivatePart.Equals(pv.PrivatePart)
                && PublicPart.Equals(pv.PublicPart);
        }

        public override int GetHashCode() {
            int hash = Id.GetHashCode()
                ^ PrivatePart.GetHashCode()
                ^ PublicPart.GetHashCode();
            foreach(var l in PlayerIds) { hash ^= l.GetHashCode(); }
            return hash;
        }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "NotificationType")]
    [JsonDerivedType(typeof(AddPlayer), typeDiscriminator: (int)NotificationType.ADD_PLAYER)]
    [JsonDerivedType(typeof(StartGame), typeDiscriminator: (int)NotificationType.START_GAME)]
    [JsonDerivedType(typeof(StartDeal), typeDiscriminator: (int)NotificationType.START_DEAL)]
    [JsonDerivedType(typeof(StartRound), typeDiscriminator: (int)NotificationType.START_ROUND)]
    [JsonDerivedType(typeof(SetHand), typeDiscriminator: (int)NotificationType.SET_HAND)]
    [JsonDerivedType(typeof(SetHandPublic), typeDiscriminator: (int)NotificationType.SET_HAND_PUBLIC)]
    [JsonDerivedType(typeof(PlayCard), typeDiscriminator: (int)NotificationType.PLAY_CARD)]
    [JsonDerivedType(typeof(EndRoundWinner), typeDiscriminator: (int)NotificationType.END_ROUND_WINNER)]
    [JsonDerivedType(typeof(EndRoundDrawn), typeDiscriminator: (int)NotificationType.END_ROUND_DRAWN)]
    [JsonDerivedType(typeof(EndDeal), typeDiscriminator: (int)NotificationType.END_DEAL)]
    [JsonDerivedType(typeof(EndGame), typeDiscriminator: (int)NotificationType.END_GAME)]
    [JsonDerivedType(typeof(SetActivePlayer), typeDiscriminator: (int)NotificationType.SET_ACTIVE_PLAYER)]
    [JsonDerivedType(typeof(SetDealerPlayer), typeDiscriminator: (int)NotificationType.SET_DEALER)]
    [JsonDerivedType(typeof(CallTruco), typeDiscriminator: (int)NotificationType.CALL_TRUCO)]
    [JsonDerivedType(typeof(AcceptTruco), typeDiscriminator: (int)NotificationType.ACCEPT_TRUCO)]
    [JsonDerivedType(typeof(Fold), typeDiscriminator: (int)NotificationType.FOLD)]
    public abstract class TrucoNotification {
        public NotificationType NotificationType { get; set; }

        public TrucoNotification(NotificationType NotificationType) {
            this.NotificationType = NotificationType;
        }

        public bool DictionaryEqual<K, V>(Dictionary<K, V> one, Dictionary<K, V> two) {
            if (one == null || two == null) return one == null && two == null;
            if (one.Keys.Count != two.Keys.Count) return false;
            foreach (K key in one.Keys) {
                if (!two.ContainsKey(key)) return false;
                V twoValue = two[key];
                if (twoValue == null || !one[key].Equals(twoValue)) return false;
            }
            return true;
        }

        public string ToString<K, V>(Dictionary<K, V> dictionary) {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<K, V> pair in dictionary) {
                builder.Append("{ ");
                builder.Append(pair.Key);
                builder.Append(": ");
                builder.Append(pair.Value);
                builder.Append(" }");
            }
            return builder.ToString();
        }
    }

    public class Nothing : TrucoNotification {

        public Nothing() : base(NotificationType.NONE) { }

    }

    public class AddPlayer : TrucoNotification {

        public Player Player { get; set; }

        public AddPlayer(Player Player) : base(NotificationType.ADD_PLAYER) {
            this.Player = Player;
        }
        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (AddPlayer) obj;
            return Player.Equals(other.Player);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode() ^Player.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("AddPlayer {{ player={0} }}", Player);
        }
    }

    public class StartGame : TrucoNotification {
        public List<Player> Players { get; set; }
        public List<long> Seating { get; set; }

        public StartGame(List<Player> Players, List<long> Seating) : base(NotificationType.START_GAME) {
            this.Players = Players;
            this.Seating = Seating;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (StartGame) obj;
            return
                Players.SequenceEqual(other.Players)
                && Seating.SequenceEqual(other.Seating);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode();
            foreach (var p in Players) {
                hash ^= p.GetHashCode();
            }
            foreach (var l in Seating) {
                hash ^= l.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return string.Format("StartGame {{ Players={0} Seating={1} }}",
                string.Join(", ", Players), string.Join(", ", Seating));
        }
    }

    public class StartDeal : TrucoNotification {

        public long DealerId { get; set; }

        public StartDeal(long DealerId) : base(NotificationType.START_DEAL) {
            this.DealerId = DealerId;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (StartDeal)obj;
            return DealerId == other.DealerId;
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode() ^ DealerId.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("StartDeal {{ DealerId={0} }}", DealerId);
        }
    }

    public class StartRound : TrucoNotification {
        public long StartingPlayerId { get; set; }

        public StartRound(long StartingPlayer) : base(NotificationType.START_ROUND) {
            this.StartingPlayerId = StartingPlayer;
        }
    }

    public class SetHand : TrucoNotification {
        public long PlayerId { get; set; }
        public Card[] Cards { get; set; }

        public SetHand(long PlayerId, Card[] Cards) : base(NotificationType.SET_HAND) {
            this.PlayerId = PlayerId;
            this.Cards = Cards;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) { return false; }
            var other = (SetHand)obj;
            if (other == null) return false;
            return Cards.SequenceEqual(other.Cards);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode();
            foreach (var card in Cards) {
                hash ^= card.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return "SetHand { PlayerId = " + PlayerId + " Cards = [" + string.Join<Card>(", ", Cards) + "] }";
        }
    }

    public class SetHandPublic : TrucoNotification {
        public long PlayerId { get; set; }
        public int NumberOfCards { get; set; }
        public SetHandPublic(long PlayerId, int NumberOfCards) : base(NotificationType.SET_HAND_PUBLIC) {
            this.PlayerId = PlayerId;
            this.NumberOfCards = NumberOfCards;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (SetHandPublic)obj;
            return PlayerId == other.PlayerId
                && NumberOfCards == other.NumberOfCards;
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode()
                ^ NumberOfCards.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("SetHandPublic {{ PlayerId={0} NumberOfCards={1} }}", PlayerId, NumberOfCards);
        }
    }

    public class PlayCard : TrucoNotification {
        public long PlayerId { get; set; }
        public Card Card { get; set; }
        public Dictionary<long, Card> Round { get; set; }

        public PlayCard(long PlayerId, Card Card, Dictionary<long, Card> Round) : base(NotificationType.PLAY_CARD) {
            this.PlayerId = PlayerId;
            this.Card = Card;
            this.Round = Round;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) { return false; }
            var other = (PlayCard)obj;
            return PlayerId == other.PlayerId
                && Card.Equals(other.Card)
                && DictionaryEqual(Round, other.Round);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode()
                ^ Card.GetHashCode();
            foreach (var kvp in Round) {
                hash ^= kvp.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return "PlayCard { PlayerId = " + PlayerId +
                " Card = " + Card +
                " Round = { " + ToString(Round) + " } }";
        }
    }

    public class EndRoundWinner : TrucoNotification {
        public long WinnerPlayerId { get; set; }
        public Dictionary<long, Card> Round { get; set; }

        public EndRoundWinner(long WinnerPlayerId, Dictionary<long, Card> Round)
                : base(NotificationType.END_ROUND_WINNER) {
            this.WinnerPlayerId = WinnerPlayerId;
            this.Round = Round;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) { return false; }
            var other = (EndRoundWinner)obj;
            return WinnerPlayerId == other.WinnerPlayerId
                && DictionaryEqual(Round, other.Round);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ WinnerPlayerId.GetHashCode();
            foreach (var kvp in Round) {
                hash ^= kvp.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return "EndRoundWinner { WinnerPlayerId = " + WinnerPlayerId +
                " Round = { " + ToString(Round) + " }";
        }
    }

    public class EndRoundDrawn : TrucoNotification {

        public List<long> DrawnPlayerIds { get; set; }
        public Dictionary<long, Card> Round { get; set; }

        public EndRoundDrawn(List<long> DrawnPlayerIds, Dictionary<long, Card> Round)
                : base(NotificationType.END_ROUND_DRAWN) {
            this.DrawnPlayerIds = DrawnPlayerIds;
            this.Round = Round;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) { return false; }
            var other = (EndRoundDrawn)obj;
            return DrawnPlayerIds.SequenceEqual(other.DrawnPlayerIds)
                && DictionaryEqual(Round, other.Round);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode();
            foreach (var p in DrawnPlayerIds) {
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

    public class EndGame : TrucoNotification {
        public Dictionary<Team, int> Score { get; set; }

        public EndGame(Dictionary<Team, int> Score)
                : base(NotificationType.END_GAME) {
            this.Score = Score;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (EndGame)obj;
            return DictionaryEqual(Score, other.Score);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode();
            foreach (var kvp in Score) {
                hash ^= kvp.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return string.Format("EndGame {{ Score={0} }}", ToString(Score));
        }
    }

    public class CallTruco : TrucoNotification {
        public long PlayerId { get; set; }
        public int Points { get; set; }

        public CallTruco(long PlayerId, int Points) : base(NotificationType.CALL_TRUCO) {
            this.PlayerId = PlayerId;
            this.Points = Points;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (CallTruco)obj;
            return PlayerId == other.PlayerId
                && Points == other.Points;
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode()
                ^ Points.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("CallTruco {{ PlayerId={0} Points={1} }}", PlayerId, Points);
        }
    }

    public class AcceptTruco : TrucoNotification {
        public long PlayerId { get; set; }

        public AcceptTruco(long PlayerId) : base(NotificationType.ACCEPT_TRUCO) {
            this.PlayerId = PlayerId;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (AcceptTruco)obj;
            return PlayerId == other.PlayerId;
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("AcceptTruco {{ PlayerId={0} }}", PlayerId);
        }
    }

    public class Fold : TrucoNotification {
        public long PlayerId { get; set; }

        public Fold(long PlayerId) : base(NotificationType.FOLD) {
            this.PlayerId = PlayerId;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (Fold)obj;
            return PlayerId == other.PlayerId;
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("Fold {{ PlayerId={0} }}", PlayerId);
        }
    }

    public class SetActivePlayer : TrucoNotification {
        public long PlayerId { get; set; }

        public SetActivePlayer(long PlayerId) : base(NotificationType.SET_ACTIVE_PLAYER) {
            this.PlayerId = PlayerId;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (SetActivePlayer)obj;
            return PlayerId == other.PlayerId;
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("SetActivePlayer {{ PlayerId={0} }}", PlayerId);
        }
    }

    public class SetDealerPlayer : TrucoNotification {
        public long PlayerId { get; set; }

        public SetDealerPlayer(long PlayerId) : base(NotificationType.SET_DEALER) {
            this.PlayerId = PlayerId;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var other = (SetDealerPlayer)obj;
            return PlayerId == other.PlayerId;
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode()
                ^ PlayerId.GetHashCode();
            return hash;
        }

        public override string ToString() {
            return string.Format("SetDealerPlayer {{ PlayerId={0} }}", PlayerId);
        }
    }

    public class EndDeal : TrucoNotification {
        public List<long> Players { get; set; }

        public Dictionary<Team, int> Score { get; set; }

        public EndDeal(List<long> WinnerPlayerIds, Dictionary<Team, int> Score) : base(NotificationType.END_DEAL) {
            this.Players = WinnerPlayerIds;
            this.Score = Score;
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) { return false; }
            var other = (EndDeal)obj;
            return Players.SequenceEqual(other.Players)
                && DictionaryEqual(Score, other.Score);
        }

        public override int GetHashCode() {
            int hash = NotificationType.GetHashCode();
            foreach (var p in Players) {
                hash ^= p.GetHashCode();
            }
            foreach (var kvp in Score) {
                hash ^= kvp.Key.GetHashCode();
            }
            return hash;
        }

        public override string ToString() {
            return string.Format("EndDeal {{ Players = {0} Score={1} }}", string.Join(", ", Players), ToString(Score));
        }
    }


}
