using UnityEngine;

public class EnemyStates : MonoBehaviour
{
    [Header("Ground Check")]
    [Tooltip("Maximum distance from the ground")]
    [SerializeField] float distanceThreshold = .15f;
    [SerializeField] bool isGrounded = true;

    public bool IsGrounded => isGrounded;
    Vector3 RaycastOrigin => transform.position + Vector3.up * OriginOffset;

    const float OriginOffset = .001f;
    


    void LateUpdate()
    {
        DoGroundCheck();
    }

    void DoGroundCheck()
    {
        isGrounded = Physics.Raycast(RaycastOrigin, Vector3.down, distanceThreshold * 2);
    }
}
