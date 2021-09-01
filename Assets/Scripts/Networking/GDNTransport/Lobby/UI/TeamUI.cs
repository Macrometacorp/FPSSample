using System.Collections;
using System.Collections.Generic;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;

public class TeamUI : MonoBehaviour {
   public LobbyUI lobby;
   public int teamIndex;
   public List<PlayerUI> players;
   public TMP_InputField teamName;
   public GameObject highlight;
   
   private void Awake() {
      players.Clear();
      players.AddRange(GetComponentsInChildren<PlayerUI>());
   }

   // Macrometa.Lobby. is needed to stop FPSSample conflicts
   public void DisplayTeam(Macrometa.Lobby.Team team, string anOwnerId) {
      if (teamName != null) {
         teamName.text = team.name;
      }
      highlight.SetActive(false);
       var pos = team.Find(anOwnerId);
      for (int i = 0; i < players.Count; i++) {
         bool highlight = pos == i;
         if (i < team.slots.Count) {
            players[i].DisplayPlayer(team.slots[i],highlight);
         }
         else {
            players[i].DisplayPlayer(null,false);
         }
      }
   }
   
   public void TeamSelectedClicked() {
      Debug.Log("TeamSelectedClicked(): " + teamIndex);
      lobby.TeamSelected( teamIndex);
      highlight.SetActive(true);
   }
   
   public void TeamNameChanged() {
      Debug.Log("TeamNameChanged(): " + teamIndex + " : "+teamName.text );
      lobby.TeamNameChanged(teamName.text, teamIndex);
   }
}
