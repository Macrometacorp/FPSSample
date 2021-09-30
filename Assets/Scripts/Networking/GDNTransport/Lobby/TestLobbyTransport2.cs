using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BestHTTP.SignalRCore.Messages;
using Macrometa;
using Macrometa.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestLobbyTransport2 : MonoBehaviour {
    public string localId = "";
    public string gameMode = "DeathMatch";
    public string mapName = "Level 01";
    public int maxPlayers = 8;
    [Space] 
    public TMP_Text textDisplay;
    public TMP_InputField inputField;
    public ScrollRect scrollRect;
    public Prelobby prelobby;
    public bool isLobbyDisplayed = false;
 

    public GDNClientLobbyNetworkDriver2 gdnClientLobbyNetworkDriver2;
    public LobbyUI lobbyUi;
    public CreateLobbyUI createLobbyUi;

    /// <summary>
    ///  should wait for gdnStreamDriver.chatConsumerExists
    /// </summary>
    public void Start() {
        localId = GDNStreamDriver.localId;
        inputField.Select();
        inputField.ActivateInputField();
        prelobby.gdnClientLobbyNetworkDriver2 = gdnClientLobbyNetworkDriver2;
        lobbyUi.gameObject.SetActive(false);
        prelobby.gameObject.SetActive(true);
    }

    public void Update() {
        while (gdnClientLobbyNetworkDriver2.gdnStreamDriver.chatMessages.Count > 0) {
            AddText(gdnClientLobbyNetworkDriver2.gdnStreamDriver.chatMessages.Dequeue());
        }

        if (gdnClientLobbyNetworkDriver2.lobbyValue != null && gdnClientLobbyNetworkDriver2.lobbyValue.baseName != "" && 
            gdnClientLobbyNetworkDriver2.lobbyValue.baseName != null &&
            gdnClientLobbyNetworkDriver2.lobbyUpdateAvail && !isLobbyDisplayed) {
            lobbyUi.gameObject.SetActive(true);
            GameDebug.Log("Show lobby A");
            lobbyUi.ResetBlockUI();
            prelobby.gameObject.SetActive(false);
            isLobbyDisplayed = true;
            GameDebug.Log("Show lobby Z");
              
            prelobby.Wait(false);
        }
        
        if ((gdnClientLobbyNetworkDriver2.lobbyValue == null || gdnClientLobbyNetworkDriver2.lobbyValue.baseName == "" ||
             gdnClientLobbyNetworkDriver2.lobbyValue.baseName == null )&&
             isLobbyDisplayed) {
            lobbyUi.gameObject.SetActive(false);
            prelobby.gameObject.SetActive(true);
            isLobbyDisplayed = false;
            GameDebug.Log("Hide lobby ");
        }

        //display lobby closing now message when after lobby admin stops updating lobby
        if (gdnClientLobbyNetworkDriver2.lobbyValue != null && gdnClientLobbyNetworkDriver2.lobbyValue.closeLobbyNow) {
            lobbyUi.DisplayLobbyValue(gdnClientLobbyNetworkDriver2.lobbyValue, gdnClientLobbyNetworkDriver2.clientId);
        }
        
        if (gdnClientLobbyNetworkDriver2.lobbyValue != null && gdnClientLobbyNetworkDriver2.lobbyUpdateAvail &&
            isLobbyDisplayed) {
            
            if (gdnClientLobbyNetworkDriver2.lobbyValue.joinGameNow) {
                //join game now
            }
            //GameDebug.Log("UpdateLocalLobby in test lobby transport 2");
            lobbyUi.DisplayLobbyValue(gdnClientLobbyNetworkDriver2.lobbyValue, gdnClientLobbyNetworkDriver2.clientId);
            gdnClientLobbyNetworkDriver2.lobbyUpdateAvail = false;
        }

        if (gdnClientLobbyNetworkDriver2.lobbyList != null && gdnClientLobbyNetworkDriver2.lobbyList.isDirty &&
            !isLobbyDisplayed) {
            //GameDebug.Log("Update Lobby List in transport 2");
            gdnClientLobbyNetworkDriver2.lobbyList.isDirty = false;
            //show loblist
            prelobby.lobbyListUi.DisplayLobbies(gdnClientLobbyNetworkDriver2.lobbyList);
            
            
            //lobbyUi.DisplayLobbyValue(gdnClientLobbyNetworkDriver2.lobbyValue, gdnClientLobbyNetworkDriver2.clientId);
        }
        
    }

    #region Chat

    public void AddText(string aString) {
        textDisplay.text += aString + "\n";
        textDisplay.text = LimitLines(textDisplay.text);
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public string LimitLines(string aString, int lines = 50) {
        var stringArray = aString.Split('\n');
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
        Debug.Log("Sent text: A " + inputField.text);
        if (inputField.text == "") return;
        Debug.Log("Sent text: B " + inputField.text);
        SendText("<b>" + localId + "</b>: " + inputField.text);
        Debug.Log("Sent text: B " + inputField.text);
        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();
    }

    public void SendText(string msg) {
        gdnClientLobbyNetworkDriver2.gdnStreamDriver.ChatSend(
            gdnClientLobbyNetworkDriver2.gdnStreamDriver.chatChannelId, msg);
    }

    public void ChangeChannelId(string newChannelId) {

        textDisplay.text = "";
        var oldMessages = GDNStreamDriver.ChatBuffer.Dump();
        int i = 1;
        foreach (var rm in oldMessages) {
            if (rm.properties.d == newChannelId) {
                AddText(Encoding.UTF8.GetString(Convert.FromBase64String(rm.payload)));
                Debug.Log(Encoding.UTF8.GetString(Convert.FromBase64String(rm.payload)) + " : " + i++);
            }
        }

        gdnClientLobbyNetworkDriver2.gdnStreamDriver.chatChannelId = newChannelId;
    }

    #endregion Chat

    #region Lobby

    public void GameNameChanged(string gameName) {
        var defaultConfig = RwConfig.ReadConfig();
        defaultConfig.gameName = gameName;
        RwConfig.Change(defaultConfig);
        RwConfig.Flush();
    }

   
    
    public void CreateLobby() {
        if (createLobbyUi.baseName.text == "") {
            createLobbyUi.NameError(true);
            return;
        }
        createLobbyUi.NameError(false);
        var cd = RwConfig.ReadConfig();
        cd.gameName = createLobbyUi.baseName.text;
        RwConfig.Change(cd);
        RwConfig.Flush();
        gdnClientLobbyNetworkDriver2.gameMode = createLobbyUi.gameMode.itemText.text;
        gdnClientLobbyNetworkDriver2.maxPlayers = 8;
        gdnClientLobbyNetworkDriver2.startDocumentInit = true;
        GameDebug.Log("CreateLobby()");
        prelobby.Wait(true);
    }
    #endregion Lobby
}
