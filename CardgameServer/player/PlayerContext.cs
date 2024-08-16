namespace CardgameServer.player
{
    using Microsoft.EntityFrameworkCore;

    public class PlayerContext : DbContext
    {
        public PlayerContext(DbContextOptions<PlayerContext> options) : base(options) { }

        public DbSet<Player> Players => Set<Player>();
    }
}
