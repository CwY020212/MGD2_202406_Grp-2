using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    [Tooltip("How fast the ball moves left/right")]
    public float dodgeSpeed = 5;

    [Tooltip("How fast the ball moves forwards automatically")]
    [Range(0, 10)]
    public float rollSpeed = 5;

    [Header("Swipe Properties")]
    [Tooltip("How far will the player move upon swiping")]
    public float swipeMove = 2f;

    [Tooltip("How far must the player swipe before we will execute the action (in inches)")]
    public float minSwipeDistance = 0.25f;

    private float minSwipeDistancePixels;
    private Vector2 touchStart;

    public enum MobileHorizMovement
    {
        Accelerometer,
        ScreenTouch
    }
    public MobileHorizMovement horizMovement = MobileHorizMovement.Accelerometer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        minSwipeDistancePixels = minSwipeDistance * Screen.dpi;
    }

    private void FixedUpdate()
    {
        var horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR

        if (Input.GetMouseButton(0))
        {
            horizontalSpeed = CalculateMovement(Input.mousePosition);
        }

#elif UNITY_IOS || UNITY_ANDROID
        if (horizMovement == MobileHorizMovement.Accelerometer)
        {
            horizontalSpeed = Input.acceleration.x * dodgeSpeed;
        }
        if (Input.touchCount > 0)
        {
            if (horizMovement == MobileHorizMovement.ScreenTouch)
            {
                Touch touch = Input.touches[0];
                horizontalSpeed = CalculateMovement(touch.position);
            }
        }
#endif
        rb.AddForce(horizontalSpeed, 0, rollSpeed);
    }

    private void Update()
    {

#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            SwipeTeleport(touch);
        }
#endif
    }


    private float CalculateMovement(Vector3 pixelPos)
    {
        var worldPos = Camera.main.ScreenToViewportPoint(pixelPos);
        float xMove = 0;
        if (worldPos.x < 0.5f)
        {
            xMove = -1;
        }
        else
        {
            xMove = 1;
        }
        return xMove * dodgeSpeed;
    }

    private void SwipeTeleport(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            touchStart = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            Vector2 touchEnd = touch.position;
            float x = touchEnd.x - touchStart.x;
            if (Mathf.Abs(x) < minSwipeDistancePixels)
            {
                return;
            }
            Vector3 moveDirection;
            if (x < 0)
            {
                moveDirection = Vector3.left;
            }
            else
            {
                moveDirection = Vector3.right;
            }
            RaycastHit hit;
            if (!rb.SweepTest(moveDirection, out hit, swipeMove))
            {
                rb.MovePosition(rb.position + (moveDirection * swipeMove));
            }
        }
    }
    }
