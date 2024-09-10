using CardgameModel.Truco;
using System;

namespace CardgameModel {
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Player(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }

        public override string ToString() {
            return "Player { Id=" + Id + " Name=" + Name + " }";
        }

        public override bool Equals(Object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var other = (Player) obj;
            return
                other != null
                && Id == other.Id
                && Name == other.Name;
        }

        public override int GetHashCode() {
            return Id.GetHashCode() ^ Name.GetHashCode();
        }
    }
}
