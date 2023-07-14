using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed, jumpForce, maxFallSpeed, gravity, drag, bhopWindow, bhopSpeedBoost, startHealth, cameraXSensitivity, cameraYSensitivity;

    [SerializeField]
    private Slider healthBar;

    private float cameraYaw = 0, cameraPitch = 0, yVelocity, currentHealth, bhopTimer;
    private bool prevFrameGrounded, canBhop;
    private Vector3 forceVector;

    private Transform mainCamera;
    private CharacterController controller;
    private PlayerControls playerInput;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        playerInput = new PlayerControls();

        playerInput.Game.Enable();

        currentHealth = healthBar.maxValue = startHealth;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {

    }

    private void Update()
    {
        updateCamera();
        updateMovement();
    }

    public PlayerControls getInputs()
    {
        return playerInput;
    }

    private void updateCamera()
    {
        Vector2 cameraInput = playerInput.Game.Camera.ReadValue<Vector2>();

        cameraYaw += cameraXSensitivity * cameraInput.x;
        cameraPitch -= cameraYSensitivity * cameraInput.y;

        // Clamp y axis to prevent camera from going too far
        cameraPitch = Mathf.Clamp(cameraPitch, -90, 90);

        // Player only rotates on x axis to prevent collision oddities
        transform.eulerAngles = new Vector3(0, cameraYaw, 0);
        mainCamera.eulerAngles = new(cameraPitch, cameraYaw, 0);
    }

    private void updateMovement()
    {
        prevFrameGrounded = controller.isGrounded;

        Vector2 inputVector = playerInput.Game.Movement.ReadValue<Vector2>();

        if (inputVector != Vector2.zero)
        {
            // Redirect movement vector to be in direction of transform
            Vector3 movementVector = transform.right * inputVector.x + transform.forward * inputVector.y;

            controller.Move(moveSpeed * Time.deltaTime * movementVector.normalized);
        }

        // We need to do this even when grounded to make sure isGrounded works correctly
        yVelocity -= gravity * Time.deltaTime;

        // Don't fall faster than max fall speed
        yVelocity = Mathf.Max(yVelocity, maxFallSpeed);

        controller.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);

        if(controller.isGrounded)
        {
            checkJumping();
        }

        // Apply forces if we have any
        if(forceVector !=  Vector3.zero)
        {
            controller.Move(forceVector * Time.deltaTime);

            // Apply drag
            forceVector = Vector3.Lerp(forceVector, Vector3.zero, drag * Time.deltaTime);
        }
    }

    private void checkJumping()
    {
        // If we just landed, allow the player to bhop
        if (!prevFrameGrounded)
        {
            canBhop = true;
        }

        if (playerInput.Game.Jump.WasPerformedThisFrame())
        {
            yVelocity = jumpForce;

            // If we can bhop, do so
            if (canBhop && bhopTimer <= bhopWindow)
            {
                forceVector += transform.forward * bhopSpeedBoost;
            }

            // If player does normal jump, reset bhopping
            else
            {
                bhopTimer = 0;
                canBhop = false;
            }
        }

        else
        {
            bhopTimer += Time.deltaTime;

            // Prevent y velocity from building up while grounded
            yVelocity = 0;
        }
    }

    public void dealDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;

        if(currentHealth <= 0)
        {
            SceneManager.LoadSceneAsync("IlantitleScene");
        }
    }

    public void heal(float health)
    {
        currentHealth += health;

        // Never heal above starting health
        currentHealth = Mathf.Min(currentHealth, startHealth);

        healthBar.value = currentHealth;
    }

    public void addForce(Vector3 force) { forceVector += force; }
}