using PathCreation;
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrindScript : MonoBehaviour
{
    [Serializable]
    public class GrindInfo
    {
        public PathCreator pathCreator;
        public float distanceTravelled;

        public GrindInfo(PathCreator pathCreator, float distanceTravelled)
        {
            this.pathCreator = pathCreator;
            this.distanceTravelled = distanceTravelled;
        }
    }

    #region EVENTS
    public static event Action StartGrinding;           // Subscribers: PlayerEffectsHandler, HookScript --> Wenn Grind startet, dann cancle WallHook, SlidingScript
    public static event Action<GrindInfo> Grinding;     // Subscribers: FirstPersonCamController, ThirdPersonCam
    public static event Action CancleGrinding;          // Subscribers: JumpingScript --> Wenn Grind gecancelt werden kann, dann ResetJump in JumpingScript
    public static event Action EndGrinding;             // Subscribers: PlayerEffectsHandler, FirstPersonCamController, ThirdPersonCam
    #endregion

    #region INSPECTOR FIELDS
    [Header("General Grind Settings")]
    [SerializeField] bool autoGrind = true;
    [SerializeField] KeyCode grindKey = KeyCode.Space;
    [SerializeField] LayerMask whatIsGrindable;
    [SerializeField] float railDetectionRadius;

    [Header("Player References")]
    [SerializeField] Transform grindCheckOrigin;
    [SerializeField] Transform playerObj;

    [Header("Grind Speed Settings")]
    [SerializeField] float grindSpeedMultiplier;
    [SerializeField] float speedBuffAfterGrind;

    [Header("Grind Settings")]
    [SerializeField] float minGrindAngle;
    [SerializeField] float maxGrindAngle;
    [SerializeField] float cancleGrindTime;
    [SerializeField] float grindCooldown;
    #endregion

    #region PRIVATE FIELDS
    ControllerType activeControllerType;
    Rigidbody rb;
    PathCreator pathCreator;
    GameObject lastRail;

    float velocityBeforeGrind;
    float grindTimer;
    float distanceTravelled;
    float parentZRotation;

    bool readyToCancleGrind = false;
    bool grindingOnCooldown = false;
    bool clockwise = false;

    Vector3 lastPosition;
    #endregion



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(grindCheckOrigin.position, railDetectionRadius);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeControllerType = ControllerSettings.Instance.ActiveControllerType;
    }

    void OnEnable()
    {
        JumpingScript.Jumping += CancleGrind;
        HookScript.Statichook += CancleGrind;
    }

    void OnDisable()
    {
        JumpingScript.Jumping -= CancleGrind;
        HookScript.Statichook -= CancleGrind;
    }



    void Update()
    {
        if (autoGrind || Input.GetKeyDown(grindKey))
            DetectRail();

        if (IsOnRail())
        {
            UpdateGrindTimer();
            Grind();
        }
    }

    void FixedUpdate()
    {
        if (IsOnRail())
        {
            Grinding?.Invoke(new GrindInfo(pathCreator, distanceTravelled));
            UpdatePlayerAlignment();          
        }
    }



    void UpdateGrindTimer()
    {
        grindTimer += Time.deltaTime;

        if (grindTimer >= cancleGrindTime)
        {
            grindTimer = 0;
            readyToCancleGrind = true;
            CancleGrinding?.Invoke();
        }
    }

    void DetectRail()
    {
        if (IsOnRail() || grindingOnCooldown) return;

        GameObject rail = IsRailUnderPlayer();

        if (rail == null) lastRail = null;
        if (rail != null && rail != lastRail) PrepareGrind(rail);
    }

    void PrepareGrind(GameObject rail)
    {
        pathCreator = GetPathCreatorFromRail(rail);

        if (pathCreator == null)
        {
            Debug.Log("Make sure there is a path on the rail object!");
            return;
        }
            
        Vector3 closestPoint = pathCreator.path.GetClosestPointOnPath(grindCheckOrigin.position);
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(closestPoint);

        Vector3 pathDirection;

        // Wichtig für curved rails --> Weil da das Ende des Pfades Probleme macht
        if (distanceTravelled >= pathCreator.path.length - 0.5f && distanceTravelled <= pathCreator.path.length + 0.5f)
            pathDirection = pathCreator.path.GetDirectionAtDistance(distanceTravelled - 2);
        else
            pathDirection = pathCreator.path.GetDirectionAtDistance(distanceTravelled);

        Vector3 currentMoveDirection = new Vector3(rb.velocity.x, grindCheckOrigin.position.y, rb.velocity.z);
        float angle = Vector3.Angle(currentMoveDirection, pathDirection);

        // Wenn man fast im 90° Winkel auf die Rail rennt, dann nicht grinden! (70° - 110° => No Grind)
        if (angle >= minGrindAngle && angle <= maxGrindAngle)
        {
            lastRail = rail;
            return;
        }

        if (angle < minGrindAngle) clockwise = true;
        if (angle > maxGrindAngle) clockwise = false;

        parentZRotation = rail.transform.eulerAngles.y;

        if (parentZRotation == 0) parentZRotation = 180;

        velocityBeforeGrind = rb.velocity.magnitude;

        StartGrinding?.Invoke();       
        PlayerStateHandler.Instance.SetSpecialPlayerState(PlayerStates.OnRail);
    }

    void Grind()
    {
        if(clockwise)
            distanceTravelled += velocityBeforeGrind * grindSpeedMultiplier * Time.deltaTime;
        else
            distanceTravelled -= velocityBeforeGrind * grindSpeedMultiplier * Time.deltaTime;

        Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(distanceTravelled);
        Vector3 currentMoveDirection = new Vector3(rb.velocity.x, grindCheckOrigin.position.y, rb.velocity.z);
        float angle = Vector3.Angle(currentMoveDirection, pathDirection);

        if (LeaveRailByAngle(angle))
        {         
            EndGrind();
            return;
        }

        if (!IsEndOfRail())
        {
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
            lastPosition = transform.position;
            return;
        }

        EndGrind();
    }

    void UpdatePlayerAlignment()
    {
        if (activeControllerType != ControllerType.TOP_DOWN) return;

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition * Player_Aiming_Alignment.Instance.MouseSensitivity);
        Vector3 pos = transform.position;
        pos.y += 1;
        Plane p = new Plane(Vector3.up, pos);

        if (p.Raycast(mouseRay, out float hitDist))
        {
            Vector3 hitPoint = mouseRay.GetPoint(hitDist);
            playerObj.LookAt(hitPoint);

            Quaternion mouseYRotation = Quaternion.Euler(0, playerObj.rotation.eulerAngles.y - parentZRotation, 0);
            Quaternion finalRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled) * mouseYRotation;

            playerObj.rotation = finalRotation;
        }
    }



    void EndGrind()
    {
        PlayerStateHandler.Instance.LeaveSpecialPlayerState();
        EndGrinding?.Invoke();

        grindTimer = 0;        
        StartCoroutine(GrindCooldown());
        readyToCancleGrind = false;

        rb.velocity = velocityBeforeGrind * rb.velocity.normalized * speedBuffAfterGrind;
    }

    void CancleGrind()
    {
        if (IsOnRail() && readyToCancleGrind) EndGrind();
    }

    IEnumerator GrindCooldown()
    {
        grindingOnCooldown = true;
        yield return new WaitForSeconds(grindCooldown);
        grindingOnCooldown = false;      
    }

    GameObject IsRailUnderPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(grindCheckOrigin.position, railDetectionRadius, whatIsGrindable);
        GameObject nearestRail = null;

        float shortestDistanceToCollider = Mathf.Infinity;
        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(hitCollider.transform.position, grindCheckOrigin.position);
            if (distance < shortestDistanceToCollider)
                nearestRail = hitCollider.gameObject;
        }

        return nearestRail;
    }


    #region HELPER
    bool IsOnRail() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnRail;
    bool LeaveRailByAngle(float angle) => (angle >= minGrindAngle && angle <= maxGrindAngle);
    bool IsEndOfRail() => (pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop) == lastPosition);
    PathCreator GetPathCreatorFromRail(GameObject rail) => rail.transform.GetChild(0).GetComponent<PathCreator>();
    #endregion
}
