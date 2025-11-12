using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRigidbody1 : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 verticalMovementDirection;
    private Vector3 horizontalMovementDirection;

    public Vector3 HorizontalMovementDirection
    {
        set
        {
            horizontalMovementDirection = value;
        }
        get
        {
            return horizontalMovementDirection;
        }
    }

    public Vector3 VerticalMovementDirection
    {
        set
        {
            verticalMovementDirection = value;
        }
        get
        {
            return verticalMovementDirection;
        }
    }

    public Vector3 Velocity
    {
        get
        {
            return rb.velocity;
        }
    }

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    

    

    
}
