using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Macrometa;
using UnityEngine;
using UnityEngine.UI;

public class TestLobbyTransport : MonoBehaviour {
    public string localId = "GrantAdmin";
    public string gameMode = "DeathMatch";
    public string mapName = "Level 01";
    public int maxPlayers = 8; 
    [Space]
    public Text textDisplay;
    public InputField inputField;
    public ScrollRect scrollRect;
    
    public GDNClientLobbyNetworkDriver gdnClientLobbyNetworkDriver;
    public LobbyUI lobbyUi;
    
    /// <summary>
    ///  should wait for gdnStreamDriver.chatConsumerExists
    /// </summary>
    public void Start() {
        gdnClientLobbyNetworkDriver.localId = localId;
        inputField.Select();
        inputField.ActivateInputField(); 
    }

    public void Update() {
        while (gdnClientLobbyNetworkDriver.gdnStreamDriver.chatMessages.Count > 0) {
            AddText(gdnClientLobbyNetworkDriver.gdnStreamDriver.chatMessages.Dequeue());
        }

        if (gdnClientLobbyNetworkDriver.lobbyValue != null) {
            lobbyUi.DisplayLobbyValue(gdnClientLobbyNetworkDriver.lobbyValue,gdnClientLobbyNetworkDriver.clientId);
        }
    }
    
    #region Chat
    public void AddText(string aString) {
        textDisplay.text +=  aString +"\n";
        textDisplay.text = LimitLines(textDisplay.text);
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public string LimitLines(string aString, int lines = 50) {
        var stringArray =  aString.Split('\n');
        if (stringArray.Length < lines) {
            return aString;
        }
        else {
            var sb = new StringBuilder();
            for (int i = 1; i < stringArray.Length; i++) {
                sb.Append(stringArray[i] + "\n");
            }
            return sb.ToString();
        }
    }
    
    public void InputText() {
        Debug.Log( "Sent text: A " + inputField.text);
        if (inputField.text == "") return;
        Debug.Log( "Sent text: B " + inputField.text);
        SendText("<b>" + localId + "</b>: "+inputField.text);
        Debug.Log( "Sent text: B " + inputField.text);
        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();
    }

    public void SendText(string msg ) {
        gdnClientLobbyNetworkDriver.gdnStreamDriver.ChatSend(gdnClientLobbyNetworkDriver.gdnStreamDriver.chatChannelId ,msg);
    }

    public void ChangeChannelId(string newChannelId) {
        
        textDisplay.text = "";
        var oldMessages = GDNStreamDriver.ChatBuffer.Dump();
        int i = 1;
        foreach (var rm in oldMessages) {
            if (rm.properties.d == newChannelId) {
                AddText(Encoding.UTF8.GetString(Convert.FromBase64String(rm.payload)));
                Debug.Log(Encoding.UTF8.GetString(Convert.FromBase64String(rm.payload))+ " : "+ i++);
            }
        }
        gdnClientLobbyNetworkDriver.gdnStreamDriver.chatChannelId = newChannelId;
    }
    
    #endregion Chat
    
    #region Lobby
   
    #endregion Lobby
}
