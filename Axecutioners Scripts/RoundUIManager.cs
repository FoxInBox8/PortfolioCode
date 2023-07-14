using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

public class RoundUIManager : MonoBehaviour
{
	// Constants
	private float arrowOffset = 10f;					// Amount to move the arrow each time it is activated
	private float[] arrowStartPositions = new float[2];	// The default x positions of the arrows
	private float timeBetweenOperaionsRounds = 0.25f;	// Delay between operations of increasing/decreasing the round counter
	private float timeBetweenOperaionsLength = 0.25f;	// Delay between operations of increasing/decreasing the round length
	public int maxRounds;								// Maximum number of rounds allowed; set in inspector (min is 1)
	public int minLength;								// Minimum length of each round allowed; set in inspector
	public int maxLength;								// Maximum length of each round allowed; set in inspector
	public int lengthBeforeJump;						// Number before the round length jumps to the maximum; set in inspector
	public int lengthDelta;								// Amount the length of the round changes each click; set in inspector

	// External Objects
	public GameObject roundsLabel;						// The label for the current number of rounds (number only)
	public GameObject roundsButton;						// Button that displays the number of rounds (number and text)
	public GameObject timeLabel;						// The label for the current length of each round (number only)
	public GameObject timeButton;                       // Button that displays the length of each round (number and text)
	public GameObject[] arrows;							// The arrows that move when the values are changed
	public GameObject[] arrowButtons;					// The buttons for the arrows

	// Variables
	public int numberOfRounds;							// Current number of rounds to be played
	public int lengthOfRound;							// Current length of each round
	private float operationTimer;						// Timer to keep track of when to perform the next operation
	private Vector2 input;								// Input left and right (-1, 1) to determine when to increase/decrease the selected counter; value set by input system

	void Start()
	{
		// Save the default x positions of the arrows
		arrowStartPositions[0] = arrows[0].transform.position.x;
		arrowStartPositions[1] = arrows[1].transform.position.x;

		// Reset the point counters for the players
		RoundManager.points[0] = 0;
		RoundManager.points[1] = 0;

		// Set defaults
		RoundManager.currentRound = 1;
		RoundManager.numRounds = numberOfRounds;
		RoundManager.roundLength = lengthOfRound;
    }

	void FixedUpdate()
	{
		GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
		// If the selected button is the rounds button
		if (selectedButton == roundsButton)
		{
			// Move the arrows to the appropriate location
			MoveArrow(0, arrowStartPositions[0], roundsButton);
			MoveArrow(1, arrowStartPositions[1], roundsButton);
			// Update the round counter
			UpdateRounds();
		}
		// If the selected button is the length button
		else if (selectedButton == timeButton)
		{
			// Move the arrows to the appropriate location
			MoveArrow(0, arrowStartPositions[0], timeButton);
			MoveArrow(1, arrowStartPositions[1], timeButton);
			// Update the time counter
			UpdateTime();
		}
		// If any other button is selected, hide the arrows by moving them offscreen
		else if (!(selectedButton == arrowButtons[0] || selectedButton == arrowButtons[1]))
		{
			MoveArrow(0, -100, timeButton);
			MoveArrow(1, -100, timeButton);
		}
		
		// Update the values in the rounds manager to use in game
		RoundManager.numRounds = numberOfRounds;
		RoundManager.roundLength = lengthOfRound;

		// Update the display
		roundsLabel.GetComponent<TMP_Text>().text = numberOfRounds.ToString();
		timeLabel.GetComponent<TMP_Text>().text = lengthOfRound.ToString();
	}

	// Gets input from input system. Called when horiozntal movement keys are pressed
	public void GetInput(InputAction.CallbackContext context)
	{
		input = context.ReadValue<Vector2>();
	}

	// Update the number of rounds to be played
	void UpdateRounds()
	{
		// Disable the appropriate arrow if the max or min value is reached
		arrows[0].SetActive(numberOfRounds != 1);
		arrows[1].SetActive(numberOfRounds != maxRounds);

		// If right input
		if (input.x > 0)
		{
			// Increase the number of rounds every operation cycle
			if (operationTimer >= timeBetweenOperaionsRounds)
			{
				changeRounds(1);
				operationTimer = 0;
				// Bump the corresponding arrow out to visualize the change
				MoveArrow(1, arrows[1].transform.position.x + arrowOffset, roundsButton);
			}
		}
		// If left input
		else if (input.x < 0)
		{
			// Decrease the number of rounds every operation cycle
			if (operationTimer >= timeBetweenOperaionsRounds)
			{
				changeRounds(-1);
				operationTimer = 0;
				// Bump the corresponding arrow out to visualize the change
				MoveArrow(0, arrows[0].transform.position.x - arrowOffset, roundsButton);
			}
		}
		else
		{
			// Reset the operaion timer to ensure the next input is immediately read
			operationTimer = timeBetweenOperaionsRounds;
		}
		// Increase the operation timer
		operationTimer += Time.deltaTime;
	}

	// Update the length of each round
	void UpdateTime()
	{
		// Disable the appropriate arrow if the max or min value is reached
		arrows[0].SetActive(lengthOfRound != minLength);
		arrows[1].SetActive(lengthOfRound != maxLength);

		// If right input
		if (input.x > 0)
		{
			// Increase the length of rounds every operation cycle
			if (operationTimer >= timeBetweenOperaionsLength)
			{
                changeTime(lengthDelta);
                operationTimer = 0;
                // Bump the corresponding arrow out to visualize the change
                MoveArrow(1, arrows[1].transform.position.x + arrowOffset, timeButton);
            }
		}
		// If left input
		else if (input.x < 0)
		{
			// Decrease the length of rounds every operation cycle
			if (operationTimer >= timeBetweenOperaionsLength)
			{
                changeTime(-lengthDelta);
                operationTimer = 0;
				// Bump the corresponding arrow out to visualize the change
				MoveArrow(0, arrows[0].transform.position.x - arrowOffset, timeButton);
			}
		}
		else
		{
			// Reset the operaion timer to ensure the next input is immediately read
			operationTimer = timeBetweenOperaionsLength;
		}
		// Increase the operation timer
		operationTimer += Time.deltaTime;
	}
	
	void changeRounds(int delta)
	{
		numberOfRounds = Mathf.Clamp(numberOfRounds + delta, 1, maxRounds);
	}
	
	void changeTime(int delta)
	{
		int newLength = lengthOfRound + delta;
		if (newLength > lengthBeforeJump) newLength = (delta > 0) ? maxLength : lengthBeforeJump;
		newLength = Mathf.Clamp(newLength, minLength, maxLength);
		lengthOfRound = newLength;
	}

	// Move the corresponding arrow to the hright of the button and the specified x value
	void MoveArrow(int arrowNum, float x, GameObject button)
	{
		arrows[arrowNum].transform.position = new Vector3(x, button.transform.position.y, 0);
	}
	
	public void clickArrow(int arrowSide)
	{
		if (EventSystem.current.currentSelectedGameObject.transform.position.y == roundsButton.transform.position.y)
		{
			EventSystem.current.SetSelectedGameObject(roundsButton);
			changeRounds(arrowSide);
		}
		if (EventSystem.current.currentSelectedGameObject.transform.position.y == timeButton.transform.position.y)
		{
			EventSystem.current.SetSelectedGameObject(timeButton);
			changeTime(arrowSide * lengthDelta);
		}
	}
}