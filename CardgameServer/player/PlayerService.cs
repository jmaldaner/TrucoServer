using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;

namespace CardgameServer.player
{
    [Route("player")]
    [ApiController]
    public class PlayerService : ControllerBase
    {
        private static readonly Random random = new Random();

        private readonly PlayerContext db;

        public PlayerService(PlayerContext playerDb)
        {
            db = playerDb;
        }

        // GET api/<PlayerService>/5
        [HttpGet("")]
        public async Task<ActionResult<Player>> Get([FromQuery] string email)
        {
            var player = await db.Players.SingleOrDefaultAsync(player => player.Name == email);
            if (player == null)
            {
                return NotFound();
            }
            return player;
        }

        // GET api/<PlayerService>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> Get(long id)
        {
            var player = await db.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return player;
        }

        // POST api/<PlayerService>
        [HttpPost]
        public async Task<ActionResult<Player>> Post([FromBody] Player player)
        {
            player.Id = random.NextInt64();
            db.Players.Add(player);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = player.Id }, player);
        }

        // PUT api/<PlayerService>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, Player player)
        {
            if (id != player.Id)
            {
                return BadRequest("PUT mismatch Id from request and updated entity body");
            }

            db.Entry(player).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(id))
                {
                    return NotFound();
                } else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE api/<PlayerService>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var player = await db.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            db.Players.Remove(player);
            await db.SaveChangesAsync();

            return NoContent();
        }

        private bool PlayerExists(long id)
        {
            return db.Players.Any(e => e.Id == id);
        }
    }
}
