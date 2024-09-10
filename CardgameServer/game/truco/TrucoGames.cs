using CardgameModel;

namespace CardgameServer.game.truco
{
    public class TrucoGames
    {
        private readonly Random random = new Random();
        private Dictionary<int, TrucoGame> games = [];

        public TrucoGame Create()
        {
            int id = random.Next();
            var game = new TrucoGame(id, new Shuffler<Card>(), new Shuffler<int>());
            games[id] = game;
            return game;
        }

        public TrucoGame CreateOrJoin(Player player)
        {
            TrucoGame trucoGame = null;
            foreach (var game in games.Values)
            {
                if (game.IsAcceptingPlayers())
                {
                    trucoGame = game;
                    break;
                }
            }
            if (trucoGame == null)
            {
                trucoGame = Create();
            }
            trucoGame.AddPlayer(player);
            return trucoGame;
        }

        public TrucoGame? Get(int id)
        {
            return games[id];
        }
    }
}
