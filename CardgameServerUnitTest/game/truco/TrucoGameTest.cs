using CardgameModel;
using CardgameModel.Truco;
using CardgameServer;
using CardgameServer.game;
using CardgameServer.game.truco;

namespace CardgameServerUnitTest.game.truco
{
    [TestClass]
    public class TrucoGameTest
    {
        private static readonly Player PLAYER_1 = new Player(1, "P1");
        private static readonly Player PLAYER_2 = new Player(2, "P2");
        private static readonly Player PLAYER_3 = new Player(3, "P3");
        private static readonly Player PLAYER_4 = new Player(4, "P4");

        private static readonly List<int> SEATING = [1, 2, 3, 4];

        private static readonly Dictionary<int, Player> PLAYERS =
            new Dictionary<int, Player>()
            {
                { 1, PLAYER_1 },
                { 2, PLAYER_2 },
                { 3, PLAYER_3 },
                { 4, PLAYER_4 }
            };

        private static readonly Dictionary<Team, List<int>> TEAMS =
            new Dictionary<Team, List<int>>() {
                { Team.TeamOne, [ PLAYER_1.Id, PLAYER_3.Id] }, { Team.TeamTwo, [ PLAYER_2.Id, PLAYER_4.Id ] } };

        private static readonly Dictionary<int, Team> TEAM_OF_PLAYER = new Dictionary<int, Team>() {
            { PLAYER_1.Id, Team.TeamOne }, { PLAYER_2.Id, Team.TeamTwo }, { PLAYER_3.Id, Team.TeamOne }, { PLAYER_4.Id, Team.TeamTwo } };

        [TestMethod]
        [ExpectedException(typeof(GameException))]
        public void testJoinPlayersNotEnough()
        {
            TrucoGame truco = new(0, new NoShuffler<Card>(), new NoShuffler<int>());
            truco.AddPlayer(PLAYER_1);
            truco.Play(PLAYER_1, Card.Blank);
        }

        [TestMethod]
        public void testRoundTeam2_TwoWins_NoLoss_NoDraw()
        {
            List<Card> cards = [
                Card.Of("d2"), Card.Of("dA"), Card.Of("d7"),
                Card.Of("c2"), Card.Of("c4"), Card.Of("cJ"),
                Card.Of("hK"), Card.Of("h2"), Card.Of("h7"),
                Card.Of("sA"), Card.Of("sQ"), Card.Of("s2")];
            cards.Reverse();
            TrucoGame truco = new(0, new PrecogShuffler<Card>(cards), new PrecogShuffler<int>(SEATING));

            List<R> rounds = [
                R.Of(4, 1, 2, "c2", 3, "hK", 4, "sA", 1, "dA"),
                R.Of(2, 1, 4, "sQ", 1, "d7", 2, "c4", 3, "h7")
                ];

            JoinPlayers(truco);
            truco.Deal(PLAYER_1);
            int startPlayerId = 2;
            foreach (R r in rounds)
            {
                startPlayerId = DoRound(truco, startPlayerId, r);
            }
            TrucoGameModel tgm = truco.ToModel();
            Assert.IsTrue(tgm.Scores[Team.TeamOne] == 0);
            Assert.IsTrue(tgm.Scores[Team.TeamTwo] == 1);
        }

        [TestMethod]
        public void testTwoRoundsTeam1_TwoWins()
        {
            List<Card> cards = [
                Card.Of("d2"), Card.Of("d6"), Card.Of("dK"),
                Card.Of("c2"), Card.Of("c6"), Card.Of("cK"),
                Card.Of("h2"), Card.Of("h6"), Card.Of("hK"),
                Card.Of("sQ"), Card.Of("s6"), Card.Of("sK")];
            cards.Reverse();
            TrucoGame truco = new(0, new PrecogShuffler<Card>(cards), new PrecogShuffler<int>(SEATING));

            List<R> round = [
                R.Of(2, 1, 2, "c2", 3, "hK", 4, "sQ", 1, "d6"),
                R.Of(1, 1, 2, "c6", 3, "h6", 4, "s6", 1, "d2"),
                R.Of(3, 1, 1, "dK", 2, "cK", 3, "h2", 4, "sK")];

            JoinPlayers(truco);
            for (int i = 0; i < 2; i++)
            {
                int dealerPlayerId = i + 1;
                truco.Deal(PLAYERS[dealerPlayerId]);
                int startPlayerId = dealerPlayerId + 1;
                foreach (R r in round)
                {
                    startPlayerId = DoRound(truco, startPlayerId, r);
                }
            }
            TrucoGameModel tgm = truco.ToModel();
            Assert.IsTrue(tgm.Scores[Team.TeamOne] == 2);
            Assert.IsTrue(tgm.Scores[Team.TeamTwo] == 0);
        }

        [TestMethod]
        public void testRoundTeam2_Draw_Win() {
            List<Card> cards = [
                Card.Of("d2"), Card.Of("dA"), Card.Of("d7"),
                Card.Of("c2"), Card.Of("c4"), Card.Of("cJ"),
                Card.Of("hK"), Card.Of("h2"), Card.Of("h7"),
                Card.Of("sA"), Card.Of("sQ"), Card.Of("s2")];
            cards.Reverse();
            TrucoGame truco = new(0, new PrecogShuffler<Card>(cards), new PrecogShuffler<int>(SEATING));

            List<R> rounds = [
                R.Of([2, 3], 1, 2, "c2", 3, "h2", 4, "sQ", 1, "dA"),
                R.Of(2, 1, 2, "c4", 3, "hK", 4, "s2", 1, "d7")
                ];

            JoinPlayers(truco);
            TrucoGameModel tgm = truco.ToModel(PLAYER_1);
            truco.Deal(PLAYER_1);
            int startPlayerId = 2;
            foreach (R r in rounds) {
                startPlayerId = DoRound(truco, startPlayerId, r);
            }
            tgm = truco.ToModel(PLAYER_1);
            Assert.IsTrue(tgm.Scores[Team.TeamOne] == 0);
            Assert.IsTrue(tgm.Scores[Team.TeamTwo] == 1);
        }

        [TestMethod]
        public void testRoundTeam2_Win_Draw() {
            List<Card> cards = [
                Card.Of("d2"), Card.Of("dA"), Card.Of("d7"),
                Card.Of("c2"), Card.Of("c4"), Card.Of("cJ"),
                Card.Of("hK"), Card.Of("h2"), Card.Of("h7"),
                Card.Of("sA"), Card.Of("sQ"), Card.Of("s2")];
            cards.Reverse();
            TrucoGame truco = new(0, new PrecogShuffler<Card>(cards), new PrecogShuffler<int>(SEATING));

            List<R> rounds = [
                R.Of(2, 1, 2, "c4", 3, "hK", 4, "sQ", 1, "d2"),
                R.Of([2, 3, 4], 1, 2, "c2", 3, "h2", 4, "s2", 1, "dA")
                ];

            JoinPlayers(truco);
            TrucoGameModel tgm = truco.ToModel(PLAYER_1);
            truco.Deal(PLAYER_1);
            int startPlayerId = 2;
            foreach (R r in rounds) {
                startPlayerId = DoRound(truco, startPlayerId, r);
            }
            tgm = truco.ToModel(PLAYER_1);
            Assert.IsTrue(tgm.Scores[Team.TeamOne] == 0);
            Assert.IsTrue(tgm.Scores[Team.TeamTwo] == 1);
        }

        [TestMethod]
        public void testRoundTeam2_Wiu_Loss_Truco_Win() {
            List<Card> cards = [
                Card.Of("d2"), Card.Of("dA"), Card.Of("d7"),
                Card.Of("c2"), Card.Of("c4"), Card.Of("cJ"),
                Card.Of("hK"), Card.Of("h2"), Card.Of("h7"),
                Card.Of("sA"), Card.Of("sQ"), Card.Of("s2")];
            cards.Reverse();
            TrucoGame truco = new(0, new PrecogShuffler<Card>(cards), new PrecogShuffler<int>(SEATING));

            List<R> rounds = [
                R.Of(1, 1, 2, "c2", 3, "h2", 4, "sQ", 1, "d7"),
                R.Of(2, 1, 1, "d2", 2, "c4", 3, "hK", 4, "s2"),
                ];

            JoinPlayers(truco);
            TrucoGameModel tgm = truco.ToModel(PLAYER_1);
            truco.Deal(PLAYER_1);
            int startPlayerId = 2;
            foreach (R r in rounds) {
                startPlayerId = DoRound(truco, startPlayerId, r);
            }
            int startAt = truco.Notifications().Count > 0 ? truco.Notifications().Last().Id : 0;
            truco.Truco(PLAYER_2);
            truco.Accept(PLAYER_3);
            R lastRound = R.Of(3, 3, 2, "cJ", 3, "h7", 4, "sA", 1, "dA");
            startPlayerId = DoRound(truco, startPlayerId, lastRound);
            ExpectPublicNotification(
                truco,
                startAt,
                [
                    new CallTruco(PLAYER_2.Id, 3, PLAYER_3.Id),
                    new AcceptTruco(PLAYER_3.Id, 3),
                    new EndRoundWinner(Team.TeamOne, PLAYER_3.Id, lastRound.Cards),
                    new EndDeal([PLAYER_1.Id, PLAYER_3.Id], new() { { Team.TeamOne, 3 }, { Team.TeamTwo, 0 } })
                ]);
            tgm = truco.ToModel(PLAYER_1);
            Assert.IsTrue(tgm.Scores[Team.TeamOne] == 3);
            Assert.IsTrue(tgm.Scores[Team.TeamTwo] == 0);
        }

        [TestMethod]
        public void testRoundTeam2_Wiu_Loss_Truco_Fold() {
            List<Card> cards = [
                Card.Of("d2"), Card.Of("dA"), Card.Of("d7"),
                Card.Of("c2"), Card.Of("c4"), Card.Of("cJ"),
                Card.Of("hK"), Card.Of("h2"), Card.Of("h7"),
                Card.Of("sA"), Card.Of("sQ"), Card.Of("s2")];
            cards.Reverse();
            TrucoGame truco = new(0, new PrecogShuffler<Card>(cards), new PrecogShuffler<int>(SEATING));

            List<R> rounds = [
                R.Of(1, 1, 2, "c2", 3, "h2", 4, "sQ", 1, "d7"),
                R.Of(2, 1, 1, "d2", 2, "c4", 3, "hK", 4, "s2"),
                ];

            JoinPlayers(truco);
            TrucoGameModel tgm = truco.ToModel(PLAYER_1);
            truco.Deal(PLAYER_1);
            int startPlayerId = 2;
            foreach (R r in rounds) {
                startPlayerId = DoRound(truco, startPlayerId, r);
            }
            int startAt = truco.Notifications().Count > 0 ? truco.Notifications().Last().Id : 0;
            truco.Truco(PLAYER_2);
            truco.Fold(PLAYER_3);
            ExpectPublicNotification(
                truco,
                startAt,
                [
                    new CallTruco(PLAYER_2.Id, 3, PLAYER_3.Id),
                    new Fold(PLAYER_3.Id),
                    new EndDeal([PLAYER_2.Id, PLAYER_4.Id], new() { { Team.TeamOne, 0 }, { Team.TeamTwo, 1 } })
                ]);
            tgm = truco.ToModel(PLAYER_1);
            Assert.IsTrue(tgm.Scores[Team.TeamOne] == 0);
            Assert.IsTrue(tgm.Scores[Team.TeamTwo] == 1);
            Assert.IsTrue(tgm.TeamFolded == Team.TeamOne);
        }

        [TestMethod]
        public void testRoundTeam2_Wiu_Loss_Truco_Six_Win() {
            List<Card> cards = [
                Card.Of("d2"), Card.Of("dA"), Card.Of("d7"),
                Card.Of("c2"), Card.Of("c4"), Card.Of("cJ"),
                Card.Of("hK"), Card.Of("h2"), Card.Of("h7"),
                Card.Of("sA"), Card.Of("sQ"), Card.Of("s2")];
            cards.Reverse();
            TrucoGame truco = new(0, new PrecogShuffler<Card>(cards), new PrecogShuffler<int>(SEATING));

            List<R> rounds = [
                R.Of(1, 1, 2, "c2", 3, "h2", 4, "sQ", 1, "d7"),
                R.Of(2, 1, 1, "d2", 2, "c4", 3, "hK", 4, "s2"),
                ];

            JoinPlayers(truco);
            TrucoGameModel tgm = truco.ToModel(PLAYER_1);
            truco.Deal(PLAYER_1);
            int startPlayerId = 2;
            foreach (R r in rounds) {
                startPlayerId = DoRound(truco, startPlayerId, r);
            }
            int startAt = truco.Notifications().Count > 0 ? truco.Notifications().Last().Id : 0;
            truco.Truco(PLAYER_2);
            truco.Truco(PLAYER_3);
            truco.Accept(PLAYER_2);
            R lastRound = R.Of(3, 6, 2, "cJ", 3, "h7", 4, "sA", 1, "dA");
            startPlayerId = DoRound(truco, startPlayerId, lastRound);
            ExpectPublicNotification(
                truco,
                startAt,
                [
                    new CallTruco(PLAYER_2.Id, 3, PLAYER_3.Id),
                    new CallTruco(PLAYER_3.Id, 6, PLAYER_2.Id),
                    new AcceptTruco(PLAYER_2.Id, 6),
                    new EndRoundWinner(Team.TeamOne, PLAYER_3.Id, lastRound.Cards),
                    new EndDeal([PLAYER_1.Id, PLAYER_3.Id], new() { { Team.TeamOne, 6 }, { Team.TeamTwo, 0 } })
                ]);
            tgm = truco.ToModel(PLAYER_1);
            Assert.IsTrue(tgm.Scores[Team.TeamOne] == 6);
            Assert.IsTrue(tgm.Scores[Team.TeamTwo] == 0);
        }

        private static int DoRound(TrucoGame truco, int startPlayer, R r)
        {
            int currentPlayer = startPlayer;
            Dictionary<int, Card> played = [];
            int startAt = 0;
            do
            {
                ExpectPublicNotification(
                    truco,
                    startAt,
                    [new SetActivePlayer(currentPlayer, r.points)]);
                Card card = r.Cards[currentPlayer];
                played.Add(currentPlayer, card);
                startAt = truco.Notifications().Count > 0 ? truco.Notifications().Last().Id : 0;
                truco.Play(PLAYERS[currentPlayer], card);
                // I'm to get the next player, player IDs are 1-based, more stable would be using generic IDs and SEATING index
                int nextPlayer = (currentPlayer % PLAYERS.Count) + 1;
                ExpectPublicNotification(
                    truco,
                    startAt,
                    [ new PlayCard(currentPlayer, card, played )]);
                currentPlayer = nextPlayer;
            } while (currentPlayer != startPlayer);
            if (r.Winner.HasValue)
            {
                ExpectPublicNotification(
                    truco,
                    startAt,
                    [new EndRoundWinner(TEAM_OF_PLAYER[r.Winner.Value], r.Winner.Value, played)]);
            }
            else if (r.Drawn != null)
            {
                ExpectPublicNotification(
                    truco,
                    startAt,
                    [new EndRoundDrawn(r.Drawn, played)]);
            }
            return startPlayer = r.Winner.HasValue ? r.Winner.Value : startPlayer;
        }

        internal record R(int? Winner, List<int>? Drawn, int points, Dictionary<int, Card> Cards)
        {
            private static Dictionary<int, Card> ProcessCardParams(params Object[] playerAndCard)
            {
                var cards = new Dictionary<int, Card>();
                for (int i = 0; i < playerAndCard.Length; i += 2) {
                    cards.Add((int)playerAndCard[i], Card.Of((string)playerAndCard[i + 1]));
                }
                return cards;
            }

            internal static R Of(int winner, int points, params Object[] playerAndCard)
            {
                return new R(winner, null, points, ProcessCardParams(playerAndCard));
            }

            internal static R Of(List<int> drawn, int points, params Object[] playerAndCard) {
                return new R(null, drawn, points, ProcessCardParams(playerAndCard));
            }
        }

        private static void JoinPlayers(TrucoGame truco)
        {
            var startAt = truco.Notifications().Count() > 0 ? truco.Notifications().Last().Id : 0;
            truco.AddPlayer(PLAYER_1);
            truco.AddPlayer(PLAYER_2);
            truco.AddPlayer(PLAYER_3);
            truco.AddPlayer(PLAYER_4);
            ExpectPublicNotification(
                truco,
                startAt,
                [new AddPlayer(PLAYER_1),
                    new AddPlayer(PLAYER_2),
                    new AddPlayer(PLAYER_3),
                    new AddPlayer(PLAYER_4),
                    new StartGame(new List<Player>(PLAYERS.Values), SEATING, TEAMS)]);
        }

        private static void ExpectPublicNotification(TrucoGame truco, int startAt, TrucoNotification[] expected)
        {
            var actual = truco.Notifications();
            var expectedIndex = 0;
            foreach(var notification in actual)
            {
                if (notification.Id < startAt)
                {
                    continue;
                }
                if (notification.PublicPart.Equals(expected[expectedIndex]))
                {
                    if (++expectedIndex >= expected.Length)
                    {
                        return;
                    }
                }
            }
            Console.WriteLine(Neat(actual));
            Console.WriteLine(Neat(expected));
            throw new Exception(
                String.Format(
                    "Missing expected event: started={0} expectedIndex={1}\nactual={2}\nexpected={3}",
                    startAt,
                    expectedIndex,
                    Neat(actual.Select(x => x.PublicPart).ToList(), startAt),
                    Neat(expected)));
        }

        private static string Neat<T>(ICollection<T> collection, int skip = 0)
        {
            return "{ " + string.Join(",\n", collection.Skip(skip)) + " }";
        }

        internal class NoShuffler<T> : IShuffler<T>
        {
            public void Shuffle(List<T> input) { }
        }

        internal class PrecogShuffler<T> : IShuffler<T>
        {
            private readonly List<T> precog;

            internal PrecogShuffler(List<T> precog)
            {
                this.precog = precog;
            }

            public void Shuffle(List<T> input)
            {
                input.Clear();
                foreach(T t in precog)
                {
                    input.Add(t);
                }
            }
        }
    }
}
