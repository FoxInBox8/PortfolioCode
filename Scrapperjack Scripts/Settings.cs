using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class Settings : MonoBehaviour
{
    public enum SETTINGS_TYPE
    {
        X_SENSITIVITY = 0,
        Y_SENSITIVITY,
        VOLUME,
    }

    private enum ACTION
    {
        MOVE_FORWARD = 0,
        MOVE_BACKWARDS,
        MOVE_LEFT,
        MOVE_RIGHT,
        JUMP,
        LONG_JUMP,
        HIGH_JUMP,
        DASH,
        HOVER,
    }

    private const int NUM_FLOAT_SETTINGS = 3, FORWARD_INDEX = 1, BACKWARDS_INDEX = 2, LEFT_INDEX = 3, RIGHT_INDEX = 4;
    private const string X_SENS_KEY = "xSens", Y_SENS_KEY = "ySens", VOLUME_KEY = "volume", CONTROLS_KEY = "controls", INPUT_WAITING_STRING = "Waiting...",
                         LEFT_HANDED_BINDINGS = "{ \"bindings\":[{\"action\":\"Player/Movement\",\"id\":\"d6b083a3-598f-488c-b426-dc2c99d2467a\",\"path\":\"<Keyboard>/i\",\"interactions\":\"\",\"processors\":\"\"},{ \"action\":\"Player/Movement\",\"id\":\"268b32de-857b-4bd1-aef9-93b31427fe2b\",\"path\":\"<Keyboard>/k\",\"interactions\":\"\",\"processors\":\"\"},{ \"action\":\"Player/Movement\",\"id\":\"4429fc0f-adef-4953-8b93-931dae4d3de2\",\"path\":\"<Keyboard>/j\",\"interactions\":\"\",\"processors\":\"\"},{ \"action\":\"Player/Movement\",\"id\":\"66c152b4-853b-4e9b-a7e3-4d2c4de7dfea\",\"path\":\"<Keyboard>/l\",\"interactions\":\"\",\"processors\":\"\"},{ \"action\":\"Player/Long Jump\",\"id\":\"838d837e-8dc0-4e8d-894a-b4d7ceb61f3d\",\"path\":\"<Keyboard>/slash\",\"interactions\":\"\",\"processors\":\"\"},{ \"action\":\"Player/High Jump\",\"id\":\"8d1fa3b9-f6e2-489e-9da1-39900167c32a\",\"path\":\"<Keyboard>/u\",\"interactions\":\"\",\"processors\":\"\"},{ \"action\":\"Player/Dash\",\"id\":\"22f63252-0ddf-4687-9ba0-3537427c0790\",\"path\":\"<Keyboard>/rightShift\",\"interactions\":\"\",\"processors\":\"\"},{ \"action\":\"Player/Hover\",\"id\":\"5c5e8e8f-3a00-4af0-a4b9-473e83b3fe4b\",\"path\":\"<Keyboard>/n\",\"interactions\":\"\",\"processors\":\"\"}]}";

    // Need to do these weird variable declarations so that the headers look good in editor

    [Header("Default values"), SerializeField]
    private float defaultXSens;

    [SerializeField]
    private float defaultYSens, defaultVolume;

    [Header("Panels"), SerializeField]
    private GameObject cameraPanel;

    [SerializeField]
    private GameObject soundPanel, keyboardPanel, controllerPanel;

    [Header("Buttons"), SerializeField]
    private GameObject cameraButton;

    [SerializeField]
    private GameObject soundButton, keyboardButton, controllerButton;

    [Header("Sliders"), SerializeField]
    private Slider xSensitivitySlider;

    [SerializeField]
    private Slider ySensitivitySlider, volumeSlider;

    [Header("Keyboard Button Texts"), SerializeField]
    private TMP_Text forwardsRebindButtonText;

    [SerializeField]
    private TMP_Text backwardsRebindButtonText, leftRebindButtonText, rightRebindButtonText, jumpRebindButtonText,
                     longJumpRebindButtonText, highJumpRebindButtonText, dashRebindButtonText, hoverRebindButtonText;

    [Header("Controller Button Texts"), SerializeField]
    private TMP_Text controllerJumpRebindButtonText;

    [SerializeField]
    private TMP_Text controllerLJRebindButtonText, controllerHJRebindButtonText, controllerDashRebindButtonText, controllerHoverRebindButtonText;

    private PlayerControls playerInput;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private AudioManager am;

    private float[] settings = new float[NUM_FLOAT_SETTINGS];

    private void Awake()
    {
        am = FindObjectOfType<AudioManager>();

        // Have only mouse panel active
        openCameraPanel(false);

        // Load settings
        settings[(int)SETTINGS_TYPE.X_SENSITIVITY] = PlayerPrefs.GetFloat(X_SENS_KEY, defaultXSens);
        settings[(int)SETTINGS_TYPE.Y_SENSITIVITY] = PlayerPrefs.GetFloat(Y_SENS_KEY, defaultYSens);
        settings[(int)SETTINGS_TYPE.VOLUME] = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);

        // Set sliders
        xSensitivitySlider.value = settings[(int)SETTINGS_TYPE.X_SENSITIVITY];
        ySensitivitySlider.value = settings[(int)SETTINGS_TYPE.Y_SENSITIVITY];
        volumeSlider.value = settings[(int)SETTINGS_TYPE.VOLUME];
    }

    private void Start()
    {
        // This all needs to be in Start and not Awake because player must be loaded

        playerInput = FindObjectOfType<PlayerScript>().getInput();

        // Load controls
        string rebinds = PlayerPrefs.GetString(CONTROLS_KEY, string.Empty);

        // Only apply if rebinds exist
        if (!string.IsNullOrEmpty(rebinds))
        {
            playerInput.LoadBindingOverridesFromJson(rebinds);
        }

        setButtonTexts();
    }

    public void saveSettings()
    {
        // Save to array
        settings[(int)SETTINGS_TYPE.X_SENSITIVITY] = xSensitivitySlider.value;
        settings[(int)SETTINGS_TYPE.Y_SENSITIVITY] = ySensitivitySlider.value;
        settings[(int)SETTINGS_TYPE.VOLUME] = volumeSlider.value;

        // Save to playerprefs
        PlayerPrefs.SetFloat(X_SENS_KEY, settings[(int)SETTINGS_TYPE.X_SENSITIVITY]);
        PlayerPrefs.SetFloat(Y_SENS_KEY, settings[(int)SETTINGS_TYPE.Y_SENSITIVITY]);
        PlayerPrefs.SetFloat(VOLUME_KEY, settings[(int)SETTINGS_TYPE.VOLUME]);
        PlayerPrefs.SetString(CONTROLS_KEY, playerInput.SaveBindingOverridesAsJson());

        // Write to disk
        PlayerPrefs.Save();
    }

    // Used by other scripts to get setting
    public float getSetting(SETTINGS_TYPE setting)
    {
        return settings[(int)setting];
    }

    // Used by reset button
    public void resetToDefaultSettings()
    {
        // Set all settings to default
        settings[(int)SETTINGS_TYPE.X_SENSITIVITY] = defaultXSens;
        settings[(int)SETTINGS_TYPE.Y_SENSITIVITY] = defaultYSens;
        settings[(int)SETTINGS_TYPE.VOLUME] = defaultVolume;

        // Reset sliders
        xSensitivitySlider.value = settings[(int)SETTINGS_TYPE.X_SENSITIVITY];
        ySensitivitySlider.value = settings[(int)SETTINGS_TYPE.Y_SENSITIVITY];
        volumeSlider.value = settings[(int)SETTINGS_TYPE.VOLUME];

        // Reset to default controls
        playerInput.RemoveAllBindingOverrides();

        setButtonTexts();
    }

    // Actions taken when player exits settings
    private void OnDisable()
    {
        // Save settings
        saveSettings();
    }

    // Used by menu buttons
    public void openCameraPanel(bool playSound = true)
    {
        // Play select sound effect if audio manager loaded
        if(playSound)
        {
            am.play("MenuSelect");
        }

        // Set only desired panel active
        cameraPanel.SetActive(true);
        soundPanel.SetActive(false);
        keyboardPanel.SetActive(false);
        controllerPanel.SetActive(false);

        // Set selected button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(cameraButton);
    }

    // Used by menu buttons
    public void openSoundPanel()
    {
        // Set only desired panel active
        soundPanel.SetActive(true);
        cameraPanel.SetActive(false);
        keyboardPanel.SetActive(false);
        controllerPanel.SetActive(false);

        // Set selected button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(soundButton);
    }

    // Used by menu buttons
    public void openKeyboardPanel()
    {
        // Set only desired panel active
        cameraPanel.SetActive(false);
        soundPanel.SetActive(false);
        keyboardPanel.SetActive(true);
        controllerPanel.SetActive(false);

        // Set selected button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(keyboardButton);
    }

    // Used by menu buttons
    public void openControllerPanel()
    {
        // Set only desired panel active
        cameraPanel.SetActive(false);
        soundPanel.SetActive(false);
        keyboardPanel.SetActive(false);
        controllerPanel.SetActive(true);

        // Set selected button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controllerButton);
    }

    public void enableLeftHanded()
    {
        // Load left-handed bindings
        playerInput.LoadBindingOverridesFromJson(LEFT_HANDED_BINDINGS);

        setButtonTexts();
    }

    // Used to rebind keyboard buttons
    public void rebindKeyboard(string action)
    {
        // Must have keyboard connected to continue
        if (Keyboard.current == null) { return; }

        // Need to disable input to rebind
        playerInput.Player.Disable();

        // Convert string to enum
        ACTION toRebind = (ACTION)Enum.Parse(typeof(ACTION), action.ToUpper());

        // Rebind appropriate button
        switch (toRebind)
        {
            case ACTION.MOVE_FORWARD:
                // Set button text
                forwardsRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Movement.PerformInteractiveRebinding(FORWARD_INDEX).WithControlsHavingToMatchPath("<Keyboard>")
                                    .WithCancelingThrough("<Keyboard>/escape").WithBindingGroup("KBM").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.MOVE_BACKWARDS:
                // Set button text
                backwardsRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Movement.PerformInteractiveRebinding(BACKWARDS_INDEX).WithControlsHavingToMatchPath("<Keyboard>")
                                    .WithCancelingThrough("<Keyboard>/escape").WithBindingGroup("KBM").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.MOVE_LEFT:
                // Set button text
                leftRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Movement.PerformInteractiveRebinding(LEFT_INDEX).WithControlsHavingToMatchPath("<Keyboard>")
                                    .WithCancelingThrough("<Keyboard>/escape").WithBindingGroup("KBM").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.MOVE_RIGHT:
                // Set button text
                rightRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Movement.PerformInteractiveRebinding(RIGHT_INDEX).WithControlsHavingToMatchPath("<Keyboard>")
                                    .WithCancelingThrough("<Keyboard>/escape").WithBindingGroup("KBM").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.JUMP:
                // Set button text
                jumpRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Jump.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Keyboard>").WithCancelingThrough("<Keyboard>/escape")
                    .WithBindingGroup("KBM").OnComplete(operations => stopRebinding(toRebind)).OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.LONG_JUMP:
                // Set button text
                longJumpRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.LongJump.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Keyboard>").WithCancelingThrough("<Keyboard>/escape")
                    .WithBindingGroup("KBM").OnComplete(operations => stopRebinding(toRebind)).OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.HIGH_JUMP:
                // Set button text
                highJumpRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.HighJump.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Keyboard>").WithCancelingThrough("<Keyboard>/escape")
                    .WithBindingGroup("KBM").OnComplete(operations => stopRebinding(toRebind)).OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.DASH:
                // Set button text
                dashRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Dash.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Keyboard>").WithCancelingThrough("<Keyboard>/escape")
                    .WithBindingGroup("KBM").OnComplete(operations => stopRebinding(toRebind)).OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.HOVER:
                // Set button text
                hoverRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Hover.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Keyboard>").WithCancelingThrough("<Keyboard>/escape")
                    .WithBindingGroup("KBM").OnComplete(operations => stopRebinding(toRebind)).OnCancel(operation => stopRebinding(toRebind)).Start();
                break;
        }
    }

    // Used to rebind controller buttons
    public void rebindController(string action)
    {
        // Must have controller connected to continue
        if(Gamepad.current == null) { return; }

        // Need to disable input to rebind
        playerInput.Player.Disable();

        // Convert string to enum
        ACTION toRebind = (ACTION)Enum.Parse(typeof(ACTION), action.ToUpper());

        // Rebind appropriate button
        switch (toRebind)
        {
            case ACTION.JUMP:
                // Set button text
                controllerJumpRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Jump.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Gamepad>").WithCancelingThrough("<Gamepad>/start")
                                    .WithBindingGroup("Controller").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.LONG_JUMP:
                // Set button text
                controllerLJRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.LongJump.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Gamepad>").WithCancelingThrough("<Gamepad>/start")
                                    .WithBindingGroup("Controller").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.HIGH_JUMP:
                // Set button text
                controllerHJRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.HighJump.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Gamepad>").WithCancelingThrough("<Gamepad>/start")
                                    .WithBindingGroup("Controller").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.DASH:
                // Set button text
                controllerDashRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Dash.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Gamepad>").WithCancelingThrough("<Gamepad>/start")
                                    .WithBindingGroup("Controller").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;

            case ACTION.HOVER:
                // Set button text
                controllerHoverRebindButtonText.text = INPUT_WAITING_STRING;

                // Start rebinding
                rebindingOperation = playerInput.Player.Hover.PerformInteractiveRebinding().WithControlsHavingToMatchPath("<Gamepad>").WithCancelingThrough("<Gamepad>/start")
                                    .WithBindingGroup("Controller").OnComplete(operation => stopRebinding(toRebind))
                                    .OnCancel(operation => stopRebinding(toRebind)).Start();
                break;
        }
    }

    private void stopRebinding(ACTION reboundAction)
    {
        // Need to dispose of rebinding to avoid error
        rebindingOperation.Dispose();

        setButtonTexts();

        // Re-enable input
        playerInput.Player.Enable();
    }

    private void setButtonTexts()
    {
        // Set keyboard button texts
        forwardsRebindButtonText.text = playerInput.Player.Movement.bindings[FORWARD_INDEX].ToDisplayString();
        backwardsRebindButtonText.text = playerInput.Player.Movement.bindings[BACKWARDS_INDEX].ToDisplayString();
        leftRebindButtonText.text = playerInput.Player.Movement.bindings[LEFT_INDEX].ToDisplayString();
        rightRebindButtonText.text = playerInput.Player.Movement.bindings[RIGHT_INDEX].ToDisplayString();
        jumpRebindButtonText.text = playerInput.Player.Jump.GetBindingDisplayString(InputBinding.MaskByGroup("KBM"));
        longJumpRebindButtonText.text = playerInput.Player.LongJump.GetBindingDisplayString(InputBinding.MaskByGroup("KBM"));
        highJumpRebindButtonText.text = playerInput.Player.HighJump.GetBindingDisplayString(InputBinding.MaskByGroup("KBM"));
        dashRebindButtonText.text = playerInput.Player.Dash.GetBindingDisplayString(InputBinding.MaskByGroup("KBM"));
        hoverRebindButtonText.text = playerInput.Player.Hover.GetBindingDisplayString(InputBinding.MaskByGroup("KBM"));

        // Set controller button texts
        controllerJumpRebindButtonText.text = playerInput.Player.Jump.GetBindingDisplayString(InputBinding.MaskByGroup("Controller"));
        controllerLJRebindButtonText.text = playerInput.Player.LongJump.GetBindingDisplayString(InputBinding.MaskByGroup("Controller"));
        controllerHJRebindButtonText.text = playerInput.Player.HighJump.GetBindingDisplayString(InputBinding.MaskByGroup("Controller"));
        controllerDashRebindButtonText.text = playerInput.Player.Dash.GetBindingDisplayString(InputBinding.MaskByGroup("Controller"));
        controllerHoverRebindButtonText.text = playerInput.Player.Hover.GetBindingDisplayString(InputBinding.MaskByGroup("Controller"));
    }
}