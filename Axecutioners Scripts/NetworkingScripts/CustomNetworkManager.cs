using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;
using Telepathy;
using Mirror.Examples.MultipleMatch;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Assertions.Must;
using static UnityEngine.GraphicsBuffer;

public struct CreatePlayer1Message : NetworkMessage
{

}
public struct CreatePlayer2Message : NetworkMessage
{

}

public enum NetworkID
{
    INVALID = -1,
    PLAYER2 = 0,
    PLAYER1 = 1
}

public class CustomNetworkManager : NetworkManager
{
    public List<NetworkPlayerController> playerList = new List<NetworkPlayerController>();

    //change scene
    public void ServerChange(string scene)
    {
        ServerChangeScene(scene);
    }

    //when a server first starts - setup messages that the server will use to spawn players
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server Started!");

        NetworkServer.RegisterHandler<CreatePlayer1Message>(CreatePlayer1);
        NetworkServer.RegisterHandler<CreatePlayer2Message>(CreatePlayer2);
    }
    //when a server stops
    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server Stopped!");
    }
    //when a client connects, create a player object for them
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Client Connected: " + NetworkClient.connection.connectionId);

        if (!NetworkServer.active)
        {
            CreatePlayer2Message msg = new CreatePlayer2Message();
            NetworkClient.Send(msg);
        }
    }
    public override void OnClientDisconnect()
    {
        Debug.Log("Client Disconnected!");
    }

    //these create player 1 or 2 and set authority to the gameobject from the connection passed
    public void CreatePlayer1(NetworkConnectionToClient conn, CreatePlayer1Message msg)
    {
        Debug.Log("Spawned Player 1");

        GameObject player = Instantiate(spawnPrefabs[0]);

        //make sure input is set
        LobbyController.Instance.FindManagers();
        LobbyController.Instance.inputManager.playerControllers[0] = player.GetComponent<PlayerScript>();
        LobbyController.Instance.inputManager.playerInputs[0] = player.GetComponent<PlayerInput>();
        //LobbyController.Instance.lobbyName.GetComponent<TextMeshProUGUI>().text = playerList[0].player_name + "'s Lobby";

        if (NetworkServer.AddPlayerForConnection(conn, player) == false)
            Debug.LogError("Adding player 1 for connection failed!");
    }
    public void CreatePlayer2(NetworkConnectionToClient conn, CreatePlayer2Message msg)
    {
        Debug.Log("Spawned Player 2");

        GameObject player = Instantiate(spawnPrefabs[1]);
        LobbyController.Instance.FindManagers();
        LobbyController.Instance.inputManager.playerControllers[1] = player.GetComponent<PlayerScript>();
        LobbyController.Instance.inputManager.playerInputs[1] = player.GetComponent<PlayerInput>();

        if (NetworkServer.AddPlayerForConnection(conn, player) == false)
            Debug.LogError("Adding player 2 for connection failed!");
    }

    //reset players when restarting rounds - reset player data and pause for FIGHT countdown
    public void ResetPlayers()
    {
        Debug.Log("Reset Players");
        LobbyController.Instance.Reset();

        //enable collider and animator
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<BoxCollider>().enabled = true;
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<BoxCollider>().enabled = true;
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<Animator>().enabled = true;
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<Animator>().enabled = true;

        LobbyController.Instance.FindManagers();
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().uiManager = LobbyController.Instance.UImanager;
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().cameraMover = LobbyController.Instance.gameCamera;
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().fmodAudioManager = LobbyController.Instance.fmodManager;
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().crownZone = LobbyController.Instance.crownZone;
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().necromancer = LobbyController.Instance.necromancer;
        playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().uiManager = LobbyController.Instance.UImanager;
        playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().cameraMover = LobbyController.Instance.gameCamera;
        playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().fmodAudioManager = LobbyController.Instance.fmodManager;
        playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().crownZone = LobbyController.Instance.crownZone;
        playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().necromancer = LobbyController.Instance.necromancer;

        //reset hp
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().HP = 3;
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().HP = 3;

        //make sure animation is set before round starts
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().EnterState(PlayerState.IDLE);
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().UpdateAnimation();
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().EnterState(PlayerState.IDLE);
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().UpdateAnimation();

        //pause
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().networkPause = true;
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().networkPause = true;
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().dead = false;
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().dead = false;

        //pause
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

        //reset targets
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().target = playerList[(int)NetworkID.PLAYER2].gameObject;
        playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().target = playerList[(int)NetworkID.PLAYER1].gameObject;

        //input - array position in playerControllers/playerInputs is different with playerList
        LobbyController.Instance.inputManager.playerControllers[0] = playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>();
        LobbyController.Instance.inputManager.playerInputs[0] = playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerInput>();
        LobbyController.Instance.inputManager.playerControllers[1] = playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>();
        LobbyController.Instance.inputManager.playerInputs[1] = playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerInput>();

        //reset UI manager players[]
        LobbyController.Instance.UImanager.players[0] = playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>();
        LobbyController.Instance.UImanager.players[1] = playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>();

        //reset timers
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().ResetTimers();
        playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().ResetTimers();

        //reset position
        playerList[(int)NetworkID.PLAYER1].transform.position = new Vector3(1.5f, 0.01f, 0f);
        playerList[(int)NetworkID.PLAYER2].transform.position = new Vector3(-1.5f, 0.01f, 0f);

        //update points - rpc so only call player1
        playerList[(int)NetworkID.PLAYER1].SetPoints(RoundManager.points[0], RoundManager.points[1]);

        //reset crown
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().ResetCrown();
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().ResetCrown();

        //reset dismemberment and camera
        LobbyController.Instance.ResetDismemberment();
        LobbyController.Instance.ResetCamera(playerList[(int)NetworkID.PLAYER1].gameObject, playerList[(int)NetworkID.PLAYER2].gameObject);

        //pause
        playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().networkPause = true;
        playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().networkPause = true;

        //make sure round is correct
        playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().advanceRound();
    }
    public void DestroyPlayers()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            Destroy(playerList[i].gameObject);
        }
    }
}