﻿using UnityEditor;

namespace Macrometa {
    public class PlayStats {

        public struct PlayerStats {
            public string name;     //player name
            public string killed;   //player name
            public string killedBy; //player name
            public string matchType; //DeathMatch, Assault
            public MatchResult matchResult; //lose , tie ,  win
            public string teamName;
            public string city;
            public string country;
            public int mainRobotShots;
            public int secondaryRobotShots;
            public int mainPioneerShots;
            public int secondaryPioneerShots;
            public string avatar; //Robot, Pioneer
            
            public override string ToString() {
                return "PlayerStats name: " + name + " killed: " + killed + " killedBy: " + killedBy +
                       " matchType: " + matchType + "result: "+ matchResult ;
            }
        }

        public enum MatchResult {
            Lose,
            Tie,
            Win
        }
        public class GameStats {
            public string gameType;
            public int numPlayers;

            public override string ToString() {
                return "GameStats gameType: " + gameType + " numPlayers: " + numPlayers;
            }
        }

        public static GameStats gameStats = new GameStats();

        public static void AddPlayerStat(string name, string killed, string killedBy, string gameType) {
            var ps = new PlayerStats() {
                name = name,
                killed = killed,
                killedBy = killedBy,
                matchType = gameType
            };
            SendPlayerStats(ps);
        }

        public static void AddPlayerMatchResultStat(string name, string gameType,
            MatchResult matchResult, int score) {
            var ps = new PlayerStats() {
                name = name,
                matchType = gameType,
                matchResult = matchResult,
                //score = score
            };
            SendPlayerStats(ps);
        }
        
        public static void SendPlayerStats(PlayerStats ps) {
            GameDebug.Log(ps.ToString());
        }

        public static void UpdateGameType(string gameType) {
            gameStats.gameType = gameType;
        }

        public static void UpdateNumPlayers(int numPlayers) {
            if (gameStats.numPlayers < numPlayers) {
                gameStats.numPlayers = numPlayers;
            }
        }


        public static void SendGameStats() {
            GameDebug.Log(gameStats.ToString());
        }
    }
}