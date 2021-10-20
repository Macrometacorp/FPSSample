using System.Collections.Generic;
using Macrometa;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;
using System.Linq;


    public class LobbyListUI : MonoBehaviour {
        //public List<LobbyListItemUI> items;
        public LobbyListItemUI prefabItem;
        public GameObject itemHolder;
        public Prelobby prelobby;

        public void DisplayLobbies(LobbyList lobbyList) {
            foreach (Transform child in itemHolder.transform) {
               Destroy(child.gameObject);
            }
            itemHolder.transform.DetachChildren();
            var sorted  = lobbyList.lobbies.OrderBy(x=> x.adminName).ToList();
            foreach (var lv in sorted) {
                if (lv.closeLobbyNow || lv.showGameInitNow || lv.joinGameNow) continue;
                var lli = Instantiate<LobbyListItemUI>(prefabItem,itemHolder.transform,false);
                lli.gameObject.SetActive(true);
                //lli.transform.SetParent(itemHolder.transform);
                lli.DisplayLobbyValue(lv);
            }
        }
    }
