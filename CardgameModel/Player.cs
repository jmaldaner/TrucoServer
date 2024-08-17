namespace CardgameModel {
    public class Player
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public Player(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return "Player { Id=" + Id + " Name=" + Name + " }";
        }
    }
}
