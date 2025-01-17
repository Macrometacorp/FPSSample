﻿using System;
using System.Collections;
using System.Collections.Generic;
using Macrometa;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour 
{
    
    const string cls ="MainMenu";
    [System.Serializable]
    public struct UIBinding
    {
        public GameObject[] menus;

        public TMPro.TextMeshProUGUI buildId;

        // Create menu
        public TMPro.TMP_InputField servername;
        public TMPro.TMP_Dropdown gamemode;
        public TMPro.TMP_Dropdown levelname;
        public TMPro.TMP_Dropdown maxplayers;
        public Toggle decidatedServer;
    }

    public UIBinding uiBinding;

    [FormerlySerializedAs("mainMenu")] public GameObject mainMenuGO;
    public GameObject createGameMenu;
    public GameObject createFailMsg;
    public GameObject introMenu;
    public GameObject button_JoinGame;
    public JoinMenu joinMenu;
    public OptionsMenu optionMenu;
    public GameObject disconnectedMenu;
    public bool isDisconnected = false;
    //public GDNClientBrowserNetworkDriver gdnClientBrowserNetworkDriver;
    public GDNClientLobbyNetworkDriver2 gdnClientBrowserNetworkDriver;
    public TestLobbyTransport2 testLobbyTransport2;
    // Currently active submenu, used by menu backdrop to track what is going on
    public int activeSubmenuNumber;

    CanvasGroup m_CanvasGroup;

    private bool createGame = false;
    private List<string> botsPLaying = new List<string>();

    public void NameChanged(string playerName) {
        testLobbyTransport2.localId = playerName;
    }

    public void SetPanelActive(ClientFrontend.MenuShowing menuShowing)
    {
        var active = menuShowing != ClientFrontend.MenuShowing.None;
        gameObject.SetActive(active);
        if(active)
        {
            foreach(var a in GetComponentsInChildren<MenuButton>(true))
            {
                var enabled = (a.ingameOption && menuShowing == ClientFrontend.MenuShowing.Ingame) || (a.mainmenuOption && menuShowing == ClientFrontend.MenuShowing.Main);
                a.gameObject.SetActive(enabled);
            }
            // Close any open menu
            ShowSubMenu(null);
        }
    }

    public bool GetPanelActive()
    {
        return gameObject.activeSelf;
    }

    public void Awake()
    {
        createGameMenu.SetActive(false);
        joinMenu.UpdateGdnFields();
        optionMenu.UpdateGdnFields();
        optionMenu.UpdateConnectionFields();
        m_CanvasGroup = GetComponent<CanvasGroup>();

        uiBinding.gamemode.options.Clear();
        uiBinding.gamemode.options.Add(new TMPro.TMP_Dropdown.OptionData("Deathmatch"));
        uiBinding.gamemode.options.Add(new TMPro.TMP_Dropdown.OptionData("Assault"));
        uiBinding.gamemode.RefreshShownValue();

        uiBinding.levelname.options.Clear();
        uiBinding.levelname.options.Add(new TMPro.TMP_Dropdown.OptionData("Level_01"));
        uiBinding.levelname.options.Add(new TMPro.TMP_Dropdown.OptionData("Level_00"));
        uiBinding.levelname.RefreshShownValue();

        uiBinding.maxplayers.options.Clear();
        uiBinding.maxplayers.options.Add(new TMPro.TMP_Dropdown.OptionData("2"));
        uiBinding.maxplayers.options.Add(new TMPro.TMP_Dropdown.OptionData("4"));
        uiBinding.maxplayers.options.Add(new TMPro.TMP_Dropdown.OptionData("8"));
        //uiBinding.maxplayers.options.Add(new TMPro.TMP_Dropdown.OptionData("16"));
        uiBinding.maxplayers.RefreshShownValue();

        uiBinding.buildId.text = Game.game.buildId;
    }
    
    void Update() {
        if (createGame) {
            CreateBotsFromLobby();
            CreateServer();
            createGame = false;
        }
    }
    
    public void CreateGameFromLobby() {
        createGame = true;
       // CreateBotsFromLobby();
      //  CreateServer();
    }
    void OnEnable() {
        if (isDisconnected) {
            mainMenuGO.SetActive(false);
            disconnectedMenu.SetActive(true);
        } 
    }
    
    public void UpdateMenus()
    {
        if(joinMenu.gameObject.activeInHierarchy)
            joinMenu.UpdateMenu();

        if(optionMenu.gameObject.activeInHierarchy)
            optionMenu.UpdateMenu();
    }

    internal void SetAlpha(float v)
    {
        m_CanvasGroup.alpha = v;
    }

    // Called from the Menu/Button_* UI.Buttons
    public void ShowSubMenu(GameObject ShowMenu)
    {
        if (ShowMenu == uiBinding.menus[activeSubmenuNumber]) {
            ShowMenu = introMenu;
        }
        activeSubmenuNumber = 0;
        for(int i = 0; i < uiBinding.menus.Length; i++)
        {
            var menu = uiBinding.menus[i];
            if (menu == ShowMenu)
            {
                menu.SetActive(true);
                activeSubmenuNumber = i;
               // GameDebug.Log("Menu name: " +  menu.name);
                if (menu == createGameMenu) {
                   // GameDebug.Log("Menu name: " +  menu.name + " setting false");
                    createFailMsg.SetActive(false);
                }
            }
            else if (menu.activeSelf)
                menu.SetActive(false);
        }
    }

    public void OnJoinGame() {
        Console.EnqueueCommand("connect localhost");
    }

    public void OnQuitGame()
    {
        Console.EnqueueCommand("quit");
    }

    public void OnLeaveGame()
    {
        Console.EnqueueCommand("disconnect");
    }

    public void OnCreateGame() {
        CreateGame();
        //gdnClientBrowserNetworkDriver.tryKVInit = true;
    }
    
    public void CreateGame(){   
        var servername = uiBinding.servername.text;

        var levelname = uiBinding.levelname.options[uiBinding.levelname.value].text;

        // TODO : Add commands to set these
        var gamemode = uiBinding.gamemode.options[uiBinding.gamemode.value].text.ToLower();
        var maxplayers = uiBinding.maxplayers.options[uiBinding.maxplayers.value].text;

        var dedicated = uiBinding.decidatedServer.isOn;
        if(dedicated)
        {
            var process = new System.Diagnostics.Process();
            if (Application.isEditor)
            {
                process.StartInfo.FileName = k_AutoBuildPath + "/" + k_AutoBuildExe;
                process.StartInfo.WorkingDirectory = k_AutoBuildPath;
            }
            else
            {
                // TODO : We should look to make this more robust but for now we just
                // kill other processes to avoid running multiple servers locally
                var thisProcess = System.Diagnostics.Process.GetCurrentProcess();
                /*
                var processes = System.Diagnostics.Process.GetProcesses();
                foreach (var p in processes)
                {
                    if (p.Id != thisProcess.Id && p.ProcessName == thisProcess.ProcessName)
                    {
                        try
                        {
                            p.Kill();
                        }
                        catch (System.Exception)
                        {
                        }
                    }
                }
                */
                process.StartInfo.FileName = thisProcess.MainModule.FileName;
                process.StartInfo.WorkingDirectory = thisProcess.StartInfo.WorkingDirectory;
                GameDebug.Log(string.Format("filename='{0}', dir='{1}'", process.StartInfo.FileName, process.StartInfo.WorkingDirectory));
            }

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = " -batchmode -nographics -noboot -consolerestorefocus" +
                                          " +serve " + levelname + " +game.modename " + gamemode.ToLower() +
                                          " +servername \"" + servername + "\"";
           
            
            if (process.Start()){
                GameDebug.Log("game process started");
                //StartCoroutine(SendConnect(10));
                ShowSubMenu(introMenu);
            }
            
        }
        else
        {
            Console.EnqueueCommand("serve " + levelname);
            Console.EnqueueCommand("servername \"" + servername + "\"");
        }
        
    }
   

    private void CreateServer() {
        var servername = uiBinding.servername.text;

        var levelname = uiBinding.levelname.options[uiBinding.levelname.value].text;

        // TODO : Add commands to set these
        var gamemode = uiBinding.gamemode.options[uiBinding.gamemode.value].text.ToLower();
        var maxplayers = 8;

        var dedicated = uiBinding.decidatedServer.isOn;
        if (dedicated) {
            var process = new System.Diagnostics.Process();
            if (Application.isEditor) {
                process.StartInfo.FileName = k_AutoBuildPath + "/" + k_AutoBuildExe;
                process.StartInfo.WorkingDirectory = k_AutoBuildPath;
            }
            else {
                // TODO : We should look to make this more robust but for now we just
                // kill other processes to avoid running multiple servers locally
                var thisProcess = System.Diagnostics.Process.GetCurrentProcess();
                var processes = System.Diagnostics.Process.GetProcesses();
                foreach (var p in processes) {
                    if (p.Id != thisProcess.Id && p.ProcessName == thisProcess.ProcessName) {
                        try {
                            // p.Kill();
                        }
                        catch (System.Exception) {
                        }
                    }
                }

                process.StartInfo.FileName = thisProcess.MainModule.FileName;
                process.StartInfo.WorkingDirectory = thisProcess.StartInfo.WorkingDirectory;
                GameDebug.Log(string.Format("filename='{0}', dir='{1}'", process.StartInfo.FileName,
                    process.StartInfo.WorkingDirectory));
            }

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = " -batchmode -nographics -noboot -consolerestorefocus" +
                                          " +serve " + "Level_01" + " +game.modename "
                                          + ("Deathmatch".ToLower()) +
                                          " +servername \"" + servername + "\"";
            if (process.Start()) {
                GameDebug.Log("game process started");
                //StartCoroutine(SendConnect(10));
                //ShowSubMenu(introMenu);
            }
        }
        else {
            Console.EnqueueCommand("serve " + levelname);
            Console.EnqueueCommand("servername \"" + servername + "\"");
        }
    }

    public void CreateBotsFromLobby() {
        var b0Count =  gdnClientBrowserNetworkDriver.lobbyValue.team0.BotCount();
        var b1Count =  gdnClientBrowserNetworkDriver.lobbyValue.team1.BotCount();
        GameDebugPlus.Log(MMLog.Mm, cls, "CreateBotsFromLobby()", "bot lobby: " +
                 b0Count + " : " + b1Count);
       
          
            foreach (var slot in gdnClientBrowserNetworkDriver.lobbyValue.team0.slots) {
                if (slot.isBot) {
                    GameDebugPlus.Log(MMLog.Mm, cls, "CreateBotsFromLobby()", "isBot: " + 
                        slot.playerName);
                    CreateBot(slot.playerName);
                }
            }
            foreach (var slot in gdnClientBrowserNetworkDriver.lobbyValue.team1.slots) {
                if (slot.isBot) {
                    GameDebugPlus.Log(MMLog.Mm, cls, "CreateBotsFromLobby()", "isBot: " + 
                                                                              slot.playerName);
                    CreateBot(slot.playerName);
                }
            }
    }

    public void CreateBot(string aName) {
        var process = new System.Diagnostics.Process();
        var thisProcess = System.Diagnostics.Process.GetCurrentProcess();
        process.StartInfo.FileName = thisProcess.MainModule.FileName;
        process.StartInfo.WorkingDirectory = thisProcess.StartInfo.WorkingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Arguments = " -batchmode -nographics -forceclientsystems" +
                                      " +client localhost "+
                                      " +isbotstring yes" + 
                                      " +debugmove 2 " +
                                      " +client.debugmovename " +aName ;
        GameDebugPlus.Log(MMLog.Mm, cls, "CreateBot", "bot process arguments: " + process.StartInfo.Arguments );
        if (process.Start()) {
            GameDebugPlus.Log(MMLog.Mm, cls, "CreateBot", "bot process started: " + aName);
        }
        else {
            GameDebugPlus.Log(MMLog.Mm, cls, "CreateBot", "bot process filed start: " + aName);
        } 
    }

    public void JoinGame() {
        Console.EnqueueCommand("connect localhost");
    }
    
    IEnumerator SendConnect(float delay)
    {
        Debug.Log("mainMenu OnCreateGame waiting: " + delay);
        //yield on a new YieldInstruction that waits for delay seconds.
        yield return new WaitForSeconds(delay);
        Console.EnqueueCommand("connect localhost");
        Debug.Log("mainMenu OnCreateGame SendConnect connect localhost");
    }
    
    public static readonly string k_AutoBuildPath = "AutoBuild";
    public static readonly string k_AutoBuildExe = "AutoBuild.exe";

}
