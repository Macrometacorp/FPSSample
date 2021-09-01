using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prelobby : MonoBehaviour {

    public GameObject lobbyListWait;
    public GameObject createLobbyWait;


    public void Wait(bool val) {
        lobbyListWait.SetActive(val);
        createLobbyWait.SetActive(val);
    }
    
}
