using CardgameModel;

namespace CardgameServer.game.truco
{
    public class TrucoGames
    {
        private readonly Random random = new Random();
        private Dictionary<long, TrucoGame> games = [];

        public TrucoGame Create()
        {
            long id = random.NextInt64();
            var game = new TrucoGame(id, new Shuffler<Card>(), new Shuffler<long>());
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

        public TrucoGame? Get(long id)
        {
            return games[id];
        }
    }
}
