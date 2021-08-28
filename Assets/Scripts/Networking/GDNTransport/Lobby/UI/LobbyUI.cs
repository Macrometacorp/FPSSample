using System;
using System.Collections;
using System.Collections.Generic;
using Macrometa;
using Macrometa.Lobby;
using UnityEngine;

public class LobbyUI : MonoBehaviour {
   public TeamUI team0;
   public TeamUI team1;
   public TeamUI unnassigned;
   public List<TeamUI> teams;
   public LobbyValue lobbyValue;



   public void DisplayLobbyValue(LobbyValue aLobbyValue) {
      lobbyValue = aLobbyValue;
      team0.DisplayTeam(lobbyValue.team0);
      team1.DisplayTeam(lobbyValue.team1);
      unnassigned.DisplayTeam(lobbyValue.unassigned);
      //still need other lobby level code

   }
   public void TeamSelected(int teamIndex) {
      Debug.Log("TeamSelected(): " + teamIndex);
      GDNClientLobbyNetworkDriver.MoveToTeam( teamIndex);
   }

   public void ServerAllowed(string clientID) {
      //send client ID to app
   }

   public void StartGame() {
      
   }
}
