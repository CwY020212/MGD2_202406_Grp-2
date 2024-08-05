using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    private Rigidbody rb;

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

    

        // Do not always addforce
        if (rb.velocity.magnitude < rollSpeed)
        {
            rb.AddForce(0, 0, rollSpeed);
        }

        rb.AddForce(horizontalSpeed, 0, 0);
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

   
}
