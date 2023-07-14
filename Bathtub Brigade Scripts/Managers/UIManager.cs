using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public enum CannonSide
    {
        LEFT = 0,
        RIGHT
    }

    private const int NUM_UPGRADE_TRACKERS = UpgradeScript.NUM_UPGRADE_LEVELS - 1, NUM_UPGRADE_COSTS = 2;

    public Slider healthBar, leftCannonBar, rightCannonBar, boostBar;

    [SerializeField]
    private Image compass, deathScreen, treasure;

    [SerializeField]
    private Sprite purchasedIcon;

    [SerializeField]
    private KeyCode upgradeKey;

    [SerializeField]
    private GameObject upgradePanel;

    private bool playerCanUpgrade = false, exitButtonPressed = false;

	private GameObject boostBarFill;
    private Image[,] upgradeIcons = new Image[UpgradeScript.NUM_UPGRADE_TYPES, NUM_UPGRADE_TRACKERS];

    private TMP_Text treasureText, upgradeTreasureText, upgradeMetalText, upgradeWoodText, upgradePlasticText;
    private TMP_Text[,] upgradeCosts = new TMP_Text[UpgradeScript.NUM_UPGRADE_TYPES, NUM_UPGRADE_COSTS];

    private PlayerScript playerScript;
    private PlayerMovement playerMovement;
    private DriverCannon leftCannon, rightCannon;
    private UpgradeScript upgradeManager;

    private void Start()
    {
        GameObject tempPlayer = GameObject.FindGameObjectWithTag("Player");

        playerScript = tempPlayer.GetComponent<PlayerScript>();
        playerMovement = tempPlayer.GetComponent<PlayerMovement>();

        // Set max values
        healthBar.maxValue = playerScript.currentHealth;
        leftCannonBar.maxValue = leftCannon.timer;
        rightCannonBar.maxValue = rightCannon.timer;
        boostBar.maxValue = playerMovement.boostStamina;

        // Disable Upgrade panel
        upgradePanel.SetActive(false);

        // Get boost bar image
        boostBarFill = boostBar.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject;

        // Get treasure text
        treasureText = treasure.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();

        // Get upgrade manager
        upgradeManager = FindObjectOfType<UpgradeScript>();

        // Save here for readability
        Transform topPanel = upgradePanel.transform.GetChild(0);

        // Get upgrade top panel texts
        upgradeTreasureText = topPanel.GetChild(1).gameObject.GetComponent<TMP_Text>();
        upgradeMetalText = topPanel.GetChild(3).gameObject.GetComponent<TMP_Text>();
        upgradeWoodText = topPanel.GetChild(5).gameObject.GetComponent<TMP_Text>();
        upgradePlasticText = topPanel.GetChild(7).gameObject.GetComponent<TMP_Text>();

        const int ICONS = 1, GOLD_TEXT = 3, MATERIAL_TEXT = 5;

        // Get upgrade type texts
        for(int i = 0; i < UpgradeScript.NUM_UPGRADE_TYPES; ++i)
        {
            upgradeCosts[i, 0] = upgradePanel.transform.GetChild(i + 1).GetChild(GOLD_TEXT).gameObject.GetComponent<TMP_Text>();
            upgradeCosts[i, 1] = upgradePanel.transform.GetChild(i + 1).GetChild(MATERIAL_TEXT).gameObject.GetComponent<TMP_Text>();
        }

        // Get upgrade icons
        for (int i = 0; i < UpgradeScript.NUM_UPGRADE_TYPES; ++i)
        {
            for(int j = 0; j < NUM_UPGRADE_TRACKERS; ++j)
            {
                upgradeIcons[i, j] = upgradePanel.transform.GetChild(i + 1).GetChild(ICONS).GetChild(2 - j).GetComponent<Image>();
            }
        }
    }

    private void Update()
    {
        // Update sliders
        healthBar.value = playerScript.currentHealth;
        leftCannonBar.value = leftCannon.timer;
        rightCannonBar.value = rightCannon.timer;
        boostBar.value = playerMovement.boostStamina;
        treasureText.SetText(playerScript.collectables[(int)Pickups.PickupType.TREASURE].ToString());

        // Change color of boost slider when recharging from empty
        boostBarFill.GetComponent<Image>().color = playerMovement.boostExaustion ? Color.red : Color.green;

        // Rotate compass with mouse
        compass.transform.localEulerAngles = new Vector3(0, 0, Camera.main.transform.eulerAngles.y);

        // Update upgrade top panel
        upgradeTreasureText.text = playerScript.collectables[(int)Pickups.PickupType.TREASURE].ToString();
        upgradeMetalText.text = playerScript.collectables[(int)Pickups.PickupType.METAL].ToString();
        upgradeWoodText.text = playerScript.collectables[(int)Pickups.PickupType.WOOD].ToString();
        upgradePlasticText.text = playerScript.collectables[(int)Pickups.PickupType.PLASTIC].ToString();

        // Update costs in upgrade panel
        for(int i = 0; i < UpgradeScript.NUM_UPGRADE_TYPES; ++i)
        {
            if(upgradeManager.upgradeList[i].currentLevel < UpgradeScript.NUM_UPGRADE_LEVELS - 1)
            {
                upgradeCosts[i, 0].text = upgradeManager.goldUpgradeCost[upgradeManager.upgradeList[i].currentLevel + 1].ToString();
                upgradeCosts[i, 1].text = upgradeManager.materialUpgradeCost[upgradeManager.upgradeList[i].currentLevel + 1].ToString();
            }

            else
            {
                upgradeCosts[i, 0].text = "";
                upgradeCosts[i, 1].text = "";
            }
        }

        // Update upgrade icons
        for(int i = 0; i < UpgradeScript.NUM_UPGRADE_TYPES; ++i)
        {
            for(int j = 0; j < upgradeManager.upgradeList[i].currentLevel; ++j)
            {
                upgradeIcons[i, j].sprite = purchasedIcon;
            }
        }

        // Toggle upgrade panel on key press
        if (playerCanUpgrade && (Input.GetKeyDown(upgradeKey) || exitButtonPressed))
        {
            upgradePanel.SetActive(!upgradePanel.activeSelf);

            // Toggle other UI
            compass.gameObject.SetActive(!compass.gameObject.activeSelf);
            treasure.gameObject.SetActive(!treasure.gameObject.activeSelf);

            // Toggle cursor
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;

            Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
        }

        // Reset button after closing upgrade panel
        exitButtonPressed = false;

        // Actions taken on death
        if (playerScript.currentHealth <= 0)
        {
            // Disable UI
            healthBar.gameObject.SetActive(false);
            leftCannonBar.gameObject.SetActive(false);
            rightCannonBar.gameObject.SetActive(false);
            boostBar.gameObject.SetActive(false);
            compass.gameObject.SetActive(false);
            treasure.gameObject.SetActive(false);

            // Enable death screen
            deathScreen.gameObject.SetActive(true);
        }
    }

    // Used by cannons to sync with UI
    public void registerCannon(DriverCannon cannon, CannonSide side)
    {
        if(side == CannonSide.LEFT)
        {
            leftCannon = cannon;
        }

        else
        {
            rightCannon = cannon;
        }
    }

    // Used by UI buttons
    public void upgrade(int type)
    {
        upgradeManager.upgrade((UpgradeScript.upgradeType)type);
    }

    // Used by docks
    public void togglePlayerUpgrade()
    {
        playerCanUpgrade = !playerCanUpgrade;
    }

    // Used by upgrade UI exit button
    public void exitButton()
    {
        exitButtonPressed = true;
    }
}