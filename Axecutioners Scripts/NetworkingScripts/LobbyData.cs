using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine.Assertions.Must;
using System;

public class LobbyData : MonoBehaviour
{
    public CSteamID lobbyID;
    public string lobbyName;
    public TextMeshProUGUI lobbyName_UI;
    public GameObject joinButton;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("LOBBY ID: " + lobbyID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLobbyData()
    {
        if (lobbyName != "")
            lobbyName_UI.text = lobbyName;
        else
            lobbyName_UI.text = "Empty";
    }

    //button
    public void JoinLobby(string scene)
    {
        //Steamworks functions are privated so we need to use an external call (ie cant use SteamLobby.Instance.JoinLobby)
        //LobbyList.Instance.GetLobbies();
        SteamLobby.Instance.Join(lobbyID, scene);
    }
}
