using CardgameModel;
using CardgameModel.Truco;
using CardgameServer.player;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices.Marshalling;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CardgameServer.game.truco
{
    [Route("game/truco")]
    [ApiController]
    public class TrucoGameService : ControllerBase
    {
        private static readonly int DEFAULT_LONG_POLL_TIMEOUT_SECONDS = 30;

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
        public async Task<ActionResult<TrucoGameModel>> Get(int id, [FromQuery] int? playerid)
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

        // GET api/<TrucoGameService>/5
        [HttpGet("{id}/notifications")]
        public async Task<ActionResult<IEnumerable<TrucoNotification>>> GetNotifications(
                CancellationToken userCt,
                int id,
                [FromQuery] int? playerid,
                [FromQuery] int? startAt,
                [FromQuery] int? timeout)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(userCt);
            cts.CancelAfter(TimeSpan.FromSeconds(timeout.GetValueOrDefault(DEFAULT_LONG_POLL_TIMEOUT_SECONDS)));

            var timeoutTask = Task.Delay(-1, cts.Token);
            var gnp = await ValidateGameAndPlayer(id, playerid);
            if (gnp.Value != null)
            {
                var notificationTask = gnp.Value.Game.WaitForNotifications(cts, gnp.Value.Player, startAt.GetValueOrDefault(0));
                Task completedTask = await Task.WhenAny(notificationTask, timeoutTask);
                if (completedTask == notificationTask)
                {
                    var notifications = await notificationTask;
                    return Ok(notifications);
                }
                return NoContent();
            }
            else
            {
                return gnp.Result;
            }
        }

        // POST api/<TrucoGameService>
        [HttpPost]
        public TrucoGameModel Post()
        {
            return trucoGames.Create().ToModel();
        }


        // POST api/<TrucoGameService>/<gameid>/join
        [HttpPost("{id}/join")]
        public async Task<ActionResult<TrucoGameModel>> JoinGame(int id, [FromBody] PlayerIdentity player)
        {
            var gnp = await ValidateGameAndPlayer(id, player.PlayerId);
            if (gnp.Value != null) {
                gnp.Value.Game.AddPlayer(gnp.Value.Player);
                return gnp.Value.Game.ToModel(gnp.Value.Player);
            } else {
                return gnp.Result;
            }
        }

        // POST api/<TrucoGameService>/join
        [HttpPost("join")]
        public async Task<ActionResult<TrucoGameModel>> CreateOrJoinGame([FromBody] string playerName) {
            Player player = new Player(random.Next(), playerName);
            playerContext.Players.Add(player);
            await playerContext.SaveChangesAsync();
            var game = trucoGames.CreateOrJoin(player);
            return game.ToModel();
        }

        // POST api/<TrucoGameService>/play?playerid=id
        [HttpPost("{id}/play")]
        public async Task<ActionResult> PlayCard(
            int id,
            [FromBody] Card card,
            [FromQuery] int playerid)
        {
            var gnp = await ValidateGameAndPlayer(id, playerid);
            if (gnp.Value != null) {
                gnp.Value.Game.Play(gnp.Value.Player, card);
                return NoContent();
            } else {
                return gnp.Result;
            }
        }

        // POST api/<TrucoGameService>/deal?playerid=id
        [HttpPost("{id}/deal")]
        public async Task<ActionResult> Deal(
            int id,
            [FromQuery] int playerid) {
            var gnp = await ValidateGameAndPlayer(id, playerid);
            if (gnp.Value != null) {
                gnp.Value.Game.Deal(gnp.Value.Player);
                return NoContent();
            } else {
                return gnp.Result;
            }
        }

        // POST api/<TrucoGameService>/raise?playerid=id
        [HttpPost("{id}/raise")]
        public async Task<ActionResult> Raise(
            int id,
            [FromQuery] int playerid) {
            var gnp = await ValidateGameAndPlayer(id, playerid);
            if (gnp.Value != null) {
                gnp.Value.Game.Truco(gnp.Value.Player);
                return NoContent();
            } else {
                return gnp.Result;
            }
        }

        // POST api/<TrucoGameService>/accept?playerid=id
        [HttpPost("{id}/accept")]
        public async Task<ActionResult> Accept(
            int id,
            [FromQuery] int playerid) {
            var gnp = await ValidateGameAndPlayer(id, playerid);
            if (gnp.Value != null) {
                gnp.Value.Game.Accept(gnp.Value.Player);
                return NoContent();
            } else {
                return gnp.Result;
            }
        }

        // POST api/<TrucoGameService>/fold?playerid=id
        [HttpPost("{id}/fold")]
        public async Task<ActionResult> Fold(
            int id,
            [FromQuery] int playerid) {
            var gnp = await ValidateGameAndPlayer(id, playerid);
            if (gnp.Value != null) {
                gnp.Value.Game.Fold(gnp.Value.Player);
                return NoContent();
            } else {
                return gnp.Result;
            }
        }

        internal async Task<ActionResult<GameAndPlayer>> ValidateGameAndPlayer(int? gameId, int? playerId) {
            if (gameId == null) {
                return BadRequest("GameId is required");
            }
            if (playerId == null) {
                return BadRequest("PlayerId is required");
            }
            TrucoGame? game = trucoGames.Get(gameId.Value);
            if (game == null) {
                return NotFound(string.Format("Truco game {0} not found", gameId));
            }
            Player? player = await playerContext.Players.FindAsync(playerId);
            if (player == null) {
                return NotFound(string.Format("Player ID {0} not found", playerId));
            }
            return new GameAndPlayer(game, player);
        }

        public record PlayerIdentity(int PlayerId);
        internal record GameAndPlayer(TrucoGame Game, Player Player);

    }
}
