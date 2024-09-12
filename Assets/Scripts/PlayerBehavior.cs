using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehavior : MonoBehaviour
{
    private Rigidbody rb;

    private float radius = 0.5f;

    // Action tracking
    private bool hasJumped = false;
    private bool hasSwiped = false;

    private AudioSource audioSource;

    [Header("Sound Effects")]
    public AudioClip jumpSFX;
    public AudioClip pickUpSFX;
    public AudioClip speedBoostSFX;
    public AudioClip shieldSFX;
    public AudioClip magnetSFX;

    private float sfxVolume;

    [Header("Movement")]
    [Tooltip("How fast the ball moves left/right")]
    public float dodgeSpeed = 5;

    [Tooltip("How fast the ball moves forwards automatically")]
    public float rollSpeed = 5;

    public enum MobileHorizMovement
    {
        Accelerometer,
        ScreenTouch
    }

    public enum PowerUpType
    {
        SpeedBoost,
        ShieldBoost,
        Magnet
    }

    [Header("Power-Up Properties")]
    public float speedBoostMultiplier = 2f;
    public float shieldDuration = 5f;
    public float magnetDuration = 5f;

    private bool hasSpeedBoost = false;
    public bool hasShield = false;
    private bool hasMagnet = false;

    private float speedBoostEndTime;
    private float shieldEndTime;
    private float magnetEndTime;

    public MobileHorizMovement horizMovement = MobileHorizMovement.Accelerometer;

    [Header("Swipe Properties")]
    [Tooltip("How far will the player move upon swiping")]
    public float swipeMove = 2f;

    [Tooltip("How far must the player swipe before we will execute the action(in inches)")]
    public float minSwipeDistance = 0.25f;

    // Used to hold the value that converts minSwipeDistance to pixels
    private float minSwipeDistancePixels;

    // Stores the starting point of mobile touch events
    private Vector2 touchStart;

    [Tooltip("Height player can jump")]
    public float jumpForce = 10f;

    [Tooltip("Distance player should stop jumping")]
    public float jumpDistance;

    // Sphere collider reference for the player.
    private SphereCollider col;

    [Tooltip("What is considered to be Ground.")]
    [SerializeField] private LayerMask whatIsGround;

    [Tooltip("What is considered to be an Obstacle.")]
    [SerializeField] private LayerMask whatIsObstacle;

    // Boolean that check whether player is on the ground.
    private bool isGrounded;

    // Boolean that check whether there is an obstacle in front of the player.
    private bool hasObstacle;

    [Header("UI References")]
    public TMP_Text instructionsText; // The instructions UI text
    public TMP_Text scoreText; // Changed to TMP_Text
    public float score = 0f;

    public float Score
    {
        get { return score; }
        set
        {
            score = value;
            if (scoreText == null)
            {
                Debug.LogError("Score Text is not set. Please assign it in the Inspector.");
                return;
            }
            scoreText.text = $"{score:0}";
        }
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("Missing AudioSource component on Player.");
        }

        sfxVolume = AudioPreferences.GetSFXVolume();

        // Update the SFX volume of the audio source
        audioSource.volume = sfxVolume;

        col = GetComponent<SphereCollider>();

        minSwipeDistancePixels = minSwipeDistance * Screen.dpi;

        Score = 0;
        // Display instructions at the start
        if (instructionsText != null)
        {
            instructionsText.text = "Collect as many points as you can!\nTap the bottom of the screen to jump.\nSwipe left or right to move sideways.";
        }
    }

    private void Update()
    {
        sfxVolume = AudioPreferences.GetSFXVolume();
        audioSource.volume = sfxVolume;

        // Check Properties
        // Check if the player is on the ground.
        isGrounded = Physics.CheckSphere(transform.position, col.radius + 0.05f, whatIsGround);

        // Check if there is an obstacle in front of the player.
        hasObstacle = Physics.Raycast(transform.position, Vector3.forward, jumpDistance, whatIsObstacle, QueryTriggerInteraction.Ignore);

       
        // Check if we are running on a mobile device
#if UNITY_IOS || UNITY_ANDROID
            // Check if Input has registered more than zero touches
            if (Input.touchCount > 0)
            {
              // Store the first touch detected
                Touch touch = Input.touches[0];

                SwipeTeleport(touch);
                Jump(touch);
            }
#endif

        // Check if power-ups have expired
        CheckPowerUps();

        // Attract collectibles if magnet is active
        if (hasMagnet)
        {
            AttractCollectibles();
        }

        if (instructionsText.gameObject.activeSelf)
        {
            StartCoroutine(FadeOutText(5f)); // Fade out over 5 seconds
        }
    }

    private void FixedUpdate()
    {
        if (PauseSceneController.paused)
        {
            return;
        }
        Score += Time.deltaTime;
        // Check if we're moving to the side
        var horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;

        // Apply speed boost if active
        float currentRollSpeed = hasSpeedBoost ? rollSpeed * speedBoostMultiplier : rollSpeed;

        // Do not always add force
        if (rb.velocity.magnitude < currentRollSpeed)
        {
            rb.AddForce(0, 0, currentRollSpeed);
        }

        rb.AddForce(horizontalSpeed, 0, 0);
        if ( instructionsText.gameObject.activeSelf)
        {
            StartCoroutine(FadeOutText(5f)); // Fade out over 5 seconds
        }

    }

   
    // Will teleport the player if swiped to the left or right
    private void SwipeTeleport(Touch touch)
    {
        // Check if the touch just strated
        if (touch.phase == TouchPhase.Began)
        {
            // If so, set touchStart
            touchStart = touch.position;
        }
        // If the touch has ended
        else if (touch.phase == TouchPhase.Ended)
        {
            // Get the position the touch ended at
            Vector2 touchEnd = touch.position;

            // Calculate the difference betweeen the beginning and end of the touch on the x axis
            float x = touchEnd.x - touchStart.x;

            // If we are not moving far enough, don't do the teleport
            if (Mathf.Abs(x) < minSwipeDistancePixels)
            {
                return;
            }

            Vector3 moveDirection;

            // If moved negatively in the x axis, move left
            if (x < 0)
            {
                moveDirection = Vector3.left;
            }
            else
            {
                // Otherwise we're on the right
                moveDirection = Vector3.right;
            }

            RaycastHit hit;

            // Only move if we wouldn't hit something
            if (!rb.SweepTest(moveDirection, out hit, swipeMove))
            {
                // Move the player
                rb.MovePosition(rb.position + (moveDirection * swipeMove));
            }
        }
    }

    // Player will jump when we touch the specific area at the bottom of the screen
    private void Jump(Touch touch)
    {
        // Check if the player touches the specific area at the bottom of the screen.
        if (touch.phase == TouchPhase.Began && isGrounded && !hasObstacle)
        {
            // Convert the screen position to viewport position.
            Vector2 touchPos = Camera.main.ScreenToViewportPoint(touch.position);

            // Define the specific area at the bottom of the screen (example: bottom 25% of the screen)
            if (touchPos.y < 0.25f)
            {
                // Apply the jump force
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                // Play jump sound effect
                PlaySFX(jumpSFX);
            }
        }
    }

    public void PickUp()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hitCollider in hitColliders)
        {
            Vector3 HitPosition = hitCollider.transform.position;
            HitPosition.y = hitCollider.transform.position.y;

            Vector3 Direction = HitPosition - transform.position;
            if (Vector3.Dot(transform.forward, Direction.normalized) > 3f)
            {
                PowerUpType powerUp = hitCollider.GetComponent<PowerUp>().type;
                ApplyPowerUp(powerUp);
                // Play pick up sound effect
                PlaySFX(pickUpSFX);
                hitCollider.SendMessage("Operate", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void ApplyPowerUp(PowerUpType powerUp)
    {
        switch (powerUp)
        {
            case PowerUpType.SpeedBoost:
                ActivateSpeedBoost();
                break;
            case PowerUpType.ShieldBoost:
                ActivateShieldBoost();
                break;
            case PowerUpType.Magnet:
                ActivateMagnet();
                break;
        }
    }

    private void ActivateSpeedBoost()
    {
        hasSpeedBoost = true;
        speedBoostEndTime = Time.time + 5f; // Duration of the speed boost

        // Play speed boost sound effect
        PlaySFX(speedBoostSFX);
    }

    private void ActivateShieldBoost()
    {
        hasShield = true;
        shieldEndTime = Time.time + shieldDuration;
    }

    private void ActivateMagnet()
    {
        hasMagnet = true;
        magnetEndTime = Time.time + magnetDuration;

        // Play magnet sound effect
        PlaySFX(magnetSFX);
    }

    private void CheckPowerUps()
    {
        if (hasSpeedBoost && Time.time > speedBoostEndTime)
        {
            hasSpeedBoost = false;
        }

        if (hasShield && Time.time > shieldEndTime)
        {
            hasShield = false;
        }

        if (hasMagnet && Time.time > magnetEndTime)
        {
            hasMagnet = false;
        }
    }

    public void ShieldPower()
    {
        if (hasShield)
        {
            // Prevent the player from dying or taking damage
            Debug.Log("Shield active: Player is safe!");
            //hasShield = false;

        }
        else
        {
            // Handle the player dying logic
            Destroy(gameObject);
        }
    }

    private void AttractCollectibles()
    {
        // Find all collectible objects within a certain radius
        Collider[] collectibles = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Collectibles"));

        foreach (Collider collectible in collectibles)
        {
            // Move collectible towards the player
            Vector3 direction = (transform.position - collectible.transform.position).normalized;
            collectible.transform.position += direction * Time.deltaTime * 7f; // Adjust speed as needed
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
        }
    }
    private IEnumerator FadeOutText(float fadeDuration)
    {
        float elapsedTime = 0f;
        Color originalColor = instructionsText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            instructionsText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // After fading out, deactivate the text
        instructionsText.gameObject.SetActive(false);
    }
}
