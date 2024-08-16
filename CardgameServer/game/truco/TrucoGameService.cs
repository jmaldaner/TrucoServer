using CardgameServer.cards;
using CardgameServer.player;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardgameServer.game.truco
{
    [Route("game/truco")]
    [ApiController]
    public class TrucoGameService : ControllerBase
    {
        private Random random = new Random();

        private readonly PlayerContext playerContext;
        private readonly TrucoGames trucoGames;
        public TrucoGameService(
            TrucoGames trucoGames,
            PlayerContext playerContext)
        {
            this.trucoGames = trucoGames;
            this.playerContext = playerContext;
        }

        // GET api/<TrucoGameService>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TrucoGameModel>> Get(long id, [FromQuery] long? playerid)
        {
            TrucoGame? game = trucoGames.Get(id);
            if (game == null)
            {
                return NotFound("id");
            }
            if (playerid.HasValue)
            {
                Player? player = await playerContext.Players.FindAsync(playerid.Value);
                if (player == null)
                {
                    return NotFound("playerid");
                }
                return game.ToModel(player);
            }
            else
            {
                return game.ToModel();
            }
        }

        // POST api/<TrucoGameService>
        [HttpPost]
        public TrucoGameModel Post()
        {
            return trucoGames.Create().ToModel();
        }


        // POST api/<TrucoGameService>/join
        [HttpPost("{id}/join")]
        public async Task<ActionResult<TrucoGameModel>> JoinGame(long id, [FromBody] Player inputPlayer)
        {
            TrucoGame? game = trucoGames.Get(id);
            if (game == null)
            {
                return NotFound(string.Format("Truco game {0} not found", id));
            }
            if (inputPlayer == null) 
            {
                return NotFound("Player entity was not provided");
            }
            Player? player = await playerContext.Players.FindAsync(inputPlayer.Id);
            if (player == null)
            {
                return NotFound(string.Format("Player ID {0} not found", inputPlayer.Id));
            }
            game.AddPlayer(player);
            return game.ToModel(player);
        }

        // POST api/<TrucoGameService>/play?playerid=id
        [HttpPost("{id}/play")]
        public async Task<ActionResult> PlayCard(
            long id,
            [FromBody] Card card,
            [FromQuery] long playerid)
        {
            TrucoGame? game = trucoGames.Get(id);
            if (game == null)
            {
                return NotFound(string.Format("Truco game {0} not found", id));
            }
            Player? player = await playerContext.Players.FindAsync(playerid);
            if (player == null)
            {
                return NotFound(string.Format("Player ID {0} not found", playerid));
            }
            game.Play(player, card);
            return NoContent();
        }

    }
}
