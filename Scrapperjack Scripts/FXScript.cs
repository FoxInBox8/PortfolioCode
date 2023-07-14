using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FXScript : MonoBehaviour
{
    [SerializeField] 
    private Transform feetPoint, nozzlePoint, jetpackBodyPoint;

    [SerializeField]
    private float lightTurnOffTime;
    
    [Header("Light Points"), SerializeField] 
    private Light nozzleLight;

    [SerializeField] 
    private Light bigBulb, smallBulbA, smallBulbB;
    
    [Header("Effect Prefabs"), SerializeField] 
    private GameObject normNozzleSmokeBurst;

    // Add actual particle prefabs here
    [SerializeField] 
    private GameObject normNozzleFlameBurst, largeNozzleSmokeBurst, largeNozzleFlameBurst, dashNozzleFlameBurst, nozzleSmokeSustain, nozzleFlameSustain, 
                       chargeNozzleSmokeSustain, nozzleSmokeTrailSustain, lowBodySmokeSustain, highBodySmokeSustain, repairShine, speedTrail, explosion;

    [SerializeField]
    private float medSmokePoint, lowSmokePoint;

    // Use for instantiated effects that need to be manually destroyed due to the duration of the effect not being fixed
    [HideInInspector]
    public GameObject tempNozzleSmoke, tempNozzleFlame, tempChargeSmoke, tempBodyDamageSmokeLow, tempBodyDamageSmokeHigh;

    [HideInInspector]
    public bool groundReset = false;
    private bool isJetpackPlaying = false, isHoverPlaying = false, landSFX = false;
    public string jumping = null;

    private Image bigBulbImage, smallBulbAImage, smallBulbBImage;
    private PlayerScript player;
    private AudioManager am;
    private void Start()
    {
        bigBulbImage = GameObject.Find("LightBulb3").GetComponent<Image>();
        smallBulbAImage = GameObject.Find("LightBulb2").GetComponent<Image>();
        smallBulbBImage = GameObject.Find("LightBulb1").GetComponent<Image>();
        player = FindObjectOfType<PlayerScript>();
        am = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        // If health is between mid and low health, use sustained low smoke
        if (player.currentHealth <= medSmokePoint && player.currentHealth > lowSmokePoint && tempBodyDamageSmokeLow == null)
        {
            if (tempBodyDamageSmokeHigh)
            {
                Destroy(tempBodyDamageSmokeHigh);
            }

            tempBodyDamageSmokeLow = Instantiate(lowBodySmokeSustain, jetpackBodyPoint);
        }

        // If health is below low point, use high sustained smoke
        else if(player.currentHealth <= lowSmokePoint && tempBodyDamageSmokeHigh == null)
        {
            if(tempBodyDamageSmokeLow)
            {
                Destroy(tempBodyDamageSmokeLow);
            }

            tempBodyDamageSmokeHigh = Instantiate(highBodySmokeSustain, jetpackBodyPoint);
        }

        // No smoke effects above med point
        else if(player.currentHealth > medSmokePoint)
        {
            if (tempBodyDamageSmokeLow)
            {
                Destroy(tempBodyDamageSmokeLow);
            }

            if (tempBodyDamageSmokeHigh)
            {
                Destroy(tempBodyDamageSmokeHigh);
            }
        }
    }

    public void AirJump()
    {
        jumping = "Air Jumps";
        // Set to air jump brightness
        nozzleLight.intensity = 1;
        
        // Spawn particles
        Instantiate(normNozzleSmokeBurst, nozzlePoint);
        Instantiate(normNozzleFlameBurst, nozzlePoint);
        JetpackSFXStart();
        // Turn off lights
        StartCoroutine(NozzleLightOff());
        
        // Turn off corresponding light
        if(smallBulbB.intensity != 0)
        {
            smallBulbB.intensity = 0;
            smallBulbBImage.enabled = false;
        }

        else
        {
            smallBulbA.intensity = 0;
            smallBulbAImage.enabled = false;
        }
    }

    public void FinalAirJump()
    {
        jumping = "Final Air Jump";
        // Set to final air jump brightness
        nozzleLight.intensity = 2;

        // Spawn particles
        Instantiate(largeNozzleSmokeBurst, nozzlePoint);
        Instantiate(largeNozzleFlameBurst, nozzlePoint);
        tempNozzleSmoke = Instantiate(nozzleSmokeSustain, nozzlePoint);
        JetpackSFXStart();
        // Turn light off in future
        StartCoroutine(NozzleLightOff());

        // Turn off big light
        bigBulb.intensity = 0;
        bigBulbImage.enabled = false;
    }

    public void GroundDash()
    {
        jumping = "Ground Dash";
        // Set to ground dash brightness
        nozzleLight.intensity = 1;

        // Spawn particles
        Instantiate(speedTrail, jetpackBodyPoint);
        Instantiate(dashNozzleFlameBurst, nozzlePoint);
        JetpackSFXStart();
        // Turn light off in future
        StartCoroutine(NozzleLightOff());
       
    }

    public void AirDash()
    {
        jumping = "Air Dash";

        // Set to air dash brightness
        nozzleLight.intensity = 1;
        
        // Spawn particles
        Instantiate(speedTrail, jetpackBodyPoint);
        Instantiate(dashNozzleFlameBurst, nozzlePoint);
        tempNozzleSmoke = Instantiate(nozzleSmokeTrailSustain, nozzlePoint);
        JetpackSFXStart();

        // Turn particles off in future
        StartCoroutine(NozzleLightOff());
        Destroy(tempNozzleSmoke, lightTurnOffTime);
    }

    public void LongJump()
    {
        jumping = "Long";
        // Set to long jump brightness
        nozzleLight.intensity = 2;
        // Spawn particles
        Instantiate(normNozzleSmokeBurst, nozzlePoint);
        Instantiate(largeNozzleFlameBurst, nozzlePoint);
        tempNozzleSmoke = Instantiate(nozzleSmokeSustain, nozzlePoint);
        HoverSFXStart();

        // Turn lights off in future
        StartCoroutine(NozzleLightOff());
    }

    public void ChargeHighJump()
    {
        if(tempChargeSmoke == null)
        {
            tempChargeSmoke = Instantiate(chargeNozzleSmokeSustain, nozzlePoint);
            am.play("Scrapper_Charging");
          
        }
    }

    // Call upon prematurely cancelling a charge jump
    public void CancelCharge()
    {
        Destroy(tempChargeSmoke);
        am.stop("Scrapper_Charging");
     
    }

    public void ActivateHighJump()
    {
        jumping = "High";
        // Set light to high jump brightness
        nozzleLight.intensity = 2;
        JetpackSFXStart();
        // Spawn particles
        Instantiate(largeNozzleSmokeBurst, nozzlePoint);
        Instantiate(nozzleSmokeTrailSustain, nozzlePoint);
        tempNozzleFlame = Instantiate(nozzleFlameSustain, nozzlePoint);

        // Turn flame off in future
        StartCoroutine(ChargeJumpFlameOff());

        // Turn off appropriate light
        if (smallBulbB.intensity != 0)
        {
            smallBulbB.intensity = 0;
            smallBulbBImage.enabled = false;
        }

        else if (smallBulbA.intensity != 0)
        {
            smallBulbA.intensity = 0;
            smallBulbAImage.enabled = false;
        }

        else
        {
            bigBulb.intensity = 0;
            bigBulbImage.enabled = false;
        }
    }

    // Call upon starting to hover
    public void InitiateHover()
    {
        jumping = "Hover";
        nozzleLight.intensity = 1;
        HoverSFXStart();
        if(tempNozzleFlame == null)
        {
            tempNozzleFlame = Instantiate(nozzleFlameSustain, nozzlePoint);
        }
    }

    // Call upon ending a hover
    public void EndHover()
    {
        nozzleLight.intensity = 0;
        

        if (tempNozzleFlame != null)
        {
            Destroy(tempNozzleFlame);
            HoverSFXStop();
        }
    }

    public void Repair()
    {
        Instantiate(repairShine, jetpackBodyPoint);
    }
    
    public void Explode()
    {
        jumping = "Explosion";
        nozzleLight.intensity = 5;
        JetpackSFXStart();
        StartCoroutine(NozzleLightOff());
        Instantiate(explosion, jetpackBodyPoint);
        am.play("Scrapper_Charge_Jump_Launch");
        // Disable lights
        bigBulb.intensity = smallBulbA.intensity = smallBulbB.intensity = 0;
        bigBulbImage.enabled = smallBulbAImage.enabled = smallBulbBImage.enabled = false;
    }

    public void ResetLights()
    {
        // Set all lights to normal brightness
        if(bigBulb.intensity == 0)
        {
            bigBulb.intensity = 1.5f;
            bigBulbImage.enabled = true;
        }

        if(smallBulbA.intensity == 0)
        {
            smallBulbA.intensity = 1;
            smallBulbAImage.enabled = true;
        }

        if(smallBulbB.intensity == 0)
        {
            smallBulbB.intensity = 1;
            smallBulbBImage.enabled = true;
        }
    }

    private IEnumerator NozzleLightOff()
    {
        // Wait for set time
        yield return new WaitForSeconds(lightTurnOffTime);

        nozzleLight.intensity = 0;
        JetpackSFXStop();
        HoverSFXStop();
    }

    private IEnumerator ChargeJumpFlameOff()
    {
        // Wait for jump to be over
        yield return new WaitForSeconds(1f);

        nozzleLight.intensity = 0;

        if(tempNozzleFlame != null)
        {
            Destroy(tempNozzleFlame);
            
        }     
    }
    
    //Starts Playign the Burst Sound for the Jetpack
    private void JetpackSFXStart()
    {
        if (!isJetpackPlaying)
        {
            isJetpackPlaying = true;
            am.play("Burst");
            JumpingSFX();
            
        }
    }
    //Stops the Burst sound
    private void JetpackSFXStop()
    {
        if (isJetpackPlaying)
        {
            isJetpackPlaying = false;
            am.stop("Burst");
            
        }
    }

    //Starts Hovering Noise for the Jetpack
    private void HoverSFXStart()
    {
        if (!isHoverPlaying)
        {
            isHoverPlaying = true;
            am.play("Hover");
            JumpingSFX();
        }
    }
    //Stop Hovering sound
    private void HoverSFXStop()
    {
        if (isHoverPlaying)
        {
            isHoverPlaying = false;
            am.stop("Hover");
        }
       
    }

    //Controls Jumping sound effects
    public void JumpingSFX()
    {
        //Detects if the player is jumping
        if (jumping == "Jump")
        {
            am.play("Scrapper_Jump_Launch");
            am.play("Jump_Launch");
            jumping = "Land";
            landSFX = false;
            return;
        }

        //Detect if the player is using an ability 
        if (isJetpackPlaying || isHoverPlaying)
        {
            landSFX = false;
            if (jumping == "Explosion")
                return;
            if (jumping == "Ground Dash" || jumping == "Air Dash")
            {
                am.play("Dash");
                return;
            }
            if (jumping == "Air Jumps" || jumping == "Final Air Jump")
            {
                am.play("Scrapper_Midair");
                return;
            }
            if (jumping == "High")
            {
                am.play("Scrapper_Charge_Jump_Launch");
                if (player.isGrounded)
                {
                    am.play("Jump_Launch");
                }
                return;
            }
            if (jumping == "Long")
            {
                am.play("Scrapper_Long_Jump_Launch");
                am.play("Jump_Launch");
                return;
            }

        }

        //Detects if the player has landed from a jump
        if (player.isGrounded && !landSFX)
        {
            landSFX = true;
            if (jumping == "Ground Dash")
            {
                return;
            }
            am.play("Jump_Land");
            if (jumping != "High" && jumping != "Explosion" && jumping != "Long")
            {
                am.play("Scrapper_Jump_Land");
                return;
            }
            if (jumping == "Long" || jumping == "Explosion")
            {
                am.play("Scrapper_Long_Jump_Land");
                return;
            }
            if (jumping == "High")
            {
                am.play("Scrapper_Charge_Jump_Land");
                return;
            }
        }


      

    }

}