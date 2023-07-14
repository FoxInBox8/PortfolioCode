using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const int NUM_BUTTONS = 3;

    [SerializeField]
    private Button[] buttons = new Button[NUM_BUTTONS];

    [SerializeField]
    private RawImage titleText, background;

    [SerializeField]
    private float backgroundFadeTime, textFadeTime, buttonFadeTime;

    private float timer;
    private Color textColor, backgroundColor, buttonColor, buttonTextColor;
    private MenuManager menuManager;

    private void Start()
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();

        // Fade in the first time menu loads
        if (!menuManager.buttonsFadedIn) {
            timer = 0;

            // Set text colors
            // 1, 1, 1, 0 is transparent white
            textColor = buttonColor = new Color(1, 1, 1, 0);

            buttonTextColor = Color.clear;
            backgroundColor = Color.black;

            titleText.color = textColor;
            background.color = backgroundColor;

            // set button colors
            foreach (Button b in buttons) {
                b.image.color = buttonColor;
                b.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = buttonTextColor;
                b.enabled = false;
            }
        } else {
            // Set text colors
            textColor = buttonColor = backgroundColor = Color.white;
            buttonTextColor = Color.black;

            titleText.color = textColor;
            background.color = backgroundColor;

            // Set button colors
            foreach (Button b in buttons) {
                b.image.color = buttonColor;
                b.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = buttonTextColor;
                b.enabled = true;
            }
        }
    }

    private void Update()
    {
        // Fade in if we haven't already done so
        if (!menuManager.buttonsFadedIn) {
            timer += Time.deltaTime;

            // Fade in background
            float bgCol = Mathf.Lerp(0, 1, timer / backgroundFadeTime);

            backgroundColor.r = bgCol;
            backgroundColor.g = bgCol;
            backgroundColor.b = bgCol;
            background.color = backgroundColor;

            // Fade in text
            textColor.a = Mathf.Lerp(0, 1, (timer - backgroundFadeTime) / textFadeTime);
            titleText.color = textColor;

            // Fade in buttons
            float buttCol = Mathf.Lerp(0, 1, (timer - backgroundFadeTime - textFadeTime) / buttonFadeTime);

            buttonColor.a = buttCol;
            buttonTextColor.a = buttCol;

            foreach (Button b in buttons) {
                b.image.color = buttonColor;
                b.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = buttonTextColor;

                // Only enable when fully opaque
                if (buttonColor.a == 1) {
                    b.enabled = true;
                    menuManager.buttonsFadedIn = true;
                }
            }
        }
    }

    // Functions called by menu buttons
    public void exitButton() {
        Application.Quit();
    }

    public void changeScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}