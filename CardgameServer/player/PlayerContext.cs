namespace CardgameServer.player
{
    using CardgameModel;
    using Microsoft.EntityFrameworkCore;

    public class PlayerContext : DbContext
    {
        public PlayerContext(DbContextOptions<PlayerContext> options) : base(options) { }

        public DbSet<Player> Players => Set<Player>();
    }
}
