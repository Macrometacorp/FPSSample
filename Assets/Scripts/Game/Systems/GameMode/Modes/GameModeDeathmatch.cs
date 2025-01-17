﻿using System.Collections;
using System.Collections.Generic;
using Macrometa;
using UnityEngine;
using Unity.Entities;

// Simple, team based deathmatch mode

public class GameModeDeathmatch : IGameMode {
    public const string cls = "GameModeDeathmatch";
    [ConfigVar(Name = "game.dm.minplayers", DefaultValue = "2", Description = "Minimum players before match starts")]
    public static ConfigVar minPlayers;
    [ConfigVar(Name = "game.dm.prematchtime", DefaultValue = "60", Description = "Time before match starts")]
    public static ConfigVar preMatchTime;
    [ConfigVar(Name = "game.dm.postmatchtime", DefaultValue = "10", Description = "Time after match ends before new will begin")]
    public static ConfigVar postMatchTime;
    [ConfigVar(Name = "game.dm.roundlength", DefaultValue = "420", Description = "Deathmatch round length (seconds)")]
    public static ConfigVar roundLength;

    public void Initialize(GameWorld world, GameModeSystemServer gameModeSystemServer)
    {
        m_world = world;
        m_GameModeSystemServer = gameModeSystemServer;

        // Create teams
        m_GameModeSystemServer.CreateTeam("Team 1");
        m_GameModeSystemServer.CreateTeam("Team 2");

        Console.Write("Deathmatch game mode initialized");
    }

    public void Restart()
    {
        foreach (var t in m_GameModeSystemServer.teams)
            t.score = 0;
        m_Phase = Phase.Countdown;
        m_GameModeSystemServer.StartGameTimer(preMatchTime, "PreMatch");
    }

    public void Shutdown()
    {
    }

    char[] _msgBuf = new char[256];
    public void Update()
    {
        //GameDebug.Log(" GameModeDeathmatch.update A");
        var gameModeState = m_GameModeSystemServer.gameModeState;
        //GameDebug.Log(" GameModeDeathmatch.update B");
        var players = m_GameModeSystemServer.playersComponentGroup.GetComponentArray<PlayerState>();
        //GameDebug.Log(" GameModeDeathmatch.update C");
       
        /*
        GameDebug.Log(" GameModeDeathmatch.update D 1");
        GameDebug.Log(" GameModeDeathmatch.update D A:" +
                      GDNLobbyNetworkDriver2.inst);
        GameDebug.Log(" GameModeDeathmatch.update D B:" +
                      GDNLobbyNetworkDriver2.inst.lobbyValue);
        GameDebug.Log(" GameModeDeathmatch.update D C:" +
                      GDNLobbyNetworkDriver2.inst.lobbyValue.team0);
        GameDebug.Log(" GameModeDeathmatch.update D E:" +
                      GDNLobbyNetworkDriver2.inst.lobbyValue.team0.name);
       
        GameDebug.Log(" GameModeDeathmatch.update E 1");
        GameDebug.Log(" GameModeDeathmatch.update E 1:" +
                      GDNLobbyNetworkDriver2.inst.lobbyValue.team1.name
                      );
                      */
        m_GameModeSystemServer.NameTeam( GDNLobbyNetworkDriver2.inst.lobbyValue.team0.name,0);
        m_GameModeSystemServer.NameTeam( GDNLobbyNetworkDriver2.inst.lobbyValue.team1.name,1);
        
        //GameDebug.Log(" GameModeDeathmatch.update F");
        switch (m_Phase)
        {
            case Phase.Countdown:
                //GameDebug.Log(" GameModeDeathmatch.update Countdown");
                if (m_GameModeSystemServer.GetGameTimer() == 0)
                {
                    if (players.Length < minPlayers.IntValue)
                    {
                        m_GameModeSystemServer.chatSystem.SendChatAnnouncement("Waiting for more players.");
                        m_GameModeSystemServer.StartGameTimer(preMatchTime, "PreMatch");
                    }
                    else
                    {
                        m_GameModeSystemServer.StartGameTimer(roundLength, "");
                        m_Phase = Phase.Active;
                        m_GameModeSystemServer.chatSystem.SendChatAnnouncement("Match started!");
                        m_GameModeSystemServer.AssignTeams();
                        // kill all players to get them to respawn
                        for (int i = 0, c = players.Length; i < c; i++) {
                            var playerState = players[i];
                            if (playerState.controlledEntity != Entity.Null) {
                                var healthState = m_world.GetEntityManager()
                                    .GetComponentData<HealthStateData>(playerState.controlledEntity);
                                healthState.health = 0.0f;
                                healthState.deathTick = -1;
                                m_world.GetEntityManager()
                                    .SetComponentData(playerState.controlledEntity, healthState);
                            }
                        }
                    }
                }
                break;
            case Phase.Active:
                //GameDebug.Log(" GameModeDeathmatch.update Active");
                if (m_GameModeSystemServer.GetGameTimer() == 0)
                {
                    // Find winner team
                    var winTeam = -1;
                    var teams = m_GameModeSystemServer.teams;
                    // TODO (petera) Get rid of teams list and hardcode for teamsize 2 as all ui etc assumes it anyways.
                    if (teams.Count == 2)
                    {
                        winTeam = teams[0].score > teams[1].score ? 0 : teams[0].score < teams[1].score ? 1 : -1;
                    }

                    // TODO : For now we just kill all players when we restart 
                    // but we should change it to something less dramatic like taking
                    // control away from the player or something
                    for (int i = 0, c = players.Length; i < c; i++)
                    {
                        var playerState = players[i];
                        if (playerState.controlledEntity != Entity.Null)
                        {
                            var healthState = m_world.GetEntityManager()
                                .GetComponentData<HealthStateData>(playerState.controlledEntity);
                            healthState.health = 0.0f;
                            healthState.deathTick = -1;
                            m_world.GetEntityManager()
                                .SetComponentData(playerState.controlledEntity, healthState);
                        }
                        playerState.displayGameResult = true;
                        if (winTeam == -1) {
                            playerState.gameResult = "TIE";
                            PlayStats.AddPlayerMatchResultStat(playerState.playerName, "deathmatch",
                                PlayStats.MatchResult.Tie, playerState.score);
                        }
                        else {
                            playerState.gameResult = (playerState.teamIndex == winTeam) ? "VICTORY" : "DEFEAT";
                            var matchResult = (playerState.teamIndex == winTeam) ?
                                PlayStats.MatchResult.Win : PlayStats.MatchResult.Lose;
                            PlayStats.AddPlayerMatchResultStat(playerState.playerName, "deathmatch",
                                matchResult, playerState.score);
                        }

                        playerState.displayScoreBoard = false;
                        playerState.displayGoal = false;
                    }
                    // PlayStats.UpdateNumPlayers( players.Length);
                    m_Phase = Phase.Ended;
                    m_GameModeSystemServer.SetRespawnEnabled(false);
                    m_GameModeSystemServer.StartGameTimer(postMatchTime, "PostMatch");
                    var l = 0;
                    if (winTeam > -1) {
                        l = StringFormatter.Write(ref _msgBuf, 0, "Match over. {0} wins!",
                            m_GameModeSystemServer.teams[winTeam].name);
                       
                    }
                    else{
                        l = StringFormatter.Write(ref _msgBuf, 0, "Match over. Its a tie!");
                        
                    }
                    m_GameModeSystemServer.chatSystem.SendChatAnnouncement(new CharBufView(_msgBuf, l));
                    
                }
                break;
            case Phase.Ended:
                if (m_GameModeSystemServer.GetGameTimer() == 0)
                {
                    for (int i = 0, c = players.Length; i < c; i++)
                    {
                        var playerState = players[i];
                        playerState.displayGameResult = false;
                    }
                    m_GameModeSystemServer.Restart();
                }
                break;
        }
        //GameDebug.Log(" GameModeDeathmatch.update afterPhase");
        for(int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (player.controlledEntity == Entity.Null)
                continue;
            var charPredictedState = m_world.GetEntityManager().GetComponentData<CharacterPredictedData>(player.controlledEntity);
            var position = charPredictedState.position;
            player.enableCharacterSwitch = false;
            foreach(var b in m_GameModeSystemServer.teamBases)
            {
                if (b.teamIndex == player.teamIndex)
                {
                    var inside = (b.boxCollider.transform.InverseTransformPoint(position) - b.boxCollider.center);
                    if (Mathf.Abs(inside.x) < b.boxCollider.size.x * 0.5f && Mathf.Abs(inside.y) < b.boxCollider.size.y * 0.5f && Mathf.Abs(inside.z) < b.boxCollider.size.z * 0.5f)
                    {
                        player.enableCharacterSwitch = true;
                        break;
                    }
                }
            }
        }

        // Push scores to gameMode that is synchronized to client
        gameModeState.teamScore0 = m_GameModeSystemServer.teams[0].score;
        gameModeState.teamScore1 = m_GameModeSystemServer.teams[1].score;
    }

    public void OnPlayerJoin(PlayerState player)
    {
        player.score = 0;
        m_GameModeSystemServer.AssignTeam(player);
    }

    public void OnPlayerKilled(PlayerState victim, PlayerState killer)
    {
        if (killer != null)
        {
            if (killer.teamIndex != victim.teamIndex)
            {
                killer.score++;
                m_GameModeSystemServer.teams[killer.teamIndex].score++;
            }
        }
    }

    public void OnPlayerRespawn(PlayerState player, ref Vector3 position, ref Quaternion rotation) {
        var teamIndex = player.teamIndex;
        if (BotArray.IsBot(player.playerName)) {
           teamIndex = -1;
           //GameDebugPlus.Log(MMLog.Mm, cls, " OnPlayerRespawn", 
           //     "changed team index team: "+teamIndex );
        }
        GameDebugPlus.Log(MMLog.Mm, cls, " OnPlayerRespawn", 
            "A OnPlayerRespawn: "+   player.playerName + " team: "+teamIndex );

       ;
        m_GameModeSystemServer.GetRandomSpawnTransform(teamIndex, ref position, ref rotation);
        GameDebugPlus.Log(MMLog.Mm, cls, " OnPlayerRespawn", 
            "B OnPlayerRespawn: "+   player.playerName + " team: "+teamIndex + " :: "+ position);
       
    }

    enum Phase
    {
        Undefined,
        Countdown,
        Active,
        Ended,
    }
    Phase m_Phase;

    GameWorld m_world;
    GameModeSystemServer m_GameModeSystemServer;
}
