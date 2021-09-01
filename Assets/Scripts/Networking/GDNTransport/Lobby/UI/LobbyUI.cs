using System;
using System.Collections;
using System.Collections.Generic;
using Macrometa;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviour {
   public TMP_Text title;
   public TeamUI team0;
   public TeamUI team1;
   public TeamUI unnassigned;
   public List<TeamUI> teams;
   public LobbyValue lobbyValue;
   public string ownerId;
   
   public void DisplayLobbyValue(LobbyValue aLobbyValue,string anOwnerId) {
      lobbyValue = aLobbyValue;
      team0.DisplayTeam(lobbyValue.team0,ownerId);
      team1.DisplayTeam(lobbyValue.team1,ownerId);
      unnassigned.DisplayTeam(lobbyValue.unassigned,ownerId);
      title.text = lobbyValue.DisplayName();
      ownerId = anOwnerId;
      //still need other lobby level code

   }
   public void TeamSelected(int teamIndex) {
      Debug.Log("TeamSelected(): " + teamIndex);
      GDNClientLobbyNetworkDriver2.MoveToTeam( teamIndex);
   }

   public void TeamNameChanged(string teamName, int teamIndex) {
      GDNClientLobbyNetworkDriver2.TeamNameChanged(teamName, teamIndex);
   }
   
   public void ServerAllowed(string clientID) {
      //send client ID to app
   }

   public void StartGame() {
      
   }
}
