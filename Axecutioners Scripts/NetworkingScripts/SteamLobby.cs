using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance { get; private set; }

    public bool init, retrievedLobbies, restartServer = false;
    public ulong lobby_id;
    public CSteamID steam_id, host_id;
    private const string hostKey = "HostAddress";
    private CustomNetworkManager manager;
    public string hostName;
    public int hostImg;

    public const int MAX_LOBBIES_SHOWN = 7;


    /*      function pointers for Steamworks      */
    //hosting/joining lobbies
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequest;
    protected Callback<LobbyEnter_t> lobbyEntered;
    
    //icon bs
    protected Callback<AvatarImageLoaded_t> loadIcon;
    
    //lobby list
    protected Callback<LobbyMatchList_t> lobbyList;
    protected Callback<LobbyDataUpdate_t> lobbyData;

    public List<CSteamID> steamLobbies = new List<CSteamID>();

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
                return manager;

            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //check if steam is open
        if (!SteamManager.Initialized)
            SteamAPI.Init();

        manager = GetComponent<CustomNetworkManager>();

        //set functions to callbacks
        lobbyCreated = Callback<LobbyCreated_t>.Create(CreateLobby);
        joinRequest = Callback<GameLobbyJoinRequested_t>.Create(JoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(JoinLobby);
        loadIcon = Callback<AvatarImageLoaded_t>.Create(LoadIcons);
        lobbyList = Callback<LobbyMatchList_t>.Create(GetLobbyList);
        lobbyData = Callback<LobbyDataUpdate_t>.Create(UpdateLobbyData);

        Manager.GetComponent<SteamManager>().enabled = true;
        //SetAchievements();
    }

    private void SetAchievements()
    {
        SteamUserStats.SetAchievement("Tutorial");
    }

    public void ChangeScene(string sceneName)
    {
        Manager.ServerChangeScene(sceneName);
    }

    public void GetLobbiesList()
    {
        if (steamLobbies.Count > 0)
            steamLobbies.Clear();

        LobbyList.Instance.ClearOpenLobbies();

        //set filters for what lobbies we want to find
        //SteamMatchmaking.AddRequestLobbyListStringFilter("name", "AXECUTIONERS", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
        SteamMatchmaking.RequestLobbyList();
    }

    public void HostPrivate(string scene)
    {
        //create a friends only lobby with max of 2 connections
        //if (restartServer)
        //{
        //    restartServer = false;
        //    Manager.StartServer();
        //}

        //Manager.GetComponent<SteamManager>().enabled = true;
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
        Manager.StartHost();
        Manager.ServerChangeScene(scene);
    }
    public void HostPublic(string scene)
    {
        //create a public lobby with max of 2 connections
        //if (restartServer)
        //{
        //    restartServer = false;
        //    Manager.StartHost();
        //}
        
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 2);
        Manager.StartHost();
        Manager.ServerChangeScene(scene);
    }
    public void Join(CSteamID lobbyID, string scene)
    {
        //GetLobbiesList();

        //Manager.StartClient();
        SteamMatchmaking.JoinLobby(lobbyID);
        //Manager.StartClient();
        Manager.ServerChangeScene(scene);
    }
    public void Leave()
    {
        SteamMatchmaking.LeaveLobby((CSteamID)lobby_id);
    }

    public string GetHostName()
    {
        return hostName;
    }

    #region Steamworks Callbacks
    //callback when creating a lobby
    private void CreateLobby(LobbyCreated_t callback)
    {
        //checks for Steamworks API/Web API errors
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        //Manager.StartHost();

        //set host to current Steam user
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostKey, SteamUser.GetSteamID().ToString());

        //set lobby name
        //string lobbyName = "AXECUTIONERS";
        hostName = SteamFriends.GetPersonaName().ToString() + "'s Lobby";
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", hostName);

        host_id = SteamUser.GetSteamID();
        //steam_id = SteamUser.GetSteamID();
        //SteamMatchmaking.SetLobbyOwner((CSteamID)callback.m_ulSteamIDLobby, steam_id);

        Debug.Log("Lobby Created: " + name);
    }
    //callback when requesting to join
    private void JoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

        Debug.Log("Join request from: " + SteamFriends.GetPersonaName().ToString());
    }
    //callback when joining a lobby
    private void JoinLobby(LobbyEnter_t callback)
    {
        lobby_id = callback.m_ulSteamIDLobby;
        steam_id = SteamUser.GetSteamID();
        string name = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        if (steam_id != host_id)
        {
            //if not host, start client
            Manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostKey);
            Manager.StartClient();
        }

        Debug.Log("Joined Lobby: " + name);
    }
    //callback when loading icons (if needed)
    private void LoadIcons(AvatarImageLoaded_t callback)
    {
        Debug.Log("loaded icons");
        hostImg = callback.m_iImage;
    }
    //callback when getting the list of open public steam lobbies
    private void GetLobbyList(LobbyMatchList_t callback)
    {
        if (LobbyList.Instance.openLobbies.Count > 0)
            LobbyList.Instance.DestroyLobbies();

        Debug.Log("Called GetLobby Callback");

        //adds all open lobbies from list to the lobby list ingame
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            //restricts lobby's shown in list
            //if (i >= 7)
            //    return;

            CSteamID id = SteamMatchmaking.GetLobbyByIndex(i);
            steamLobbies.Add(id);
            SteamMatchmaking.RequestLobbyData(id);
        }
        
        retrievedLobbies = true;
    }
    //callback when updating lobby
    private void UpdateLobbyData(LobbyDataUpdate_t callback)
    {
        if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            LobbyList.Instance.DisplayLobbies(steamLobbies, callback);

            //if (steamLobbies.Count > 0)
            //    LobbyList.Instance.SetSelectables();
        }
    }
    private void UpdateAchievements(GSClientAchievementStatus_t callback)
    {

    }
    #endregion

    //for loading player icons - takes a steam image int that is returned by Steamworks and transforms it to a Texture2D
    public Texture2D SteamImageToTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        return texture;
    }

    //returns the player object from the given network identity asset id
    public NetworkPlayerController Get(uint id)
    {
        for (int i = 0; i <  Manager.playerList.Count; i++)
            if (Manager.playerList[i].gameObject.GetComponent<NetworkIdentity>().assetId == id)
                return Manager.playerList[i];

        Debug.LogError("Could not get player from asset id!");
        return null;
    }
    public int GetIndex(uint id)
    {
        for (int i = 0; i < Manager.playerList.Count; i++)
            if (Manager.playerList[i].gameObject.GetComponent<NetworkIdentity>().assetId == id)
                return i;

        Debug.LogError("Could not get player index from asset id!");
        return -1;
    }

    public string GetName()
    {
        return SteamFriends.GetPersonaName();
    }
    public int GetIcon()
    {
        return SteamFriends.GetLargeFriendAvatar(steam_id);
    }
}