using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using FMOD;
using UnityEngine.UIElements;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance { get; private set; }

    const int MAX_PLAYERS = 2;

    //UI
    public GameObject lobbyName, player1_icon, player2_icon, player1_name, player2_name, lobbyData;
    public Texture2D player1_default_icon, player2_default_icon;

    public GameObject localplayer, crownZone, necromancer;
    public ulong lobby_id;

    private CustomNetworkManager manager;
    private Vector3 defaultCamPosition;

    public bool restart = false;

    public GameUIManager UImanager;
    public GameObject gameCamera, mainCamera;
    public FMODAudioManager fmodManager; 
    public InputManager inputManager;

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
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        lobby_id = SteamLobby.Instance.lobby_id;
        UImanager.networkActive = true;
        UImanager.networkPause = true;
        defaultCamPosition = new Vector3(0f, 0.9f, 3f);

        FindManagers();
        necromancer.GetComponent<Animator>().Play("Base Layer.Idle", 0, 0);

        //UI
        lobbyData.SetActive(true);
        lobbyName.SetActive(false);
        UImanager.countdownTimer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //resumes player script update functions
    public void StartPlayers()
    {
        Manager.playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().networkActive = true;
        Manager.playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().networkActive = true;
        Manager.playerList[(int)NetworkID.PLAYER1].GetComponent<PlayerScript>().networkPause = false;
        Manager.playerList[(int)NetworkID.PLAYER2].GetComponent<PlayerScript>().networkPause = false;

        //reset state because animation was getting stuck
        Manager.playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().EnterState(PlayerState.IDLE);
        Manager.playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().EnterState(PlayerState.IDLE);
        Manager.playerList[(int)NetworkID.PLAYER1].gameObject.GetComponent<PlayerScript>().UpdateAnimation();
        Manager.playerList[(int)NetworkID.PLAYER2].gameObject.GetComponent<PlayerScript>().UpdateAnimation();
    }

    //jank way to delete dismembered animations since THIS ENTIRE CODEBASE IS GHETTO AS FUCK PLEASE SEND HELP
    public void ResetDismemberment()
    {
        GameObject obj = GameObject.Find("LowDisP1(Clone)");
        if (obj != null)
        {
            Destroy(obj.gameObject);
            return;
        }

        obj = GameObject.Find("LowDisP2(Clone)");
        if (obj != null)
        {
            Destroy(obj.gameObject);
            return;
        }

        obj = GameObject.Find("MidDisP1(Clone)");
        if (obj != null)
        {
            Destroy(obj.gameObject);
            return;
        }

        obj = GameObject.Find("MidDisP2(Clone)");
        if (obj != null)
        {
            Destroy(obj.gameObject);
            return;
        }

        obj = GameObject.Find("HighDisP1(Clone)");
        if (obj != null)
        {
            Destroy(obj.gameObject);
            return;
        }

        obj = GameObject.Find("HighDisP2(Clone)");
        if (obj != null)
        {
            Destroy(obj.gameObject);
            return;
        }
    }

    //finds managers that players need to reference when loading in
    public void FindManagers()
    {
        if (necromancer == null)
            necromancer = GameObject.Find("Base_Necro_Rig").gameObject;

        if (UImanager == null)
        {
            UImanager = FindObjectOfType<GameUIManager>();

            if (UImanager.necromancer == null)
                UImanager.necromancer = necromancer;
        }

        if (gameCamera == null)
            gameCamera = FindObjectOfType<Camera>().gameObject;

        if (fmodManager == null)
            fmodManager = FindObjectOfType<FMODAudioManager>();

        if (inputManager == null)
            inputManager = FindObjectOfType<InputManager>();

        if (crownZone == null)
            crownZone = GameObject.Find("HealthZone").gameObject;
    }

    //update points based on ID
    public void UpdatePoints(int points, int id)
    {
        if (id == 0)
            RoundManager.points[1] = points;
        else if (id == 1)
            RoundManager.points[0] = points;
        else
            UnityEngine.Debug.LogError("[NETWORK] Failed to update points RPC");
    }

    //reset camera for restarting rounds
    public void ResetCamera(GameObject player, GameObject target)
    {
        gameCamera.transform.position = new Vector3((player.transform.position.x + target.transform.position.x) * 0.5f,
                                                          gameCamera.transform.position.y,
                                                          gameCamera.transform.position.z);
    }

    //reset lobby for restarting rounds
    public void Reset()
    {
        UnityEngine.Debug.Log("Reset Lobby");

        //unpause game and disable ready system
        inputManager.NetworkInit();
        UImanager.networkPause = false;

        //update UI
        UImanager.countdownTimer.SetActive(true);
        UImanager.roundRemainingTime = RoundManager.roundLength;
        UImanager.roundTimer.GetComponent<TextMeshProUGUI>().text = RoundManager.roundLength.ToString();

        lobbyData.SetActive(false);
        //readyUI.SetActive(false);
    }

    public void CheckRound()
    {
        //// This is added just in case this scrip is used in a scene that does not have a UI manager
        if (UImanager)
        {
            UImanager.ResetTimers();
        }

        //RoundManager.currentRound++;
        //RoundManager.playerInputSchemes[playerID - 1] = inputScheme;
        //RoundManager.playerInputSchemes[target.GetComponent<PlayerScript>().playerID - 1] = target.GetComponent<PlayerScript>().inputScheme;
    }

    //leave lobby button while waiting for player
    public void Leave()
    {
        //SteamLobby.Instance.restartServer = true;
        SteamLobby.Instance.Leave();
        Manager.StopHost();
    }

    public void FindPlayers()
    {
        UImanager.players[0] = Manager.playerList[1].gameObject.GetComponent<PlayerScript>();
        UImanager.players[1] = Manager.playerList[0].gameObject.GetComponent<PlayerScript>();
    }

    //called from an RPC that starts the match once players are loaded in
    public void StartMatch(int rounds, int length)
    {
        FindPlayers();
        necromancer.GetComponent<Animator>().Play("Base Layer.StartRound", 0, 0);

        //make sure rounds are set
        RoundManager.numRounds = rounds;
        RoundManager.roundLength = length;
        UImanager.roundRemainingTime = length;
        UImanager.roundTimer.GetComponent<TextMeshProUGUI>().text = length.ToString();

        //unpause game and disable ready system
        inputManager.NetworkInit();
        UImanager.networkPause = false;

        //set data
        if (Manager.playerList.Count > 1)
        {
            int p2 = 2, p1 = 1;
            Manager.playerList[0].gameObject.transform.eulerAngles = new Vector3(0, 200f * p2 - 300f, 0);
            Manager.playerList[1].gameObject.transform.eulerAngles = new Vector3(0, 200f * p1 - 300f, 0);

            Manager.playerList[0].GetComponent<PlayerScript>().target = Manager.playerList[1].gameObject;
            Manager.playerList[1].GetComponent<PlayerScript>().target = Manager.playerList[0].gameObject;

            inputManager.playerControllers[0] = Manager.playerList[1].GetComponent<PlayerScript>();
            inputManager.playerInputs[0] = Manager.playerList[1].GetComponent<PlayerInput>();
            inputManager.playerControllers[1] = Manager.playerList[0].GetComponent<PlayerScript>();
            inputManager.playerInputs[1] = Manager.playerList[0].GetComponent<PlayerInput>();
        }
        //else
        //    Debug.Log("Playerlist was too small: " + Manager.playerList.Count);

        //update UI
        UImanager.countdownTimer.SetActive(true);
        lobbyData.SetActive(false);
        //readyUI.SetActive(false);
        crownZone.SetActive(true);
        crownZone.GetComponent<Crown>().Reset();

        //name
        //lobbyName.SetActive(true);
        //lobbyName.GetComponent<TextMeshProUGUI>().text = "Axecutioners Lobby";
        //lobbyName.GetComponent<TextMeshProUGUI>().text = Manager.playerList[0].player_name;
    }
    
    public void Ready()
    {
        lobbyData.SetActive(false);
        //readyUI.SetActive(true);
    }
}
