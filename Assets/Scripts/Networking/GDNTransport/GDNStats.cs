using System;
using System.Collections;
using System.Collections.Generic;
using Macrometa;
using Macrometa.Lobby;
using UnityEngine;

public class GDNStats {
    public static bool setupComplete = false;
    // must be set before websockets are opened.


    public static GameStats2 baseGameStats;
    public TestPlayStatsDriver testPlayStatsDriver;
    public static GDNStats instance=null;
    public static TeamInfo team0;
    public static TeamInfo team1;
    public static string gameName;
    public static string playerName;
    public InGameStats InGameStats;
    

    public static GDNStats Instance {
        get {
            if (instance==null) {
                GameDebug.Log("GDNStats creation" );
                instance = new GDNStats();
            }
            return instance;
        }
    }

    public static void Make() {
        if (instance==null) {
            GameDebug.Log("GDNStats creation Make" );
            instance = new GDNStats();
        }
        instance.Start();
    }
    
    public static void SetPlayerName(string playerName) {
        GDNStats.playerName = playerName;
        GameDebug.Log("SetPlayerName: " + playerName);
    }
    /*
    static public void SendKills(string killed, string killedBy) {
        GameDebug.Log("SendKills A");
        var gameStats2 = baseGameStats.CopyOf();
        //gameStats2.timeStamp = (long) (DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        gameStats2.killed = killed;
        gameStats2.killedBy = killedBy;
        TestPlayStatsDriver. SendStats(gameStats2);
    }
    */
    /// <summary>
    /// Start stat streams
    /// </summary>
    /// <param name="port"></param>
    /// <param name="maxConnections"></param>
    public void Start(bool isServer = true )
    {
        GameDebug.Log("GDNStats start" );
        testPlayStatsDriver= new GameObject().AddComponent<TestPlayStatsDriver>();
        MonoBehaviour.DontDestroyOnLoad(testPlayStatsDriver.gameObject);
        ResetBaseGame();
    }
    
    
    /// <summary>
    /// this only need when lobby system is not complete
    /// </summary>
    static public void AddPlayer(int teamIndex, string playerName) {
        if (teamIndex == 0) {
            team0.players.Add(playerName);
        }
        else {
            team1.players.Add(playerName);
        }
    }

    static public void ResetBaseGame() {
        team0 = new TeamInfo() {
            players =new List<string>(),
            teamName = "A Team"
        };
        team1 = new TeamInfo() {
            players = new List<string>(),
            teamName = "B Team"
        };
        GameDebug.Log("ResetBaseGame()" );
        baseGameStats = new GameStats2() {
            gameName = gameName,
            team0 = team0,
            team1 = team1
        };
    }

    static public void BaseGameFromLobby(LobbyValue lobbyValue) {
        var teamName = lobbyValue.team0.name;
        if (string.IsNullOrEmpty(teamName)) {
            teamName = "Alpha";
            if (teamName == lobbyValue.team1.name) {
                teamName = "Bravo";
            }
        } 
        team0 = new TeamInfo() {
            players =new List<string>(),
            teamName =  teamName
        };
        foreach (var slot in lobbyValue.team0.slots) {
            team0.players.Add(slot.playerName);
        }
        
        teamName = lobbyValue.team1.name;
        if (string.IsNullOrEmpty(teamName)) {
            teamName = "Bravo";
            if (teamName == lobbyValue.team0.name) {
                teamName = "Alpha";
            }
        } 
        team1 = new TeamInfo() {
            players =new List<string>(),
            teamName =  teamName
        };
        foreach (var slot in lobbyValue.team1.slots) {
            team1.players.Add(slot.playerName);
        }
        GameDebug.Log(" BaseGameFromLobby" );
        baseGameStats = new GameStats2() {
            gameName = gameName,
            team0 = team0,
            team1 = team1
        };
        GameDebug.Log(" BaseGameFromLobby GDNStats.baseGameStats" + ( GDNStats.baseGameStats != null));
        GameDebug.Log("BaseGameFromLobby team0: "+ (GDNStats.baseGameStats.team0 != null) );
        GameDebug.Log("BaseGameFromLobby team1: "+ (GDNStats.baseGameStats.team1 != null) );
        
    }
    
    
    public void MakeBaseInfo(string gameName, string team0Name, string team1Name, List<string> team0Players,
        List<String> team1Players) {
        var team0 = new TeamInfo() {
            players = team0Players,
            teamName = team0Name
        };
        var team1 = new TeamInfo() {
            players = team1Players,
            teamName = team1Name
        };

        baseGameStats = new GameStats2() {
            gameName = gameName,
            team0 = team0,
            team1 = team1
        };
    }

    static public void SendStats(GameStats2 gs) {
        gs.timeStamp = (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

        TestPlayStatsDriver.SendStats(gs);
    }
    
}
