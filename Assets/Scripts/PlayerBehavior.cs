using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    private Rigidbody rb;

    private float radius = 0.5f;

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
    private bool hasShield = false;
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
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        col = GetComponent<SphereCollider>();

        minSwipeDistancePixels = minSwipeDistance * Screen.dpi;
    }

    private void Update()
    {
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
    }

    private void FixedUpdate()
    {
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

        // Check if power-ups have expired
        CheckPowerUps();

        // Attract collectibles if magnet is active
        if (hasMagnet)
        {
            AttractCollectibles();
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
            if (Vector3.Dot(transform.forward, Direction.normalized) > 0.5f)
            {
                PowerUpType powerUp = hitCollider.GetComponent<PowerUp>().type;
                ApplyPowerUp(powerUp);
                hitCollider.SendMessage("Operate", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void ApplyPowerUp(PowerUpType powerUp)
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

    private void OnCollisionEnter(Collision collision)
    {
        if (hasShield && collision.gameObject.CompareTag("Obstacle"))
        {
            // Prevent damage or interaction with the obstacle
            // Example: Destroy obstacle or ignore collision
            Destroy(collision.gameObject);
        }
    }

    private void AttractCollectibles()
    {
        // Find all collectible objects within a certain radius
        Collider[] collectibles = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Collectible"));

        foreach (Collider collectible in collectibles)
        {
            // Move collectible towards the player
            Vector3 direction = (transform.position - collectible.transform.position).normalized;
            collectible.transform.position += direction * Time.deltaTime * 5f; // Adjust speed as needed
        }
    }
}
