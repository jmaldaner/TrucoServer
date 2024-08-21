using System.Collections.Generic;
using System.Text;

namespace CardgameModel.Truco {
    public class TrucoGameModel
    {
        public long Id { get; set; }
        public List<long> Seating {  get; set; }
        public List<long> RelativeSeating { get; set; }
        public Dictionary<long, Player> Players { get; set; }
        public Dictionary<long, Card> RoundCards { get; set; }
        public Dictionary<long, Card> LastRound {  get; set; }
        public Dictionary<long, int> HandSizes { get; set; }
        public Dictionary<Team, List<long>> Teams { get; set; }
        public Dictionary<Team, int> Scores { get; set; }
        public int Round {  get; set; }
        public long ActivePlayer { get; set; }
        public List<Card> PrivateHand {  get; set; }
        public bool IsAcceptingPlayers { get; set; }
        public long LastRoundWinner {  get; set; }
        public bool ReadyToDeal { get; set; }
        public long DealerPlayer { get; set; }
        public List<Team?> DealWinners { get; set; }
        public int RaisedTo { get; set; }
        public long RaisePlayer { get; set; }
        public bool RaiseAccepting { get; set; }
        public int PointsThisRound { get; set; }
        public Team RaiseTeam { get; set; }
        public Team TeamFolded { get; set; }
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("id=");
            sb.Append(Id);
            sb.Append("\nplayers=");
            sb.Append(Players);
            sb.Append("\nround=");
            sb.Append(RoundCards);
            sb.Append("\nhands=");
            sb.Append(HandSizes);
            sb.Append("\nteams=");
            sb.Append(Teams);
            sb.Append("\nscores=");
            sb.Append(Scores);
            sb.Append("\nprivate_hand=");
            sb.Append(PrivateHand);
            return sb.ToString();
        }

    }
    
}
