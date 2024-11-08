using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    private float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopehit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rigidBody;

    public MovementState state;
    public enum MovementState
    {
        walking,
        crouching,
        airborne
    }

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
        crouchYScale = transform.localScale.y / 2;
    }

    private void Update()
    {
        // Is on ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        PlayerInput();
        SpeedControl();
        StateHandler();

        // Handle drag
        if (grounded)
        {
            rigidBody.linearDamping = groundDrag;
        }
        else
        {
            rigidBody.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // When to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            if (grounded)
            {
                rigidBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
        }

        // Stop crouching
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if (grounded)
        {
            // Walking
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            // Airborne
            state = MovementState.airborne;
        }

        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
    }

    private void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // On slope
        if (OnSlope() && !exitingSlope)
        {
            rigidBody.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rigidBody.linearVelocity.y > 0)
            {
                rigidBody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        // On ground
        else if (grounded)
        {
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Turn gravity off while on slope
        rigidBody.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // Limit velocity on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rigidBody.linearVelocity.magnitude > moveSpeed)
            {
                rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * moveSpeed;
            }
        }
        // Limit velocity on ground or in air
        else
        {
            Vector3 maxVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);

            if (maxVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = maxVelocity.normalized * moveSpeed;
                rigidBody.linearVelocity = new Vector3(limitedVelocity.x, rigidBody.linearVelocity.y, limitedVelocity.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // Reset y velocity
        rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);

        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopehit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopehit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopehit.normal).normalized;
    }
}
