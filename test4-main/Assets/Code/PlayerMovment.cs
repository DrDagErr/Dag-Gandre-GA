using System.Collections;
using System.Threading;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    Rigidbody rb;

    [Header("Movement")]
    public MovmentSate state;
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public bool isRunning = false;
    public float jumpForce;
    public float jumpCooldown;
    public bool dubbleJump;
    public bool canDoubleAfterRelease = false;
    public float airMultipiler;
    public float groundDrag;
    public float wallrunSpeed;
    public float grappleSpeed;

    [Header("Ground Check")]
    public float playerHeigt;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform looking;

    float horizontalInput;
    float verticalInput;

    [Header("Keybinds")]
    public KeyCode jumpkey = KeyCode.Space;
    public KeyCode springKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    bool jumpReady = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYscale;
    public float startYscale;

    [Header("Sliding")]
    public float slideStopSpeed;
    public float slideStartSpeed;
    public float slideForce;
    public float slideDrag;
    public float slideYscale;
    public float maxSlideTime;
    private float slideTimer;
    private bool sliding = false;

    [Header("Slope Movement")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitSlpoe;

    Vector3 moveDiraction;


    public enum MovmentSate
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air,
        wallrunning,
        grapplning,
    }

    public bool wallrunning;

    public bool grapplning;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYscale = transform.localScale.y;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeigt * 0.5f + 0.3f, whatIsGround);

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }

        MyInput();
        SpeedControl();
        StateHandler();

        if (Input.GetKeyUp(jumpkey))
        {
            canDoubleAfterRelease = true;
        }

        if (sliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                StopSlideOrCrouch();
            }
        }

        if (Input.GetKeyDown(respawnKey))
        {
            RespawnPlayer();
        }

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpkey) && jumpReady && grounded && !sliding)
        {
            jumpReady = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(jumpkey) && dubbleJump && canDoubleAfterRelease && state == MovmentSate.air && !wallrunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            dubbleJump = false;
        }

        if (Input.GetKeyDown(crouchKey))
        {
            if (moveSpeed >= slideStartSpeed && grounded)
            {
                StartSlide();
            }
            else
            {
                StartCrouch();
            }
        }

        if (Input.GetKeyUp(crouchKey))
        {
            StopSlideOrCrouch();
        }
    }

    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovmentSate.wallrunning;
            moveSpeed = wallrunSpeed;
            isRunning = false;
        }

        else if (sliding)
        {
            isRunning = false;
            state = MovmentSate.sliding;
            if (rb.velocity.magnitude <= slideStopSpeed || !Input.GetKey(crouchKey))
            {
                StopSlideOrCrouch();
            }
            return;
        }

        else if (grapplning)
        {
            state = MovmentSate.grapplning;
            grappleSpeed = 50f;
            isRunning = false;
        }

        else if (grounded && Input.GetKey(crouchKey))
        {
            state = MovmentSate.crouching;
            moveSpeed = crouchSpeed;
            isRunning = false;
        }
        else if (grounded && Input.GetKey(springKey) && !Input.GetKey(crouchKey))
        {
            isRunning = true;
            state = MovmentSate.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovmentSate.walking;
            moveSpeed = walkSpeed;
            isRunning = false;
        }
        else
        {
            state = MovmentSate.air;
            isRunning = false;
        }
    }

    private void MovePlayer()
    {
        if (grapplning)
        {
            return;
        }
        moveDiraction = looking.forward * verticalInput + looking.right * horizontalInput;

        if (sliding)
        {
            Vector3 slideDir;

            if (OnSlope())
            {
                slideDir = GetSlopeMoveDirection();
                rb.useGravity = false;

                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeBoost = Mathf.Lerp(1f, 2.5f, slopeAngle / maxSlopeAngle);

                Vector3 controlDir = (slideDir + moveDiraction.normalized * 0.25f).normalized;

                rb.AddForce(controlDir * slideForce * slopeBoost, ForceMode.Force);
                rb.AddForce(-slopeHit.normal * 60f, ForceMode.Force);
            }
            else
            {
                rb.useGravity = true;
                Vector3 controlDir = (rb.velocity.normalized + moveDiraction.normalized * 0.3f).normalized;
                rb.AddForce(controlDir * slideForce, ForceMode.Force);
            }

            return;
        }

        if (OnSlope() && !exitSlpoe)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDiraction.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDiraction.normalized * moveSpeed * 10f * airMultipiler, ForceMode.Force);
        }

        if (!wallrunning)
        {
            rb.useGravity = !OnSlope();
        }
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitSlpoe)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }

        }
        else
        {
            Vector3 flatvel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatvel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatvel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitSlpoe = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        if (!wallrunning)
        {
            dubbleJump = true;
        }
        else
        {
            dubbleJump = false;
        }
    }

    private void ResetJump()
    {
        jumpReady = true;
        exitSlpoe = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeigt * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDiraction, slopeHit.normal).normalized;
    }

    private void StartSlide()
    {
        sliding = true;
        slideTimer = maxSlideTime;
        state = MovmentSate.sliding;

        transform.localScale = new Vector3(transform.localScale.x, slideYscale, transform.localScale.z);

        if (!OnSlope() && !grapplning)
        {
            rb.AddForce(looking.forward * slideForce, ForceMode.Impulse);
        }

        rb.drag = slideDrag;
    }


    private void StopSlideOrCrouch()
    {
        sliding = false;

        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
        }

        rb.useGravity = true;
        rb.drag = groundDrag;
    }

    private void StartCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);
    }

    [Header("Respawn")]
    public Transform respawnPoint;
    public KeyCode respawnKey = KeyCode.R;

    private void RespawnPlayer()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = respawnPoint.position;

        transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
    }
}


