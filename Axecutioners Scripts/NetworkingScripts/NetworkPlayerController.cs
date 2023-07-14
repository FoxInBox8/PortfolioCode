using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Windows;
using TMPro;

public class NetworkPlayerController : NetworkBehaviour
{
    //synced across the network
    [SyncVar] public int connection_id;
    [SyncVar] public int player_id;
    [SyncVar] public ulong steam_id;
    [SyncVar] public Texture2D icon;

    //essentially a function pointer for when a variable changes - OBSOLETE
    //[SyncVar(hook = nameof(PlayerHealthUpdate))] public int player_health;
    //[SyncVar(hook = nameof(PlayerNameUpdate))] public string player_name;
    //[SyncVar(hook = nameof(PlayerPointsUpdate))] public int player_points;

    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
                return manager;

            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        PlayerScript.someoneHit += hitNetwork;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void hitNetwork(GameObject hitPlayer)
    {
        UpdateHealth(hitPlayer.GetComponent<PlayerScript>().HP, SteamLobby.Instance.GetIndex(hitPlayer.GetComponent<NetworkIdentity>().assetId));
    }

    private void OnDestroy()
    {
        PlayerScript.someoneHit -= hitNetwork;
    }

    //sync var hooks - reaction to sync variables changing
    //private void PlayerNameUpdate(string oldValue, string newValue)
    //{
    //    if (isOwned)
    //    {
    //        player_name = newValue;
    //    }
    //}
    //private void PlayerHealthUpdate(int oldValue, int newValue)
    //{
    //    if (isOwned)
    //    {
    //        player_health = newValue;
    //    }
    //}
    //private void PlayerPointsUpdate(int oldValue, int newValue)
    //{
    //    if (isOwned)
    //    {
    //        player_points = newValue;
    //    }
    //}

    //network pause
    public void PauseGame()
    {
        LobbyController.Instance.UImanager.networkPause = !LobbyController.Instance.UImanager.networkPause;
        LobbyController.Instance.UImanager.TogglePause();
    }
    //set network client to ready
    public void Ready()
    {
        if (NetworkClient.ready == false)
            NetworkClient.Ready();
    }

    //when a NetworkIdentity component is first created
    public override void OnStartClient()
    {
        Debug.Log("Started Client");

        //base setup
        Manager.playerList.Add(gameObject.GetComponent<NetworkPlayerController>());
        PlayerScript player = gameObject.GetComponent<PlayerScript>();
        gameObject.transform.eulerAngles = new Vector3(0, 300f * this.gameObject.GetComponent<PlayerScript>().playerID - 450f, 0);

        //activate and pause input/mechanics for player
        player.networkActive = true;
        player.networkPause = false;

        //makes sure data is set
        LobbyController.Instance.FindManagers();
        player.uiManager = LobbyController.Instance.UImanager;
        player.cameraMover = LobbyController.Instance.gameCamera;
        player.fmodAudioManager = LobbyController.Instance.fmodManager;

        //if (isServer)
        //    player.uiManager.players[1] = gameObject.GetComponent<PlayerScript>();
        //else
        //    player.uiManager.players[0] = gameObject.GetComponent<PlayerScript>();

        player.crownZone = LobbyController.Instance.crownZone;
        player.necromancer = LobbyController.Instance.necromancer;

        //Create FMOD Event Instance of all Looping SFX and attach to player
        player.walkSFX = player.fmodAudioManager.CreateFMODEventInstance("PlayerWalk");
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(player.walkSFX, gameObject.transform);

        LobbyController.Instance.localplayer = gameObject;
        //LobbyController.Instance.UpdateLobby();

        string n = "error";

        //if the client is not the server then spawn player 1, since player 2 has to spawn before player 1
        //else this is player 1 and start the match
        if (isServer == false)
        {
            SetName(SteamLobby.Instance.GetName(), n, SteamLobby.Instance.GetIcon(), (int)NetworkID.PLAYER2);
            SpawnHostPlayer();
        }
        else if (Manager.playerList.Count > 1)
        {
            SetName(SteamLobby.Instance.GetName(), SteamLobby.Instance.hostName, SteamLobby.Instance.GetIcon(), (int)NetworkID.PLAYER1);
            StartGameRPC(RoundManager.numRounds, RoundManager.roundLength);
        }
        else
            Debug.Log("player count was too small: " + Manager.playerList.Count);
    }

    //when a NetworkIdentity component is destroyed
    public override void OnStopClient()
    {
        if (isServer)
        {
            //SteamMatchmaking.LeaveLobby((CSteamID)SteamLobby.Instance.lobby_id);
            //Manager.StopHost();

            if (gameObject != null)
                Destroy(gameObject);
        }
        else
        {
            //SteamMatchmaking.LeaveLobby((CSteamID)SteamLobby.Instance.lobby_id);
            //Manager.StopClient();

            if (gameObject != null)
                Destroy(gameObject);
        }

        //SteamLobby.Instance.restartServer = true;
        //Manager.StopServer();
        //Cleanup();
    }

    public void Disconnect()
    {
        Debug.Log("Disconnect");
        

        

        //Manager.ServerChangeScene("LobbyScene");
    }

    //commands sent from client to server NOTE: since the host of a Steam lobby is considered a client and server, must use RPCs to update clients 
    #region Commands
    //[Command]
    //private void PauseGameCMD()
    //{
    //    if (isServer)
    //        PauseGameRPC();
    //}
    //command functions get sent out to every client
    [Command]
    private void SpawnPlayerCMD()
    {
        CreatePlayer1Message msg = new CreatePlayer1Message();
        NetworkClient.Send(msg);
        Debug.Log("Spawn Player Command");
    }
    [Command]
    private void SetNameCMD(string name, int img, int id)
    {
        SetName(name, SteamLobby.Instance.hostName, img, id);
    }
    [Command]
    private void HealthCMD(int health, int id)
    {
        //Manager.playerList[id].GetComponent<PlayerScript>().HP = health;

        if (isServer)
            HealthRPC(health, id);
    }
    [Command]
    private void AttackCMD(float power, int id, int dmg, int stun, Vector3 launch)
    {
        //AttackValues atk = GetAttackValue(power, id);
        Manager.playerList[id].GetComponent<PlayerScript>().Attack(power, dmg, stun, launch);
        Manager.playerList[id].GetComponent<PlayerScript>().checkAttacks();

        Debug.Log("Attack Command: " + power);
    }
    [Command]
    private void MoveCMD(Vector2 moveDir, int id)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().Move(moveDir);
        Manager.playerList[id].GetComponent<PlayerScript>().UpdateAnimation();

        //if (isServer)
        //    MoveRPC(moveDir, id);
    }
    [Command]
    private void DashCMD(int id, float dash)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().Dash(dash);
        Manager.playerList[id].GetComponent<PlayerScript>().UpdateAnimation();

        //if (isServer)
        //    DashRPC(id, dash);
    }
    [Command]
    private void RestartCMD()
    {
        if (isServer)
            RestartRPC();
    }
    //[Command]
    //private void BlockCMD(float blockingPower, int id)
    //{
    //    Manager.playerList[id].GetComponent<PlayerScript>().Block(blockingPower);
    //}
    //[Command]
    //private void PointsCMD(int player1, int player2)
    //{
    //    if (isOwned)
    //        SetPoints(player1, player2);
    //}
    //[Command]
    //private void EndGameCMD(string scene)
    //{
    //    Debug.Log("endgame cmd");
    //    if (isServer)
    //        EndGameRPC(scene);
    //}
    private void CrownCMD(int id)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().CrownPlayer();
    }
    #endregion

    //remote procedure call functions can only be called by the server (host) and run the function on every client, including the host
    #region ClientRPCs
    [ClientRpc]
    private void SetNameRPC(string name, string hostName, int img, int id)
    {
        if (id == 0)
        {
            LobbyController.Instance.player2_name.GetComponent<TextMeshProUGUI>().text = name;

            if (img > 0)
            {
                LobbyController.Instance.player2_icon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, 180f);
                LobbyController.Instance.player2_icon.GetComponent<RawImage>().texture = SteamLobby.Instance.SteamImageToTexture(img);
            }
        }
        else if (id == 1)
        {
            LobbyController.Instance.player1_name.GetComponent<TextMeshProUGUI>().text = name;

            if (img > 0)
            {
                LobbyController.Instance.player1_icon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, 180f);
                LobbyController.Instance.player1_icon.GetComponent<RawImage>().texture = SteamLobby.Instance.SteamImageToTexture(img);
            }
        }
        else
            Debug.Log("Setting name failed - ID was wrong!");

        LobbyController.Instance.lobbyName.GetComponent<TextMeshProUGUI>().text = hostName;
    }
    [ClientRpc]
    private void MoveRPC(Vector2 moveDir, int id)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().Move(moveDir);
        Manager.playerList[id].GetComponent<PlayerScript>().UpdateAnimation();
    }
    [ClientRpc]
    private void StartGameRPC(int rounds, int length)
    {
        Debug.Log("Server - Start Game!"); 
        LobbyController.Instance.StartMatch(rounds, length);
    }
    //[ClientRpc]
    //private void PauseGameRPC()
    //{
    //    Debug.Log("Server - Pause Game!");
    //    LobbyController.Instance.UImanager.networkPause = !LobbyController.Instance.UImanager.networkPause;
    //    LobbyController.Instance.UImanager.TogglePause();
    //}
    [ClientRpc]
    private void RestartRPC()
    {
        Debug.Log("Server - Restart Round!");
        Manager.ResetPlayers();
    }
    [ClientRpc]
    private void EndGameRPC()
    {
        Debug.Log("Server - End Game!");
        Manager.DestroyPlayers();
        SteamMatchmaking.LeaveLobby((CSteamID)SteamLobby.Instance.lobby_id);
        //Manager.ServerChangeScene("EndScene");

        if (isServer)
            Manager.StopHost();
        else
            Manager.StopClient();

        //SteamLobby.Instance.restartServer = true;
        //Manager.StopServer();
    }
    [ClientRpc]
    private void AttackRPC(float power, int id, int dmg, int stun, Vector3 launch)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().Attack(power, dmg, stun, launch);
        Manager.playerList[id].GetComponent<PlayerScript>().checkAttacks();
    }
    [ClientRpc]
    private void DashRPC(int id, float dash)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().Dash(dash);
        Manager.playerList[id].GetComponent<PlayerScript>().UpdateAnimation();
    }
    //[ClientRpc]
    //private void BlockRPC(float blockingPower, int id)
    //{
    //    Manager.playerList[id].GetComponent<PlayerScript>().Block(blockingPower);
    //}
    [ClientRpc]
    private void HealthRPC(int health, int id)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().HP = health;

        Debug.Log("state: " + Manager.playerList[id].GetComponent<PlayerScript>().currentState);
        //means we encountered some latency and the health is synced, but clientside there was no hitstun animation because hitPlayer() was not called
        if (Manager.playerList[id].GetComponent<PlayerScript>().currentState != PlayerState.HITSTUN &&
            Manager.playerList[id].GetComponent<PlayerScript>().currentState != PlayerState.AERIAL)
        {
            Debug.LogError("[NETWORK] Dropped hitPlayer packet!");
            Manager.playerList[id].GetComponent<PlayerScript>().NetworkHit();
        }
    }
    [ClientRpc]
    private void CrownRPC(int id)
    {
        Manager.playerList[id].GetComponent<PlayerScript>().CrownPlayer();
    }
    [ClientRpc]
    private void CameraRPC()
    {
        Manager.playerList[0].GetComponent<PlayerScript>().ResetCamera();
        Manager.playerList[1].GetComponent<PlayerScript>().ResetCamera();
    }
    private void PointsRPC(int player1, int player2)
    {
        RoundManager.points[0] = player1;
        RoundManager.points[1] = player2;
    }
    #endregion

    private AttackValues GetAttackValue(float power, int id)
    {
        switch (power)
        {
            case 2.0f:
                return Manager.playerList[id].GetComponent<PlayerScript>().lightAttackValues;
            case 3.0f:
                return Manager.playerList[id].GetComponent<PlayerScript>().midAttackValues;
            case 4.0f:
                return Manager.playerList[id].GetComponent<PlayerScript>().heavyAttackValues;
            default:
                Debug.LogError("[NETWORK] Failed to get the correct attack value: " + power);
                return Manager.playerList[id].GetComponent<PlayerScript>().lightAttackValues;
        }
    }

    //these functions avoid straight command/RPC calls
    #region Command/RPC Calls
    public void SetPoints(int player1, int player2)
    {
        if (isServer)
            PointsRPC(player1, player2);
        //else
        //    PointsCMD(player1, player2);
    }
    public void ResetCamera()
    {
        if (isServer)
            CameraRPC();
    }
    public void Crown(int id)
    {
        //Call a crown command with the correct id
        if (isServer)
            CrownRPC(id);
        else
            CrownCMD(id);
    }
    public void SetName(string name, string hostName, int img, int id)
    {
        if (isServer)
            SetNameRPC(name, hostName, img, id);
        else
            SetNameCMD(name, img, id);
    }
    public void Move(Vector2 moveDir, int id)
    {
        if (isServer)
            MoveRPC(moveDir, id);
        else
            MoveCMD(moveDir, id);
    }
    //public void Block(float blocking, int id)
    //{
    //    if (isOwned)
    //        BlockCMD(blocking, id);
    //}
    public void Attack(float power, int id)
    {
        if (isServer)
        {
            if (power >= 2.0f)
            {
                AttackValues atk = GetAttackValue(power, id);
                AttackRPC(power, id, atk.damage, atk.stunTime, atk.launchPower);
            }
            else
                AttackRPC(power, id, 0, 0, Vector3.zero);
        }
        else
        {
            if (power >= 2.0f)
            {
                AttackValues atk = GetAttackValue(power, id);
                AttackCMD(power, id, atk.damage, atk.stunTime, atk.launchPower);
            }
            else
                AttackCMD(power, id, 0, 0, Vector3.zero);
        }
    }
    public void Dash(float dash, int id)
    {
        if (isServer)
            DashRPC(id, dash);
        else
            DashCMD(id, dash);
    }
    public void UpdateHealth(int health, int id)
    {
        //dont bother if player is dead
        if (health < 0)
            return;

        if (isServer)
            HealthRPC(health, id);
        else
            HealthCMD(health, id);
    }
    //public void UpdatePoints(int points)
    //{
    //    if (isOwned)
    //        PointsCMD(points);
    //}
    public void RestartGame()
    {
        if (isServer && LobbyController.Instance.restart)
        {
            RoundManager.currentRound++;
            LobbyController.Instance.restart = false;
            RestartRPC();
        }
        //else
        //    RestartCMD();
    }
    public void EndGame(string scene)
    {
        if (isServer)
            EndGameRPC();
    }
    public void SpawnHostPlayer()
    {
        if (isOwned)
            SpawnPlayerCMD();
    }
    public void IsDead()
    {

    }
    #endregion
}
