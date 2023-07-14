using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
	// Go to the scene
	public void GoToScene(string sceneName)
	{
		if (sceneName == "NetworkRoundsMenu" && SteamAPI.Init() == false) { return; }
		
		SceneManager.LoadScene(sceneName);
    }

	public void GoToRandomPlayScene()
	{
		//int theScene = 2;
		//switch (theScene)
		switch(Random.Range(0, 3))
		{
			case 0:
                SceneManager.LoadScene("PlayScene"); // I will want to rename this to TownPlayScene
				break;
			case 1:
                SceneManager.LoadScene("DungeonPlayScene");
				break;
            case 2:
                SceneManager.LoadScene("ThroneRoomPlayScene");
                break;
            default:
                SceneManager.LoadScene("PlayScene");
				break;
        }

    }

	// Quit the game
	public void QuitGame()
	{
		Application.Quit();
	}
}