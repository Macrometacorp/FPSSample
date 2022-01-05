using System;
using System.Collections;
using System.Collections.Generic;
using Macrometa;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviour {
   public MainMenu mainMenu;
   public TMP_Text title;
   public TeamUI team0;
   public TeamUI team1;
   public TeamUI unnassigned;
   public List<TeamUI> teams;
   public LobbyValue lobbyValue;
   public GameObject blockUIPanel;
   public GameObject gameInitMsg;
   public GameObject lobbyClosingMsg;
   public bool isAdmin;

   private bool _joinStarted = false;

   public string ownerId;
   protected int[] botCount = new int[2];
   
   
   public void DisplayLobbyValue(LobbyValue aLobbyValue,string anOwnerId) {
      lobbyValue = aLobbyValue;
      isAdmin = GDNClientLobbyNetworkDriver2.isLobbyAdmin;
      team0.DisplayTeam(lobbyValue.team0,ownerId,lobbyValue.rttTarget,lobbyValue.serverAllowed);
      team1.DisplayTeam(lobbyValue.team1,ownerId,lobbyValue.rttTarget,lobbyValue.serverAllowed);
      unnassigned.DisplayTeam(lobbyValue.unassigned,ownerId,lobbyValue.rttTarget,lobbyValue.serverAllowed);
      title.text = lobbyValue.DisplayName();
      ownerId = anOwnerId;
      if (lobbyValue.showGameInitNow) {
         GameInitMsg();
      }
      if (lobbyValue.closeLobbyNow) {
         LobbyClosing();
      }

      if (lobbyValue.joinGameNow && !_joinStarted) {
         _joinStarted = true;
            StartCoroutine(StartJoin());
      }
   }

   IEnumerator StartJoin() {
      var pos = lobbyValue.FindPlayerPos(ownerId);
      if (pos == -1) {
         //exit lobby immediately nice but
         //should exit when lobby closes
      }
      Debug.Log("StartJoin() ownerId: " + ownerId + " : "+ pos );
      yield return new WaitForSeconds(1+pos);
      Debug.Log("StartJoin() ownerId  B: " + ownerId + " : "+ pos );
      JoinGame();
   }

   /// <summary>
   /// Add a bot to a team
   /// </summary>
   /// <param name="teamIndex"></param>
   /// <returns>true if more bots cn be to added</returns>
   public bool AddBot(int teamIndex) {
      var aName = BotArray.data[teamIndex, botCount[teamIndex]].name;
      var teamSlot = GDNClientLobbyNetworkDriver2.BotTeamSlot(aName);
      GDNClientLobbyNetworkDriver2.MoveToTeam(teamSlot, teamIndex);
      botCount[teamIndex]++;
      return (botCount[teamIndex] != BotArray.data.GetLength(1) && 
              GDNClientLobbyNetworkDriver2.EmptySlot(teamIndex)  && !GlobalBotMaxed());
   }

   public void SetBotAddRemove() {
      team0.addBotsButton.interactable = BotAddOK(0);
      team1.addBotsButton.interactable = BotAddOK(1);

      team0.removeBotsButton.interactable = BotRemoveOK(0);
      team1.removeBotsButton.interactable = BotRemoveOK(1);
   }
   
   public bool BotAddOK(int teamIndex) {
      return (botCount[teamIndex] != BotArray.data.GetLength(1) && 
              GDNClientLobbyNetworkDriver2.EmptySlot(teamIndex)  && !GlobalBotMaxed());
   }
   
   public bool BotRemoveOK(int teamIndex) {
      return (botCount[teamIndex] > 0);
   }
   
   public bool GlobalBotMaxed() {
      return (botCount[0] + botCount[1]) >= BotArray.maxBots;
   }

   /// <summary>
   /// remove a bot from a team
   /// </summary>
   /// <param name="teamIndex"></param>
   /// <returns>true if more bots can be removed</returns>
   public bool RemoveBot(int teamIndex) {
      var aName = BotArray.data[teamIndex, botCount[teamIndex]-1].name;
      var teamSlot = GDNClientLobbyNetworkDriver2.BotTeamSlot(aName);
      GDNClientLobbyNetworkDriver2.RemoveBotFromTeam(teamSlot);
      botCount[teamIndex]--;
      return (botCount[teamIndex] != 0);
   }
   
   public void SetRttTarget(string clientId) {
      GDNClientLobbyNetworkDriver2.SetRttTarget(clientId);
   }
   
   public void TeamSelected(int teamIndex) {
      Debug.Log("TeamSelected(): " + teamIndex);
      GDNClientLobbyNetworkDriver2.MoveToTeam( teamIndex);
   }

   public void TeamNameChanged(string teamName, int teamIndex) {
      GDNClientLobbyNetworkDriver2.TeamNameChanged(teamName, teamIndex);
   }
   
   public void ServerAllowed(string clientId) {
      GameDebug.Log("pushed LobbyUI SetServerAllowed: "+ clientId);
      GDNClientLobbyNetworkDriver2.SetServerAllowed(clientId);
   }

   public void ResetBlockUI() {
      blockUIPanel.SetActive(false);
      lobbyClosingMsg.SetActive(false);
      gameInitMsg.SetActive(false);
   }
   
   public void LobbyClosing() {
      blockUIPanel.SetActive(true);
      lobbyClosingMsg.SetActive(true);
   }
   
   public void GameInitMsg() {
      blockUIPanel.SetActive(true);
      gameInitMsg.SetActive(true);
   }

   public void StartGame() {
      GameDebug.Log("LobbyUi start game");
      mainMenu.CreateGameFromLobby();
   }

   public void JoinGame() {
      mainMenu.JoinGame();
   }
}
