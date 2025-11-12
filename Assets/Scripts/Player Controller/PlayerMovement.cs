using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] Transform orientation;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 4500;
    [SerializeField] float maxSpeed = 20;
    [SerializeField] float counterMovement = 0.175f;
    [SerializeField] float threshold = 0.01f;
    [SerializeField] float maxSlopeAngle = 35f;

    private Rigidbody rb;

    private Vector3 currentDirection;
    public Vector3 CurrentDirection => currentDirection;

    private Vector3 previousPosition;

    private float horizontal;
    private float vertical;



    void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        PollKeys();
    }

    void PollKeys()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        currentDirection = (transform.position - previousPosition).normalized;
        previousPosition = transform.position;

        Vector2 mag = FindVelRelativeToLook();

        if (PlayerStateHandler.Instance.IsOnGround)
            CounterMovement(horizontal, vertical, mag, threshold, moveSpeed, orientation, counterMovement, maxSpeed);

        float multiplier = 0.5f;
        float multiplierV = 0.7f; // 0.5f

        Vector3 verticalForce = orientation.forward * vertical * moveSpeed * Time.deltaTime * multiplier * multiplierV;
        Vector3 horizontalForce = orientation.right * horizontal * moveSpeed * Time.deltaTime * multiplier;

        ApplyMovementForce(verticalForce, horizontalForce);
    }

    void ApplyMovementForce(Vector3 verticalForce, Vector3 horizontalForce)
    {
        rb.AddForce(verticalForce);
        rb.AddForce(horizontalForce);
    }



    // ****** MOVEMENT STUFF ******

    Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    void CounterMovement(float horizontal, float vertical, Vector2 mag, float threshold, float moveSpeed, Transform orientation, float counterMovement, float maxSpeed)
    {
        // Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(horizontal) < 0.05f || (mag.x < -threshold && horizontal > 0) || (mag.x > threshold && horizontal < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(vertical) < 0.05f || (mag.y < -threshold && vertical > 0) || (mag.y > threshold && vertical < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        // Limit diagonal running
        float horizontalVelocity = CalculateHorizontalVelocity();
        if (horizontalVelocity > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    float CalculateHorizontalVelocity()
    {
        float velX = rb.velocity.x;
        float velZ = rb.velocity.z;
        return Mathf.Sqrt(velX * velX + velZ * velZ);
    }
}
