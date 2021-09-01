using System;
using System.Collections;
using System.Collections.Generic;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    public string clientID; // used so buttons can send back an ID
    public LobbyUI lobby;
    public GameObject emptySlot;
    public GameObject activePlayer;
    public TMP_Text playerName;
    public TMP_Text pingTime;
    public TMP_Text rttTime;
    public Button serverAllowed; //shown to admin to select who can start game server
    public Button startGameServer; // shown to player to start game server
    public GameObject highlight;
    
    public void DisplayPlayer(TeamSlot teamSlot, bool isHighlight) {
        if (teamSlot == null) {
            emptySlot.SetActive(true);
            activePlayer.SetActive(false);
            highlight.SetActive(false);
            return;
        }

        highlight.SetActive(isHighlight);
        emptySlot.SetActive(false);
        activePlayer.SetActive(true);
        playerName.text = teamSlot.playerName;
        pingTime.text = teamSlot.ping.ToString();
        if (teamSlot.rtt > 0) {
            rttTime.text = teamSlot.rtt.ToString();
        }
        else {
            rttTime.text = "";
        }
        clientID = teamSlot.clientId;

    }
    
    public void ServerAllowedClicked() {
        lobby.ServerAllowed(clientID);
    }
    
    public void StartGameClicked() {
        lobby.StartGame();
    }
    
}
