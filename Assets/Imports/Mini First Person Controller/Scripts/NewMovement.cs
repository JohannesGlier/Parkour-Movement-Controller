using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NewMovement : MonoBehaviour
{
    public float _moveRate;
    public float _jumpImpulse;

    private bool readyToJump = true;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(float horizontal, float vertical, bool jumping)
    {
        rb.AddForce(Physics.gravity * 6f);

        float multiplier = 0.5f;
        float multiplierV = 0.7f; // 0.5f

        // Get Movement Direction and normalize it (Avoids faster diagonal movement)
        Vector3 movementDirection = new Vector3(horizontal * multiplier, 0, vertical * multiplier * multiplierV).normalized;

        // Important: Add RELATIVE force
        rb.AddRelativeForce(movementDirection * _moveRate * Time.deltaTime);

        Vector3 impulses = Vector3.zero;
        if (jumping)
            impulses += (Vector3.up * _jumpImpulse);
        rb.AddForce(impulses, ForceMode.Impulse);     
    }
}
