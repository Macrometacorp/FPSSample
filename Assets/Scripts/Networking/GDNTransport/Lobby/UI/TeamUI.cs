using System.Collections;
using System.Collections.Generic;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;


public class TeamUI : MonoBehaviour {
   public LobbyUI lobby;
   public int teamIndex;
   public List<PlayerUI> players;
   public TMP_Text teamName;
   
   private void Awake() {
      players.Clear();
      players.AddRange(GetComponentsInChildren<PlayerUI>());
   }


   public void DisplayTeam(Macrometa.Lobby.Team team) {
      if (teamName != null) {
         teamName.text = team.name;
      }

      for (int i = 0; i < players.Count; i++) {
         if (i < team.slots.Count) {
            players[i].DisplayPlayer(team.slots[i]);
         }
         else {
            players[i].DisplayPlayer(null);
         }
      }
   }
   
   public void TeamSelectedClicked() {
      Debug.Log("TeamSelectedClicked(): " + teamIndex);
      lobby.TeamSelected( teamIndex);
   }
}
