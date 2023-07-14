using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Steamworks;

public class TutorialUIManager : MonoBehaviour
{
	private static TutorialUIManager tutorialManagerInstance;

	public static GameObject tutorialTextDisplay;
	private static string[] tutorialText;
	private string filePath;
	public static int step;
	
	public static string nextScene = "MainMenu";
	public TMP_Text textComponent;

	void Awake()
	{
		if (tutorialManagerInstance == null)
		{
			tutorialManagerInstance = this;
		}
		tutorialTextDisplay = GameObject.Find("TutorialText");
	}

	void Start()
	{
		filePath = Application.streamingAssetsPath + "/TutorialText.txt";
		tutorialText = File.ReadAllLines(filePath);
		step = -1;
	}

	void Update()
	{
		// Following code from https://youtu.be/FXMqUdP3XcE
		textComponent.ForceMeshUpdate();
		TMP_TextInfo textInfo = textComponent.textInfo;

		for (int i = 0; i < textInfo.characterCount; ++i)
		{
			TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
			if (!charInfo.isVisible)
			{
				continue;
			}
			Vector3[] verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
			for (int j = 0; j < 4; ++j)
			{
				Vector3 orig = verts[charInfo.vertexIndex + j];
				verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin((Time.time * 2) + (orig.x * 0.01f)) * 10, 0);
			}
		}

		for (int i = 0; i < textInfo.meshInfo.Length; ++i)
		{
			TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
			meshInfo.mesh.vertices = meshInfo.vertices;
			textComponent.UpdateGeometry(meshInfo.mesh, i);
		}
	}

	public static void advanceStep()
	{
		if (step < tutorialText.Length - 1)
		{
			step++;
			tutorialTextDisplay.GetComponent<TMP_Text>().text = "<line-height=60%><alpha=#70>" + tutorialText[step];
		}
		else
		{
            //bool tutorial = true;
            //SteamUserStats.GetAchievement("Tutorial", out tutorial);

            SceneManager.LoadSceneAsync(nextScene);
		}
	}
}