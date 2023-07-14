using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndUIManager : MonoBehaviour
{
	public GameObject[] backgrounds;

	void Update()
	{
		// Show the correct backgronud (player 1 win, player 2 win, tie) based on who has more points
		backgrounds[0].SetActive(RoundManager.points[0] > RoundManager.points[1]);
		backgrounds[1].SetActive(RoundManager.points[1] > RoundManager.points[0]);
		backgrounds[2].SetActive(RoundManager.points[0] == RoundManager.points[1]);
	}
}