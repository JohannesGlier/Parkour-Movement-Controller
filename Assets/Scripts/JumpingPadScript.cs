using UnityEngine;
using System;

public class JumpingPadScript : MonoBehaviour
{
    public static event Action OnJumpingPad;

    [Header("Tags")]
    [SerializeField] string[] whatToBoost_Tags;
    [SerializeField] string playerTag;

    [Header("Boost Settings")]
    [SerializeField] float playerDirectionForce;
    [SerializeField] float playerUpwardForce;



    void OnTriggerEnter(Collider other)
    {
        foreach (string tag in whatToBoost_Tags)
        {
            if (other.gameObject.tag == tag)
                AddForce(other);
        }

        if (other.gameObject.tag == playerTag)
            OnJumpingPad?.Invoke();
    }

    void AddForce(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            Vector3 rbDirection = new Vector3(rb.velocity.x, -rb.velocity.y, rb.velocity.z).normalized;
            Vector3 boostDirection = rbDirection * playerDirectionForce + Vector3.up * playerUpwardForce;
            rb.AddForce(boostDirection, ForceMode.Impulse);
        }
    }
}
