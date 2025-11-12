using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityHandler : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] float defaultGravity;
    [SerializeField] float halfpipeGravity;
    
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        if (PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnWallride) return;

        if (IsOnHalfpipe())
            ApplyHalfpipeGravity();
        else
            ApplyDefaultGravity();
    }

    bool IsOnHalfpipe() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnGround_OnHalfpipe || PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnHalfpipe;

    void ApplyHalfpipeGravity() => rb.AddForce(Vector3.down * Time.deltaTime * halfpipeGravity);

    void ApplyDefaultGravity() => rb.AddForce(Vector3.down * Time.deltaTime * defaultGravity);
}
