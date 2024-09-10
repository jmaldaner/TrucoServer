using CardgameModel;
using CardgameModel.Truco;
using Nito.AsyncEx;
using System.Collections.Immutable;

namespace CardgameServer.game.truco
{
    public class TrucoGame
    {
        private readonly static ImmutableDictionary<Card, int> TRUCO_RANKS = CreateTrucoRanks();
        private const int MAX_PLAYERS = 4;
        private const int CARDS_PER_ROUND = 3;
        private const int POINTS_PER_GAME = 12;

        private static readonly int[] POINTS_PER_ROUND = [1, 3, 6, 9, 12];

        private List<Card>? deck;
        private Dictionary<int, Player> players = [];
        private List<int> seating = [];
        private readonly Dictionary<Team, int> scores =
            new Dictionary<Team, int>() {
                { Team.TeamOne, 0 },
                { Team.TeamTwo, 0 } };
        private readonly Dictionary<int, List<Card>> hands = [];
        private readonly List<PrivateNotification> notifications = [];
        private Dictionary<Player, Team> teamOfPlayer = [];
        private Dictionary<Team, List<Player>> teamPlayers = [];

        private readonly AsyncMonitor notificationMonitor = new AsyncMonitor();

        private readonly IShuffler<Card> cardShuffler;
        private readonly IShuffler<int> playerShuffler;

        private Player activePlayer;

        private List<Player?> dealWinners = [ null, null, null ];
        private List<Team?> dealTeamWinners = [ null, null, null ];
        private Player dealerPlayer;
        private int dealPointsIndex = 0;
        private Team dealRaised = Team.None;
        private Player? dealRaisedStartPlayer = null;
        private bool dealRaiseAccepted = true;
        private bool readyToDeal = false;

        private Dictionary<int, Card> roundCards = [];
        private Dictionary<int, Card> lastRoundCards = [];
        private Player? lastRoundWinner;
        private Player roundStartPlayer;
        private int currentRound = -1;
        private int currentPlayed = -1;
        private Team teamFolded = Team.None;

        public int Id { get; private set; }

        public TrucoGame(
            int id,
            IShuffler<Card> cardShuffler,
            IShuffler<int> playerShuffler)
        {
            this.Id = id;
            this.cardShuffler = cardShuffler;
            this.playerShuffler = playerShuffler;
        }

        public bool IsAcceptingPlayers()
        {
            lock(players)
            {
                return players.Count < MAX_PLAYERS;
            }
        }

        public void AddPlayer(Player player)
        {
            lock (players)
            {
                if (players.Count >= MAX_PLAYERS)
                {
                    throw new GameException(String.Format("Cannot add new player - Game {0} has maximum number of players ({1})", Id, MAX_PLAYERS));
                }
                if (players.ContainsValue(player))
                {
                    throw new GameException(String.Format("Player {0} already joined", player.Name));
                }
                players.Add(player.Id, player);
                NotifyPublic(new AddPlayer(player));
                if (players.Count == MAX_PLAYERS)
                {
                    Start();
                }
            }
        }

        private void Start()
        {
            seating = new List<int>(players.Keys);
            playerShuffler.Shuffle(seating);
            activePlayer = dealerPlayer = roundStartPlayer = players[seating[0]];
            teamOfPlayer[players[seating[0]]] = Team.TeamOne;
            teamOfPlayer[players[seating[1]]] = Team.TeamTwo;
            teamOfPlayer[players[seating[2]]] = Team.TeamOne;
            teamOfPlayer[players[seating[3]]] = Team.TeamTwo;
            teamPlayers.Add(Team.TeamOne, [ players[seating[0]], players[seating[2]] ]);
            teamPlayers.Add(Team.TeamTwo, [ players[seating[1]], players[seating[3]] ]);
            NotifyPublic(
                new StartGame(
                    new List<Player>(players.Values),
                    seating,
                    teamPlayers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ConvertAll(x => x.Id))));
            readyToDeal = true;
            NotifyPublic(new ReadyToDeal(dealerPlayer.Id));
        }

        public void Deal(Player actor)
        {
            ValidateDealer(actor);
            readyToDeal = false;
            NotifyPublic(new StartDeal(dealerPlayer.Id));
            deck = FreshTrucoDeck();
            cardShuffler.Shuffle(deck);
            foreach (var player in players.Values)
            {
                hands[player.Id] = GetFromDeck(CARDS_PER_ROUND);
                NotifyPrivate([
                    new PrivatePublicNotification([player.Id], 
                    new SetHand(player.Id, hands[player.Id].ToArray()),
                    new SetHandPublic(player.Id, CARDS_PER_ROUND))
                ]);
            }
            dealPointsIndex = 0;
            dealRaiseAccepted = true;
            dealWinners = [null, null, null];
            dealTeamWinners = [null, null, null];
            currentRound = 0;
            lastRoundCards = [];
            lastRoundWinner = null;
            teamFolded = Team.None;
            StartRound();
        }

        private void StartRound()
        {
            if (currentRound == 0)
            {
                roundStartPlayer = NextPlayer(dealerPlayer); 
            }
            else if (dealWinners[currentRound - 1] != null)
            {
                roundStartPlayer = dealWinners[currentRound - 1];
            }
            activePlayer = roundStartPlayer;
            NotifyPublic([
                new StartRound(roundStartPlayer.Id),
                new SetActivePlayer(activePlayer.Id, POINTS_PER_ROUND[dealPointsIndex])]);
            currentPlayed = 0;
            roundCards = [];
            dealRaised = Team.None;
            dealRaisedStartPlayer = null;
        }

        public void Play(Player actor, Card card)
        {
            ValidateActor(actor);
            ValidateTrucoNotInProgress();
            if (roundCards == null)
            {
                throw new GameException(String.Format("Round is not started"));
            }
            lock (roundCards)
            {
                if (!hands[activePlayer.Id].Contains(card))
                {
                    throw new GameException(String.Format("Player {0} played a card that is not in their hand: {1}", actor, card));
                }
                lock (hands[activePlayer.Id])
                {
                    hands[activePlayer.Id].Remove(card);
                }
                roundCards.Add(activePlayer.Id, card);
                NotifyPublic([
                    new PlayCard(activePlayer.Id, card, roundCards),
                    new SetHandPublic(activePlayer.Id, hands[activePlayer.Id].Count)]);
                activePlayer = NextPlayer(activePlayer);
                currentPlayed++;
                if (currentPlayed >= MAX_PLAYERS)
                {
                    EndRound();
                }
                else
                {
                    NotifyPublic(new SetActivePlayer(activePlayer.Id, POINTS_PER_ROUND[dealPointsIndex]));
                }
            }
        }

        public void Truco(Player actor)
        {
            ValidateActor(actor);
            if (roundCards == null)
            {
                throw new GameException(String.Format("Round is not started"));
            }
            lock (roundCards)
            {
                if (dealRaised == teamOfPlayer[activePlayer])
                {
                    throw new GameException(
                        String.Format(
                            "Cannot raise TRUCO, already raised by same team. Player {0} Team {1}",
                            actor,
                            teamOfPlayer[activePlayer]));
                }
                if (dealPointsIndex == POINTS_PER_ROUND.Length - 1)
                {
                    throw new GameException("Maximum already raise");
                }
                dealRaised = teamOfPlayer[activePlayer];
                if (!dealRaiseAccepted)
                {
                    // If dealRaisedAccepted is false this is a re-raise; so player is next for nine (index 3)
                    // and back for six, twelve (index 2, 4).
                    activePlayer = dealPointsIndex % 2 == 0 ? PrevPlayer(activePlayer) : NextPlayer(activePlayer);
                    dealPointsIndex++;
                } else
                {
                    dealRaisedStartPlayer = activePlayer;
                    activePlayer = NextPlayer(activePlayer);
                }
                dealRaiseAccepted = false;
                NotifyPublic([
                    new CallTruco(actor.Id, POINTS_PER_ROUND[dealPointsIndex + 1], activePlayer.Id),
                    new SetActivePlayer(activePlayer.Id, POINTS_PER_ROUND[dealPointsIndex])]);
            }
        }

        public void Accept(Player actor)
        {
            ValidateActor(actor);
            lock (roundCards)
            {
                if (dealRaised == teamOfPlayer[activePlayer])
                {
                    throw new GameException(String.Format("Cannot accept TRUCO. Truco by {0}, Accepting by {1}", dealRaised, activePlayer));
                }
                if (dealRaiseAccepted)
                {
                    throw new GameException("Truco is not in progress");
                }
                if (dealPointsIndex < POINTS_PER_ROUND.Length - 1)
                {
                    dealPointsIndex++;
                }
                dealRaiseAccepted = true;
                activePlayer = dealRaisedStartPlayer;
                dealRaisedStartPlayer = null;
                NotifyPublic([
                    new AcceptTruco(actor.Id, POINTS_PER_ROUND[dealPointsIndex]),
                    new SetActivePlayer(activePlayer.Id, POINTS_PER_ROUND[dealPointsIndex])]);
            }
        }

        public void Fold(Player actor)
        {
            ValidateActor(actor);
            lock (roundCards)
            {
                Player winnerPlayer = PrevPlayer(activePlayer);
                dealWinners[currentRound] = winnerPlayer;
                dealTeamWinners[currentRound] = teamOfPlayer[winnerPlayer];
                teamFolded = teamOfPlayer[activePlayer];
                dealRaiseAccepted = true;
                dealRaisedStartPlayer = null;
                foreach (var kvp in hands) {
                    kvp.Value.Clear();
                }
                NotifyPublic(new Fold(activePlayer.Id));
                EndDeal();
            }
        }

        public List<PrivateNotification> Notifications()
        {
            return new List<PrivateNotification>(notifications);
        }

        public async Task<IEnumerable<PublicNotification>> WaitForNotifications(CancellationTokenSource cts, Player player, int startAtIndex = 0)
        {
            List<PublicNotification> response = [];
            using (notificationMonitor.Enter(cts.Token)) {
                while (!cts.IsCancellationRequested) {
                    for (int i = notifications.FindLastIndex(x => x.Id >= startAtIndex); i >= 0; i--) {
                        if (notifications[i].Id >= startAtIndex) {
                            PrivateNotification pv = notifications[i];
                            response.Insert(
                                0,
                                new PublicNotification(pv.Id, pv.PlayerIds.Contains(player.Id) ? pv.PrivatePart : pv.PublicPart));
                        }
                    }
                    if (response.Count > 0) {
                        return response;
                    }
                    await notificationMonitor.WaitAsync(cts.Token);
                }
            }
            return response;
        }

        private void NotifyPublic(IEnumerable<TrucoNotification> notifications) {
            NotifyPrivate(notifications.Select(x => new PrivatePublicNotification([], x, x)));
        }

        private void NotifyPublic(TrucoNotification notification) {
            NotifyPrivate([ new PrivatePublicNotification([], notification, notification)]);
        }

        private void NotifyPrivate(IEnumerable<PrivatePublicNotification> privatePublicNotifications) {
            using (notificationMonitor.Enter()) {
                foreach (var ppn in privatePublicNotifications) {
                    notifications.Add(new PrivateNotification(notifications.Count, ppn.playerIds, ppn.privateNotification, ppn.publicNotification));
                }
                notificationMonitor.PulseAll();
            }
        }

        public TrucoGameModel ToModel()
        {
            return ToModelInternal(null);
        }

        public TrucoGameModel ToModel(Player player)
        {
            return ToModelInternal(player);
        }

        private TrucoGameModel ToModelInternal(Player? player)
        {
            var tgm = new TrucoGameModel()
            {
                Id = Id,
                Players = players,
                IsAcceptingPlayers = IsAcceptingPlayers()
            };
            if (dealerPlayer != null)
            {
                tgm.Players = players;
                tgm.RoundCards = roundCards;
                tgm.Seating = seating;
                tgm.HandSizes = hands.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
                tgm.Teams = teamPlayers.ToDictionary(x => x.Key, x => x.Value.ConvertAll(y => y.Id));
                tgm.Scores = scores;
                if (player != null)
                {
                    tgm.PrivateHand = hands.GetValueOrDefault(player.Id, []);
                    int playerSeatingIndex = seating.IndexOf(player.Id);
                    tgm.RelativeSeating = [];
                    for (int i = 0; i < MAX_PLAYERS -1; i++)
                    {
                        tgm.RelativeSeating.Add(seating[(playerSeatingIndex + 1 + i) % MAX_PLAYERS]);
                    }
                }
                tgm.Round = currentRound;
                tgm.ActivePlayer = activePlayer.Id;
                tgm.LastRound = lastRoundCards;
                if (lastRoundWinner != null)
                {
                    tgm.LastRoundWinner = lastRoundWinner.Id;
                }
                tgm.ReadyToDeal = readyToDeal;
                tgm.DealerPlayer = dealerPlayer.Id;
                tgm.DealWinners = dealTeamWinners;
                tgm.RaiseAccepting = !dealRaiseAccepted;
                tgm.PointsThisRound = POINTS_PER_ROUND[dealPointsIndex];
                tgm.RaiseTeam = dealRaised;
                tgm.TeamFolded = teamFolded;
                if (dealRaisedStartPlayer != null)
                {
                    tgm.RaisePlayer = dealRaisedStartPlayer.Id;
                    if (!dealRaiseAccepted)
                    {
                        tgm.RaisedTo = POINTS_PER_ROUND[dealPointsIndex + 1];
                    }
                }
            }
            return tgm;
        }

        private void EndRound()
        {
            Player? winner = null;
            List<int> drawnPlayerIds = [];
            int maxRank = -1;
            foreach (Player player in players.Values)
            {
                int roundRank = TRUCO_RANKS[roundCards[player.Id]];
                if (roundRank > maxRank)
                {
                    winner = player;
                    maxRank = roundRank;
                    drawnPlayerIds = [];
                }
                else if (roundRank == maxRank)
                {
                    if (winner != null) {
                        drawnPlayerIds.Add(winner.Id);
                    }
                    drawnPlayerIds.Add(player.Id);
                    winner = null;
                }
            }
            if (drawnPlayerIds.Count > 0)
            {
                // If the drawn players are of the same team, it counts as if the
                // first player to act in that round was the winner.
                HashSet<Team> hasDrawnPlayers = [];
                int firstDrawnPlayerId = -1;
                int roundStartPlayerIndex = seating.IndexOf(roundStartPlayer.Id);
                for (int i = 0; i < MAX_PLAYERS; i++)
                {
                    int offset = (roundStartPlayerIndex + i) % MAX_PLAYERS;
                    int offsetPlayerId = seating[offset];
                    if (drawnPlayerIds.Contains(offsetPlayerId))
                    {
                        if (firstDrawnPlayerId == -1)
                        {
                            firstDrawnPlayerId = offsetPlayerId;
                        }
                        hasDrawnPlayers.Add(teamOfPlayer[players[offsetPlayerId]]);
                    }
                }
                if (hasDrawnPlayers.Count == 1)
                {
                    winner = players[firstDrawnPlayerId];
                }
            }
            if (winner != null) {
                dealWinners[currentRound] = winner;
                dealTeamWinners[currentRound] = teamOfPlayer[winner];
                lastRoundWinner = winner;
                NotifyPublic(new EndRoundWinner(teamOfPlayer[winner], winner.Id, roundCards));
            }
            else
            {
                dealWinners[currentRound] = null;
                dealTeamWinners[currentRound] = null;
                lastRoundWinner = null;
                NotifyPublic(new EndRoundDrawn(drawnPlayerIds, roundCards));
            }
            lastRoundCards = new Dictionary<int, Card>(roundCards);
            currentRound++;
            Dictionary<Team, int> teamWins =
                dealWinners.FindAll(x => x != null).GroupBy(x => teamOfPlayer[x])
                    .ToDictionary(group => group.Key, group => group.Count());
            int totalWins = teamWins.Values.Sum();
            if (currentRound >= CARDS_PER_ROUND
                || teamWins.Values.Any(x => x == 2)
                || totalWins > 0 && totalWins < currentRound)
            {
                EndDeal();
            }
            else
            {
                StartRound();
            }
        }

        private void EndDeal()
        {
            Dictionary<Team, int> teamWins =
                dealWinners.FindAll(x => x != null).GroupBy(x => teamOfPlayer[x])
                    .ToDictionary(group => group.Key, group => group.Count());

            Team winner = Team.None;
            if (teamWins.GetValueOrDefault(Team.TeamOne, 0) > teamWins.GetValueOrDefault(Team.TeamTwo, 0))
            {
                winner = Team.TeamOne;
            }
            else if (teamWins.GetValueOrDefault(Team.TeamOne, 0) < teamWins.GetValueOrDefault(Team.TeamTwo, 0))
            {
                winner = Team.TeamTwo;
            }
            else
            {
                foreach (var player in dealWinners)
                {
                    if (player != null)
                    {
                        winner = teamOfPlayer[player];
                        break;
                    }
                }
            }
            dealerPlayer = activePlayer = NextPlayer(dealerPlayer);
            if (winner != Team.None)
            {
                scores[winner] += POINTS_PER_ROUND[dealPointsIndex];
                NotifyPublic([
                    new EndDeal(teamPlayers[winner].ConvertAll(x => x.Id), scores),
                    new ReadyToDeal(dealerPlayer.Id)]);
                if (scores[winner] >= POINTS_PER_GAME)
                {
                    EndGame();
                }
            }
            else
            {
                NotifyPublic([new EndDeal([], scores), new ReadyToDeal(dealerPlayer.Id)]);
            }
            readyToDeal = true;
        }

        private void EndGame()
        {
            foreach (var kvp in hands) {
                kvp.Value.Clear();
            }
        }

        private void ValidateStarted()
        {
            if (players == null)
            {
                throw new GameException(String.Format("Game {0} is not in progress", Id));
            }
        }

        private void ValidateDealt()
        {
            if (currentRound < 0)
            {
                throw new GameException(String.Format("Round is not started"));
            }
        }

        private void ValidateDealer(Player actor) {
            ValidateStarted();
            lock (roundCards) {
                if (!readyToDeal) {
                    throw new GameException(string.Format("Game {0} is not ready to deal..", Id));
                }
                if (dealerPlayer.Id != actor.Id) {
                    throw new GameException(String.Format("Player {0} is not the dealer - dealer is {1}", actor, dealerPlayer));
                }
            }
        }

        private void ValidateActor(Player actor)
        {
            ValidateStarted();
            ValidateDealt();
            lock (roundCards)
            {
                if (activePlayer.Id != actor.Id)
                {
                    throw new GameException(String.Format("Player {0} is not the active player - active is position {1}", actor, activePlayer));
                }
            }
        }

        private void ValidateTrucoNotInProgress()
        {
            if (roundCards == null)
            {
                throw new GameException(String.Format("Round is not started"));
            }
            if (!dealRaiseAccepted)
            {
                throw new GameException("Truco decision in progress");
            }
        }

        private List<Card> GetFromDeck(int num)
        {
            if (deck == null)
            {
                throw new GameException(String.Format("Game {0} was not started", Id));
            }
            lock (deck)
            {
                var cards = new List<Card>();
                for (int i = 0; i < num; i++)
                {
                    cards.Add(deck[deck.Count - 1]);
                    deck.RemoveAt(deck.Count - 1);
                }
                return cards;
            }
        }

        private int GetPlayerIndex(Player player)
        {
            int index = 0;
            while (index < MAX_PLAYERS && seating[index] != player.Id)
            {
                index++;
            }
            if (index == MAX_PLAYERS) throw new GameException(String.Format("There's no next player from {0}, seating is {1}", player, seating));
            return index;
        }

        private Player NextPlayer(Player player)
        {
            return players[seating[(GetPlayerIndex(player) + 1) % MAX_PLAYERS]];
        }

        private Player PrevPlayer(Player player)
        {
            int index = GetPlayerIndex(player);
            return players[seating[index == 0 ? MAX_PLAYERS - 1 : index - 1]];
        }

        private static List<Card> FreshTrucoDeck()
        {
            List<int> values = [1, 2, 3, 4, 5, 6, 7, 11, 12, 13];
            var cards = new List<Card>();
            foreach (Suit s in Card.Suits)
            {
                foreach (int value in values)
                {
                    cards.Add(new Card(s, value));
                }
            }
            return cards;
        }

        private static ImmutableDictionary<Card, int> CreateTrucoRanks()
        {
            var builder = ImmutableDictionary.CreateBuilder<Card, int>();
            int rank = 1;
            AddAll(builder, 4, rank++, [Suit.DIAMONDS, Suit.SPADES, Suit.HEARTS]);
            AddAll(builder, 5, rank++);
            AddAll(builder, 6, rank++);
            AddAll(builder, 7, rank++, [Suit.CLUBS, Suit.SPADES]);
            AddAll(builder, 12, rank++);
            AddAll(builder, 11, rank++);
            AddAll(builder, 13, rank++);
            AddAll(builder, 1, rank++, [Suit.DIAMONDS, Suit.HEARTS, Suit.CLUBS]);
            AddAll(builder, 2, rank++);
            AddAll(builder, 3, rank++);
            builder.Add(new Card(Suit.DIAMONDS, 7), rank++);
            builder.Add(new Card(Suit.SPADES, 1), rank++);
            builder.Add(new Card(Suit.HEARTS, 7), rank++);
            builder.Add(new Card(Suit.CLUBS, 4), rank++);
            return builder.ToImmutable();
        }

        private static void AddAll(ImmutableDictionary<Card, int>.Builder builder, int value, int rank, Suit[]? suites = null)
        {
            foreach (Suit s in suites != null ? suites : Card.Suits.ToArray())
            {
                builder.Add(new Card(s, value), rank);
            }
        }

        internal record PrivatePublicNotification(List<int> playerIds, TrucoNotification privateNotification, TrucoNotification publicNotification);
    }
}

