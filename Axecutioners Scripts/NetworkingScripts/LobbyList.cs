using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Dynamic;
using UnityEditor;

public class LobbyList : MonoBehaviour
{
    public static LobbyList Instance { get; private set; }

    public GameObject lobbyMenu, lobbyItem, lobbyContent;
    public GameObject hostPrivateButton, hostPublicButton, refreshButton;
    public Scrollbar scrollbar;

    public List<LobbyData> openLobbies = new List<LobbyData>();

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
        GetLobbies();
    }

    private void Update()
    {
        GameObject currentButton = EventSystem.current.currentSelectedGameObject;
        if (currentButton && currentButton.name == "JoinButton")
        {
            LobbyData currentLD = currentButton.transform.parent.GetComponent<LobbyData>();
            float lobbyIndex = openLobbies.IndexOf(currentLD);
            scrollbar.value = 1 - (lobbyIndex / (openLobbies.Count - 1));
        }
    }

    //buttons
    public void GetLobbies()
    {
        //hostPrivateButton.SetActive(false);
        //hostPublicButton.SetActive(false);
        //refreshButton.SetActive(false);

        SteamLobby.Instance.GetLobbiesList();
    }
    public void HostPrivate(string scene)
    {
        SteamLobby.Instance.HostPrivate(scene);
    }
    public void HostPublic(string scene)
    {
        SteamLobby.Instance.HostPublic(scene);
    }

    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t callback)
    {
        //goes through open lobbies from steam and creates the UI prefab in the list
        for (int i = 0; i < lobbyIDs.Count; i++)
        {
            if (lobbyIDs[i].m_SteamID == callback.m_ulSteamIDLobby)
            {
                GameObject obj = Instantiate(lobbyItem);

                obj.GetComponent<LobbyData>().lobbyID = (CSteamID)lobbyIDs[i].m_SteamID;
                //obj.GetComponent<LobbyData>().lobbyName = SteamLobby.Instance.GetHostName();
                obj.GetComponent<LobbyData>().lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDs[i].m_SteamID, "name");
                obj.GetComponent<LobbyData>().SetLobbyData();

                obj.transform.SetParent(lobbyContent.transform);
                obj.transform.localScale = Vector3.one;

                openLobbies.Add(obj.GetComponent<LobbyData>());
            }
        }

        SetSelectables();
    }

    public void ClearOpenLobbies()
    {
        for (int i = openLobbies.Count - 1; i >= 0; i--)
        {
            Destroy(openLobbies[i].gameObject);
        }
        openLobbies.Clear();
    }

    public void SetSelectables()
    {
        Navigation menuNav = new Navigation();
        menuNav.mode = Navigation.Mode.Explicit;
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.Explicit;

        //make sure there is a list item and set the mainmenu button selectable to the first item
        if (openLobbies.Count > 0)
        {
            menuNav.selectOnRight = openLobbies[0].joinButton.GetComponent<Button>();
            menuNav.selectOnUp = refreshButton.GetComponent<Button>(); //refresh list
            menuNav.selectOnDown = hostPrivateButton.GetComponent<Button>(); //host private
            menuNav.selectOnLeft = refreshButton.GetComponent<Button>(); //refresh list
            hostPublicButton.GetComponent<Button>().navigation = menuNav;
        }

        //loop through joinbuttons
        for (int i = 0; i < openLobbies.Count; i++)
        {
            int temp;
            //makes sure there is a button below
            if (i < openLobbies.Count - 1)
            {
                temp = i + 1;
                nav.selectOnDown = openLobbies[temp].joinButton.GetComponent<Button>();
            }

            //makes sure there is a button above
            if (i > 0)
            {
                temp = i - 1;
                nav.selectOnUp = openLobbies[temp].joinButton.GetComponent<Button>();
            }

            nav.selectOnLeft = hostPublicButton.GetComponent<Button>();
            nav.selectOnRight = hostPublicButton.GetComponent<Button>();
            openLobbies[i].joinButton.GetComponent<Button>().navigation = nav;
        }
    }
    private GameObject GetSelectable(CSteamID id)
    {
        for (int i = 0; i < openLobbies.Count; i++)
        {
            if (openLobbies[i].lobbyID == id)
                return openLobbies[i].joinButton;
        }

        Debug.Log("Failed to set the selectables");
        return null;
    }

    public void DestroyLobbies()
    {
        for (int i = 0; i < openLobbies.Count; i++)
            Destroy(openLobbies[i]);

        openLobbies.Clear();
    }

    public void GoToMainMenu()
    {
        //Destroy(FindObjectOfType<CustomNetworkManager>().gameObject);
        Destroy(FindObjectOfType<FMODAudioManager>().gameObject);
        SceneManager.LoadScene("MainMenu");
    }
}
