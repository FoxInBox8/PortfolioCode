using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //buttons for the network canvas when waiting for a player
    public void InviteFriends()
    {
        SteamFriends.ActivateGameOverlayInviteDialog((CSteamID)LobbyController.Instance.lobby_id);
    }
    public void LeaveLobby()
    {
        //SteamMatchmaking.LeaveLobby((CSteamID)SteamLobby.Instance.lobby_id);
        LobbyController.Instance.Leave();

        //SceneManager.LoadScene(scene);
        //SteamLobby.Instance.ChangeScene(scene);
    }
}
