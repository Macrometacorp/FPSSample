using System;
using System.Collections.Generic;
using BestHTTP.JSON;
using Macrometa.Lobby;
using UnityEditor;
using UnityEngine;

namespace Macrometa {
    public class PlayStats {
        private static LobbyValue lobbyValue;


        public enum MatchResult {
            Lose,
            Tie,
            Win
        }

        static public GameStats BaseGameStats() {
            var result = new GameStats() {
                gameName = lobbyValue.GameName(),
                team0 = lobbyValue.team0.ToTeamInfo(),
                team1 = lobbyValue.team1.ToTeamInfo()
            };
            return result;
        }

        
        public static void UpdateNumPlayers( int x){}
        public static void AddPlayerStat(string a, string b, string c, string d) {
        }

        public static void AddPlayerStat(string killed, string killedBy) {
            var ps = BaseGameStats();
            ps.killed = killed;
            ps.killedBy = killedBy;
            SendPlayerStats(ps);
        }

        public static void AddPlayerMatchResultStat(string playerName, string gameType,
            MatchResult matchResult, int score) {
            var ps = BaseGameStats();

            ps.playerName = playerName;
            ps.matchType = gameType;
            ps.matchResult = matchResult.ToString();
            //score = score

            SendPlayerStats(ps);
        }

        public static void SendPlayerStats(GameStats ps) {
            GameDebug.Log(ps.ToString());
        }

        public static void UpdateGameType(string gameType) {
            // gameStats.gameType = gameType;
        }

        /// <summary>
        /// do this from lobby info
        /// </summary>
        /// <param name="numPlayers"></param>
        public static void UpdateTeams(int numPlayers) {
        }

        public static void SendGameStats() {
            // GameDebug.Log(gameStats.ToString());
        }
    }
}


[Serializable]
public class GameStats {
    public string gameName;
    public string playerName;
    public string matchType; //DeathMatch, Assault
    public string matchResult; //Lose , Tie ,  Win

    public string teamName;

    //public List<global::TeamInfo> teams;
    public TeamInfo team0;
    public TeamInfo team1;
    public int rtt;
    public string city;
    public string country;
    public int mainRobotShots;
    public int secondaryRobotShots;
    public int mainPioneerShots;
    public int secondaryPioneerShots;
    public string avatar; //Robot, Pioneer
    public string killed;
    public string killedBy;

    public GameStats CopyOf() {
        var result = new GameStats() {
            gameName = gameName,
            playerName = playerName,
            matchType = matchType,
            matchResult = matchResult,
            teamName = teamName,
            team0 = team0.CopyOf(),
            team1 = team1.CopyOf(),
            rtt = rtt,
            city = city,
            country = country,
            mainPioneerShots = mainPioneerShots,
            mainRobotShots = mainRobotShots,
            secondaryPioneerShots = secondaryPioneerShots,
            secondaryRobotShots = secondaryRobotShots,
            avatar = avatar,
            killed = killed,
            killedBy = killedBy
        };
        //result.teams = new List<TeamInfo>();
        return result;
    }
}

[Serializable]
public class TeamInfo {
    public string teamName;
    public List<string> players;

    public TeamInfo CopyOf() {
        var result = new TeamInfo() {
            teamName = teamName,
        };
        result.players = new List<string>();
        result.players.AddRange(players);
        return result;
    }
}


/*
  {
  "gameName": "t2bGame3",
  "playerName": "P2",
  "matchType": "DeathMatch",
  "killRecords": "[{\"count\":1,\"killedBy\":\"P5\",\"killed\":\"P1\"}]",
  "totalKillRecords": "[{\"count\":1,\"killedBy\":\"P5\"}]"

 */
[Serializable]
public class InGameStats {
    public string gameName;
    public string killRecords;
    public string totalKillRecords;

    public KillRecordList killRecordsList;
    public TotalKillRecordList totalKillRecordList;
    
    static  public string test = $"[{{\"count\":1,\"killedBy\":\"P5\",\"killed\":\"P1\"}}]";
    public string test2 = "{\"krl\":" + test + "}";
    public void Convert() {
        killRecordsList = JsonUtility.FromJson<KillRecordList>( "{\"krl\":" + killRecords + "}");
        totalKillRecordList = JsonUtility.FromJson<TotalKillRecordList>( "{\"krl\":" + totalKillRecords + "}");
    }
}

[Serializable]
public class KillRecordList {
    public List<global::KillRecord> krl;
}

[Serializable]
public class TotalKillRecordList {
    public List<global::TotalKillRecord> krl;
}


[Serializable]
public class KillRecord {
    public string killed;
    public string killedBy;
    public int count;
}

[Serializable]
public class TotalKillRecord {
    public string killedBy;
    public int count;
}


[Serializable]
public class KillStats {
    public List<string> opponents;
    public List<string> comrades;
    public List<int> killOpponents;
    public List<int> killComrades;
    public List<int> killedByOpponents;
    public List<int> killedByComrades;
    public List<int> totalKillsOpponents;
    public List<int> totalKillsOComradess;
    
    public KillStats (string playerName, LobbyValue lobbyValue,InGameStats inGameStats ) {
        SetHeader(playerName, lobbyValue);

    }

    public void SetHeader(string playerName, LobbyValue lobbyValue) {
        List<string> t0 = new List<string>();
        foreach (var slot in lobbyValue.team0.slots) {
            t0.Add(slot.playerName);
        }
        List<string> t1 = new List<string>();
        foreach (var slot in lobbyValue.team1.slots) {
            t1.Add(slot.playerName);
        }

        if (t0.Contains(playerName)) {
            opponents = t1;
            comrades = t0;
        }
        else {
            opponents = t0;
            comrades = t1;
        }
        comrades.Remove(playerName);
        comrades.Add(playerName);
    }
    
    
}

/*
"gameName": "GameX5",
      "killRecords": [
       {
      ],
      "totalKillRecords": [
        
      ],
      "totalShotsFired":[
          {
              "playerName":"Anurag",
              "totalShotsFired":{// career level

                "mainRobotShots": 0,
                "secondaryRobotShots": 0,
                "mainPioneerShots": 0,
                "secondaryPioneerShots": 0
              },
              "city": "Pune",
              "country": "In"
          },
          {
            "playerName":"Grant",
            "totalShotsFired":{

              "mainRobotShots": 0,
              "secondaryRobotShots": 0,
              "mainPioneerShots": 0,
              "secondaryPioneerShots": 0
            },
            "city": "Tokyo",
            "country": "Jp"
        }
      ]
  }
*/