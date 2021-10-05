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
