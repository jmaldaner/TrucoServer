using System.Collections.Generic;
using System.Text;

namespace CardgameModel.Truco {
    public class TrucoGameModel
    {
        public int Id { get; set; }
        public List<int> Seating {  get; set; }
        public List<int> RelativeSeating { get; set; }
        public Dictionary<int, Player> Players { get; set; }
        public Dictionary<int, Card> RoundCards { get; set; }
        public Dictionary<int, Card> LastRound {  get; set; }
        public Dictionary<int, int> HandSizes { get; set; }
        public Dictionary<Team, List<int>> Teams { get; set; }
        public Dictionary<Team, int> Scores { get; set; }
        public int Round {  get; set; }
        public int ActivePlayer { get; set; }
        public List<Card> PrivateHand {  get; set; }
        public List<Card> PartnerHand { get; set; }
        public bool IsAcceptingPlayers { get; set; }
        public int LastRoundWinner {  get; set; }
        public bool ReadyToDeal { get; set; }
        public int DealerPlayer { get; set; }
        public List<Team?> DealWinners { get; set; }
        public int RaisedTo { get; set; }
        public int RaisePlayer { get; set; }
        public bool RaiseAccepting { get; set; }
        public int PointsThisRound { get; set; }
        public Team RaiseTeam { get; set; }
        public Team TeamFolded { get; set; }

    }
    
}
