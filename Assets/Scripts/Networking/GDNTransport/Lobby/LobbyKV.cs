using System;
using System.Collections.Generic;

namespace Macrometa.Lobby {
    public class LobbyKV {
        public string gameName;
        public string adminName;
        public Team unassigned;
        public Team team1;
        public Team team2;
        public bool frozen;
        public bool serverClientChosen;

    }

    [Serializable]
    public class Team {
        public string name;
        public List<TeamSlot> slots  = new List<TeamSlot>();
    }

    public class TeamSlot {
        // public bool empty; is this needed??
        public string playerName;
        public string region;
        public int ping;
        public int rtt;
        public bool runGameSErver;
    }
}