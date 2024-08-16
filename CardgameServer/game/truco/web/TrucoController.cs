using CardgameServer.cards;
using CardgameServer.player;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;

namespace CardgameServer.game.truco.web
{
    public class TrucoController : Controller
    {
        private static Random random = new Random(System.DateTime.Now.Nanosecond);
        private readonly PlayerContext playerContext;
        private readonly TrucoGames trucoGames;

        public TrucoController(
            PlayerContext playerContext, 
            TrucoGames trucoGames
            )
        {
            this.playerContext = playerContext;
            this.trucoGames = trucoGames;
        }

        public async Task<IActionResult> Index([FromQuery] long? game, [FromQuery] long? playerId)
        {
            if (game == null || playerId == null)
            {
                return Redirect("Truco/Login");
            }
            TrucoGame? trucoGame = trucoGames.Get(game.Value);
            if (trucoGame == null)
            {
                return NotFound(string.Format("Truco game {0} not found", game));
            }
            Player? player = await playerContext.Players.FindAsync(playerId);
            if (player == null)
            {
                return NotFound(string.Format("Used {0} not found", playerId));
            }
            ViewData["player"] = player;
            return View(trucoGame.ToModel(player));
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Deal(
            [FromQuery] long? game,
            [FromQuery] long? playerId)
        {
            if (game == null || playerId == null) {
                return Redirect("/Truco/Login");
            }
            TrucoGame? trucoGame = trucoGames.Get(game.Value);
            if (trucoGame == null) {
                return NotFound(string.Format("Truco game {0} not found", game));
            }
            Player? player = await playerContext.Players.FindAsync(playerId);
            if (player == null) {
                return NotFound(string.Format("Used {0} not found", playerId));
            }
            trucoGame.Deal(player);
            return Redirect(string.Format("Index?game={0}&playerid={1}", game, player.Id));
        }

        public async Task<IActionResult> Auth([FromForm] string? name)
        {
            if (name == null)
            {
                return BadRequest(string.Format("Player name was not provided"));
            }
            var player = await playerContext.Players.SingleOrDefaultAsync(p => p.Name == name);
            if (player == null)
            {
                player = new Player(random.NextInt64(), name);
                playerContext.Players.Add(player);
                await playerContext.SaveChangesAsync();
            }
            TrucoGame game = trucoGames.CreateOrJoin(player);
            return Redirect("/?game=" + game.Id + "&playerid=" + player.Id);
        }

        public async Task<IActionResult> Play(
                [FromQuery] long? game,
                [FromQuery] long? playerId,
                [FromQuery] Suit? suit,
                [FromQuery] int? value) {
            if (game == null || playerId == null || suit == null || value == null)
            {
                return Redirect("/Truco/Login");
            }
            TrucoGame? trucoGame = trucoGames.Get(game.Value);
            if (trucoGame == null) {
                return NotFound(string.Format("Truco game {0} not found", game));
            }
            Player? player = await playerContext.Players.FindAsync(playerId);
            if (player == null) {
                return NotFound(string.Format("Used {0} not found", playerId));
            }
            Card card = new Card(suit.Value, value.Value);
            trucoGame.Play(player, card);
            return Redirect(string.Format("Index?game={0}&playerid={1}", game, player.Id));
        }

        public async Task<IActionResult> Raise(
                [FromQuery] long? game,
                [FromQuery] long? playerId) {
            if (game == null || playerId == null) {
                return Redirect("/Truco/Login");
            }
            TrucoGame? trucoGame = trucoGames.Get(game.Value);
            if (trucoGame == null) {
                return NotFound(string.Format("Truco game {0} not found", game));
            }
            Player? player = await playerContext.Players.FindAsync(playerId);
            if (player == null) {
                return NotFound(string.Format("Used {0} not found", playerId));
            }
            trucoGame.Truco(player);
            return Redirect(string.Format("Index?game={0}&playerid={1}", game, player.Id));
        }

        public async Task<IActionResult> Accept(
        [FromQuery] long? game,
        [FromQuery] long? playerId) {
            if (game == null || playerId == null) {
                return Redirect("/Truco/Login");
            }
            TrucoGame? trucoGame = trucoGames.Get(game.Value);
            if (trucoGame == null) {
                return NotFound(string.Format("Truco game {0} not found", game));
            }
            Player? player = await playerContext.Players.FindAsync(playerId);
            if (player == null) {
                return NotFound(string.Format("Used {0} not found", playerId));
            }
            trucoGame.Accept(player);
            return Redirect(string.Format("Index?game={0}&playerid={1}", game, player.Id));
        }

        public async Task<IActionResult> Fold(
            [FromQuery] long? game,
            [FromQuery] long? playerId) {
            if (game == null || playerId == null) {
                return Redirect("/Truco/Login");
            }
            TrucoGame? trucoGame = trucoGames.Get(game.Value);
            if (trucoGame == null) {
                return NotFound(string.Format("Truco game {0} not found", game));
            }
            Player? player = await playerContext.Players.FindAsync(playerId);
            if (player == null) {
                return NotFound(string.Format("Used {0} not found", playerId));
            }
            trucoGame.Fold(player);
            return Redirect(string.Format("Index?game={0}&playerid={1}", game, player.Id));
        }
    }
}
