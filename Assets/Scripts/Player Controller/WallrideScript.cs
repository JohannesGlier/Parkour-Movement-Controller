using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class WallrideScript : MonoBehaviour
{
    [Serializable]
    public class Wallinfo
    {
        public Vector3 wallNormal;
        public Vector3 rotationVector;
        public Quaternion wallRotation;

        public Wallinfo(Vector3 wallrideDirection, Vector3 wallNormal, Quaternion wallRotation)
        {
            this.rotationVector = wallrideDirection;
            this.wallNormal = wallNormal;
            this.wallRotation = wallRotation;
        }
    }

    #region EVENTS
    public static event Action StartWallride;                       // Subscribers: PlayerEffectsHandler, SlidingScript
    public static event Action<Wallinfo> StartWallrideWithInfos;    // Subscribers: FirstPersonCamController, ThirdPersonCam
    public static event Action EndWallride;                         // Subscribers: PlayerEffectsHandler, FirstPersonCamController, ThirdPersonCam
    #endregion

    #region INSPECTOR FIELDS
    [Header("Player References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform raycastOrigin;
    [SerializeField] Transform playerObj;

    [Header("General Wallride Settings")]
    [SerializeField] LayerMask wallrideLayers;
    [SerializeField] float raycastLength;
    [SerializeField] float wallrideAngle;

    [Header("Wallride Settings")]
    [SerializeField] float wallForce;
    [SerializeField] float speedBuff;
    [SerializeField] float wallrideGravity;
    #endregion

    ControllerType activeControllerType;
    Rigidbody rb;
    Wallinfo wallInfo;
    int halfpipeLayer;



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeControllerType = ControllerSettings.Instance.ActiveControllerType;
        halfpipeLayer = LayerMask.NameToLayer("Halfpipe");
    }

    void FixedUpdate()
    {
        if (IsWallriding()) UpdatePlayerAlignment();

        if (!IsOnGround()) PrepareWallride();
        else CancleWallride();
    }


    void PrepareWallride()
    {
        wallInfo = DetectNearbyWall();

        if (wallInfo != null) DoWallride();
        else CancleWallride();
    }

    void UpdatePlayerAlignment()
    {
        if (wallInfo == null) return;
        if (activeControllerType != ControllerType.TOP_DOWN) return;

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition * Player_Aiming_Alignment.Instance.MouseSensitivity);
        Vector3 pos = transform.position;
        pos.y += 1;

        Plane p = new Plane(Vector3.up, pos);

        if (p.Raycast(mouseRay, out float hitDist))
        {
            Vector3 hitPoint = mouseRay.GetPoint(hitDist);
            playerObj.LookAt(hitPoint);
            playerObj.rotation = Quaternion.FromToRotation(playerObj.up, wallInfo.rotationVector) * playerObj.rotation;
            playerObj.rotation = Quaternion.Euler(playerObj.rotation.eulerAngles.x, playerObj.rotation.eulerAngles.y, playerObj.rotation.eulerAngles.z);
        }
    }

    Wallinfo DetectNearbyWall()
    {
        (Vector3 direction, Quaternion rotation)[] checks =
        {
            (orientation.forward,    Quaternion.Euler(wallrideAngle, 0, 0)),
            (-orientation.forward,   Quaternion.Euler(-wallrideAngle, 0, 0)),
            (orientation.right,      Quaternion.Euler(0, 0, -wallrideAngle)),
            (-orientation.right,     Quaternion.Euler(0, 0, wallrideAngle))
        };

        foreach (var check in checks)
        {
            Debug.DrawRay(raycastOrigin.position, check.direction * raycastLength, Color.green);
            Wallinfo info = TryGetWallinfo(check.direction, check.rotation);
            if (info != null) return info;
        }

        return null;
    }

    private Wallinfo TryGetWallinfo(Vector3 direction, Quaternion rotation)
    {
        if (Physics.Raycast(raycastOrigin.position, direction, out RaycastHit hit, raycastLength, wallrideLayers))
        {
            if (hit.collider.gameObject.layer == halfpipeLayer) return null;
            Debug.DrawRay(raycastOrigin.position, direction * raycastLength, Color.red);

            Vector3 playerRotation = rotation * hit.normal;
            return new Wallinfo(playerRotation, hit.normal, rotation);
        }

        return null;
    }

    void DoWallride()
    {
        if (PlayerStateHandler.Instance.CurrentPlayerState != PlayerStates.OnWallride)
        {
            PlayerStateHandler.Instance.SetSpecialPlayerState(PlayerStates.OnWallride);
            StartWallride?.Invoke();
            StartWallrideWithInfos?.Invoke(wallInfo);
        }     

        rb.AddForce(-wallInfo.wallNormal * wallForce * Time.deltaTime, ForceMode.Force);   // Kraft Richtung Wand (Drück Spieler gegen Wand)       
        rb.AddForce(Vector3.down * Time.deltaTime * wallrideGravity);           // Kraft Richtung Boden (Gravity)
        rb.AddForce(rb.velocity * Time.deltaTime * speedBuff);                  // Kraft in Bewegungsrichtung (Speedboost)
    }

    void CancleWallride()
    {
        if (IsWallriding())
        {
            PlayerStateHandler.Instance.LeaveSpecialPlayerState();
            EndWallride?.Invoke();
        }

        wallInfo = null;       
    }


    #region HELPER
    bool IsWallriding() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnWallride;
    bool IsOnGround() => PlayerStateHandler.Instance.IsOnGround;
    #endregion
}
