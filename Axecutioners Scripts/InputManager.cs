using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System;

public enum InputScheme
{
	KEYBOARD,
	GAMEPAD
}

public class InputManager : MonoBehaviour
{
	public PlayerInput[] playerInputs = new PlayerInput[2];
	public PlayerScript[] playerControllers;
	private GameUIManager gameUIManager;
	private InputDevice keyboard = new InputDevice();
	private InputDevice mouse = new InputDevice();
	private InputDevice[] gamepads = new InputDevice[2];
	
	private void Start()
	{
		// Get the input devices
		searchInputDevices();

		// Assign initial schemes to the players
		setInputScheme(1, RoundManager.playerInputSchemes[0]);
		setInputScheme(2, RoundManager.playerInputSchemes[1]);

		gameUIManager = FindObjectOfType<GameUIManager>();
		
		setInputScheme(1, InputScheme.KEYBOARD);
        setInputScheme(2, InputScheme.KEYBOARD);
		setInputScheme(2, InputScheme.GAMEPAD);
		setInputScheme(1, InputScheme.GAMEPAD);
	}

    private void OnEnable()
    {
        // This makes sure that any time a device is changed, we know about it
        InputSystem.onDeviceChange += deviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= deviceChange;
    }

    //need to init at a specific time for networking
    public void NetworkInit()
	{
        //playerControllers[0] = player1.GetComponent<PlayerScript>();
        //playerControllers[1] = player2.GetComponent<PlayerScript>();

        //playerInputs[0] = player1.GetComponent<PlayerInput>();
        //playerInputs[1] = player2.GetComponent<PlayerInput>();

        // Get the input devices
        searchInputDevices();

        // Assign initial schemes to the players
        setInputScheme(1, InputScheme.KEYBOARD);
        setInputScheme(2, InputScheme.KEYBOARD);
		setInputScheme(2, InputScheme.GAMEPAD);
		setInputScheme(1, InputScheme.GAMEPAD);

        // Add the searchInputDevices call to the onDeviceChange action callback
        // This makes sure that any time a device is changed, we know about it
        InputSystem.onDeviceChange += deviceChange;

        gameUIManager = FindObjectOfType<GameUIManager>();
    }

	private void deviceChange(InputDevice device, InputDeviceChange change)
	{
		if (change == InputDeviceChange.Added)
		{
			searchInputDevices();
			if (device.name == "XInputControllerWindows" || device.name == "XInputControllerWindows1")
			{
				setInputScheme(2, InputScheme.GAMEPAD);
				setInputScheme(1, InputScheme.GAMEPAD);
			}
		}
		else if (change == InputDeviceChange.Removed)
		{
			searchInputDevices();
			if (gameUIManager) gameUIManager.PauseGame(true);
			if (device.name == "XInputControllerWindows" || device.name == "XInputControllerWindows1")
			{
				if (playerControllers[0].inputDevice == device)
				{
					setInputScheme(1, InputScheme.KEYBOARD);
				}
				else if (playerControllers[1].inputDevice == device)
				{
					setInputScheme(2, InputScheme.KEYBOARD);
				}
			}
		}
	}

	private void searchInputDevices()
	{
		// Goes through the connected devices and extracts the ones we care about
		foreach (InputDevice id in InputSystem.devices)
		{
			switch (id.name)
			{
				case "Keyboard":
					keyboard = id;
					break;

				case "Mouse":
					mouse = id;
					break;
				
				case "XInputControllerWindows":
					gamepads[0] = id;
					break;

				case "XInputControllerWindows1":
					gamepads[1] = id;
					break;

				default:
					//Debug.Log("Unrecognized device connected");
					break;
			}
		}
	}

	// Updates the player's input scheme
	public void setInputScheme(int playerID, InputScheme inputScheme)
	{
		// Actually make the player input start using that object
		try
		{
			switch (inputScheme)
			{
				// If we want to set the controll scheme to keyboard, we need to make sure there is one (this will likely be true, don't know what'll happen if it isn't ¯\_(ツ)_/¯)
				case InputScheme.KEYBOARD:
					if (InputSystem.GetDevice<Keyboard>() != null && InputSystem.GetDevice<Mouse>() != null)
					{
						updatePlayerInputScheme(playerID - 1, InputScheme.KEYBOARD, "KeyboardMouse", keyboard, mouse);
					}
					break;
				
				// Connecting to a gamepad is tricker because it might not be plugged in
				case InputScheme.GAMEPAD:
					// If there are none, send an error message
					if (Gamepad.all.Count == 0)
					{
						Debug.Log("No Gamepads connected");
					}
					// If there's only one, connect that player to the gamepad and the other to keyboard
					else if (Gamepad.all.Count == 1)
					{
						updatePlayerInputScheme(playerID - 1, InputScheme.GAMEPAD, "Gamepad", Gamepad.current);
						// Make sure there's a keyboard and mouse
						if (InputSystem.GetDevice<Keyboard>() != null && InputSystem.GetDevice<Mouse>() != null)
						{
							updatePlayerInputScheme(2 - playerID, InputScheme.KEYBOARD, "KeyboardMouse", keyboard, mouse);
						}
					}
					// If there are at least two, we gucci
					else if (Gamepad.all.Count >= 2)
					{
						updatePlayerInputScheme(playerID - 1, InputScheme.GAMEPAD, "Gamepad", gamepads[playerID - 1]);
					}
					break;
			}
		}
		catch (Exception e) {Debug.Log(e);}
	}

	private void updatePlayerInputScheme(int index, InputScheme inputScheme, string inputSchemeName, params InputDevice[] inputDevices)
	{
		// Update the player's input scheme and device (in the player controller)
		if (playerControllers[index] != null && playerInputs[index] != null)
		{
            playerControllers[index].inputScheme = inputScheme;
            playerControllers[index].inputDevice = inputDevices[0];
            // Update the device they are using for the PlayerInput
            playerInputs[index].SwitchCurrentControlScheme(inputSchemeName, inputDevices);
        }
	}

	// Updates the player's input scheme, called by UI buttons
	public void setInputScheme(int newScheme)
	{
		// Apparently invoking unity events only allows for one variable to be passed, which is dumb but whatever
		setInputScheme(1 + (newScheme >> 1), (InputScheme)(newScheme % 2));
	}
}