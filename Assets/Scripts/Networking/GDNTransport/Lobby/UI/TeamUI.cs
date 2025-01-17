﻿using System.Collections;
using System.Collections.Generic;
using Macrometa;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TeamUI : MonoBehaviour {
   const string cls ="TeamUI";
   
   public LobbyUI lobby;
   public int teamIndex;
   public List<PlayerUI> players;
   public TMP_InputField teamName;
   public GameObject highlight;
   public GameObject botsControl;
   public Button addBotsButton;
   public Button removeBotsButton;

   private void Awake() {
      players.Clear();
      players.AddRange(GetComponentsInChildren<PlayerUI>()); 
      botsControl.SetActive(false);
      
      addBotsButton.onClick.AddListener(AddBot);
      removeBotsButton.onClick.AddListener(RemoveBot);
      addBotsButton.interactable = true;
         //gameObject.SetActive(true);
      removeBotsButton.interactable = false;
         // .gameObject.SetActive(false);
      
    }

   public void AddBot() {
      var moreBots = lobby.AddBot(teamIndex);
      lobby.SetBotAddRemove();
   }

   public void RemoveBot() {
      var moreBots =lobby.RemoveBot(teamIndex);
      lobby.SetBotAddRemove();
   }
   
   // Macrometa.Lobby. is needed to stop FPSSample conflicts
   public void DisplayTeam(Macrometa.Lobby.Team team, string anOwnerId, string rttTarget, string startServer) {
      if (teamName != null) {
         if (!lobby.isAdmin || !teamName.isFocused) {
            teamName.text = team.name;
         }
         teamName.placeholder.gameObject.SetActive(lobby.isAdmin && teamName.text == "");
         teamName.interactable = lobby.isAdmin;
      }
      var serverButtons = lobby.isAdmin;
      
      if (botsControl != null) {
        // GameDebugPlus.Log(MMLog.Mm, cls, "DisplayTeam", "addBots lobby.isAdmin: " + lobby.isAdmin);
         botsControl.SetActive(serverButtons);
      }
      
      highlight.SetActive(false);
      var pos = team.Find(anOwnerId);
      var rttPos = team.Find(rttTarget);
      var startServerPos = team.Find(startServer);
      //Debug.Log("startServer: "+startServer + " : "+startServerPos);
      for (int i = 0; i < players.Count; i++) {
         bool highlight = pos == i;
         if (i < team.slots.Count) {
            if (!team.slots[i].isBot) {
               players[i].rttTargetButton.gameObject.SetActive(serverButtons);
               players[i].serverAllowed.gameObject.SetActive(serverButtons);
               team.slots[i].rttTarget = (rttPos == i);
               team.slots[i].runGameServer = (startServerPos == i && startServerPos == pos);
            }
            else {
               players[i].rttTargetButton.gameObject.SetActive(false);
               players[i].serverAllowed.gameObject.SetActive(false);
               team.slots[i].rttTarget = false;
               team.slots[i].runGameServer = false; 
            }
            players[i].DisplayPlayer(team.slots[i],highlight);
         }
         else {
            players[i].DisplayPlayer(null,false);
         }
      }
   }
   
   public void TeamSelectedClicked() {
      //Debug.Log("TeamSelectedClicked(): " + teamIndex);
      lobby.TeamSelected( teamIndex);
      highlight.SetActive(true);
   }
   
   public void TeamNameChanged() {
      //Debug.Log("TeamNameChanged(): " + teamIndex + " : "+teamName.text );
      lobby.TeamNameChanged(teamName.text, teamIndex);
   }
}
