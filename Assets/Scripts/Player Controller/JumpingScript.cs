using System.Collections;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class JumpingScript : MonoBehaviour
{
    public static event Action Jumping;
    public static event Action BulletJumping;

    [Header("Key Binding")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;

    [Header("Jumping Settings")]
    [SerializeField] float jumpDelay = 0.25f;
    [SerializeField] float firstJumpForce = 700;
    [SerializeField] float doubleJumpForce = 550f;
    [SerializeField] float bulletJumpForce = 600;
    [SerializeField] float maxJumps = 1;
    [SerializeField] bool bulletJump = false;

    bool firstJump = true;
    bool canDoAirJump = false;
    bool airJump = false;
    bool jumpKeyPressed;  
    int jumpCounter = 0;

    Rigidbody rb;


    void Awake() => rb = GetComponent<Rigidbody>();



    void OnEnable()
    {
        PlayerStateHandler.Landing += OnLanding;
        GrindScript.CancleGrinding += ResetJump;
        JumpingPadScript.OnJumpingPad += EnableAirJumps;
    }

    void OnDisable()
    {
        PlayerStateHandler.Landing -= OnLanding;
        GrindScript.CancleGrinding -= ResetJump;
        JumpingPadScript.OnJumpingPad -= EnableAirJumps;
    }

    void OnLanding()
    {
        firstJump = true;
        jumpCounter = 0;
        airJump = false;
        canDoAirJump = false;
    }

    void ResetJump()
    {
        firstJump = true;
        jumpCounter = 0;
        airJump = false;
        canDoAirJump = false;
        jumpKeyPressed = false;
    }

    void EnableAirJumps()
    {
        jumpCounter++;
        firstJump = false;
        airJump = true;
        canDoAirJump = true;
    }



    void Update()
    {
        jumpKeyPressed = Input.GetKey(jumpKey);

        if (Input.GetKeyUp(jumpKey))
            canDoAirJump = true;
    }

    void FixedUpdate()
    {
        if (IsJumpingDisabled()) return;

        if (jumpKeyPressed && IsAbleToJump())       Jump(firstJumpForce);
        if (jumpKeyPressed && IsAbleToAirJump())    Jump(doubleJumpForce);
        if (jumpKeyPressed && IsAbleToBulletJump()) BulletJump(bulletJumpForce);
        if (jumpKeyPressed && IsAbleToRailJump())   Jump(firstJumpForce);
    }



    void BulletJump(float jumpForce)
    {
        UpdateJumpState();
        BulletJumping?.Invoke();

        Vector3 jumpDirection = new Vector3(rb.velocity.x, 3f, rb.velocity.z);
        StartCoroutine(PerformJump(jumpDirection, jumpForce));
    }

    void Jump(float jumpForce)
    {
        UpdateJumpState();
        StartCoroutine(PerformJump(Vector2.up, jumpForce * 2.0f));
    }

    IEnumerator PerformJump(Vector3 jumpDirection, float jumpForce)
    {
        rb.AddForce(jumpDirection * jumpForce);

        ResetFallVelocity();

        yield return new WaitForSeconds(jumpDelay);
        airJump = true;
    }

    void UpdateJumpState()
    {
        Jumping?.Invoke();
        jumpCounter++;
        firstJump = false;
        airJump = false;
        canDoAirJump = false;
    }

    void ResetFallVelocity()
    {
        //If jumping while falling, reset y velocity.
        Vector3 vel = rb.velocity;
        if (rb.velocity.y < 0.5f)
            rb.velocity = new Vector3(vel.x, 0, vel.z);
        else if (rb.velocity.y > 0)
            rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
    }




    bool IsAbleToJump() => !IsSliding() && IsOnGround() && firstJump;

    bool IsAbleToAirJump() => !IsOnGround() && !firstJump && airJump && jumpCounter < maxJumps && canDoAirJump;

    bool IsAbleToBulletJump() => IsSliding() && IsOnGround() && bulletJump && firstJump;

    bool IsAbleToRailJump() => IsOnRail() && firstJump;

    bool IsOnGround() => PlayerStateHandler.Instance.IsOnGround;

    bool IsSliding() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.Sliding;

    bool IsOnRail() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnRail;

    bool IsJumpingDisabled() 
    {
        if(maxJumps <= 0)
        {
            Debug.Log("Jumping is disabled! Max Jumps <= 0");
            return true;
        }
        return false;
    }
}
