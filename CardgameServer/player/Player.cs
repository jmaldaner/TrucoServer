namespace CardgameServer.player
{
    public class Player
    {

        public long Id { get; set; }
        public string Name { get; set; }

        public Player(long id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public override string ToString()
        {
            return "Player { Id=" + Id + " Name=" + Name + " }";
        }
    }
}
