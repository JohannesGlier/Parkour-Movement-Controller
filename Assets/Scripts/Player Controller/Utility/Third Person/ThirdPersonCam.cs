using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    #region INSPECTOR FIELDS
    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform player;
    [SerializeField] Transform playerObj;
    [SerializeField] Transform combatLookAt;
    [SerializeField] PlayerFlips playerFlips;

    [Header("Slide Settings")]
    [SerializeField] float slideAngle;
    #endregion

    #region PRIVATE FIELDS
    bool wallriding = false;
    WallrideScript.Wallinfo wallInfo;

    bool grinding = false;
    GrindScript.GrindInfo grindInfo;

    bool sliding = false; 
    bool flipping = false;
    int randomFlip;

    float timer = 0f;
    Vector3 normalVector;
    GroundStates lastGroundState;
    #endregion


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        WallrideScript.StartWallrideWithInfos += OnWallride;
        WallrideScript.EndWallride += OnEndWallride;

        GrindScript.Grinding += OnGrinding;
        GrindScript.EndGrinding += OnEndGrinding;

        SlidingScript.StartSliding += OnStartSliding;
        SlidingScript.EndSliding += OnEndSliding;

        PlayerFlips.StartFlip += OnStartFlipping;
    }

    void OnDisable()
    {
        WallrideScript.StartWallrideWithInfos -= OnWallride;
        WallrideScript.EndWallride -= OnEndWallride;

        GrindScript.Grinding -= OnGrinding;
        GrindScript.EndGrinding -= OnEndGrinding;

        SlidingScript.StartSliding -= OnStartSliding;
        SlidingScript.EndSliding -= OnEndSliding;

        PlayerFlips.StartFlip -= OnStartFlipping;
    }



    void Update()
    {
        UpdatePlayerAlignment();
    }

    void UpdatePlayerAlignment()
    {
        Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
        orientation.forward = dirToCombatLookAt.normalized;

        if (grinding)               HandleRailAlignment(dirToCombatLookAt);
        else if (wallriding)        HandleWallrideAlignment(dirToCombatLookAt);
        else if (sliding)           HandleSlideAlignment(dirToCombatLookAt);
        else if (flipping)          HandleFlipRotation(dirToCombatLookAt);
        else if (IsAboveHalfpipe()) HandleHalfpipeAlignment(dirToCombatLookAt);
        else                        playerObj.forward = dirToCombatLookAt.normalized;
    }

    void HandleRailAlignment(Vector3 direction)
    {
        if (!IsOnRail())
            grinding = false;

        float angle = Vector3.SignedAngle(orientation.up, grindInfo.pathCreator.path.GetNormalAtDistance(grindInfo.distanceTravelled), orientation.forward);

        playerObj.forward = direction.normalized;
        playerObj.rotation = Quaternion.Euler(0, playerObj.rotation.eulerAngles.y, angle);
    }

    void HandleWallrideAlignment(Vector3 direction)
    {
        playerObj.forward = direction.normalized;
        playerObj.rotation = Quaternion.Euler(20 * -wallInfo.wallRotation.eulerAngles.x, playerObj.rotation.eulerAngles.y, 20 * -wallInfo.wallRotation.eulerAngles.z);
    }

    void HandleSlideAlignment(Vector3 direction)
    {
        playerObj.forward = direction.normalized;
        playerObj.rotation = Quaternion.Euler(slideAngle, playerObj.rotation.eulerAngles.y, 0);
    }

    void HandleFlipRotation(Vector3 direction)
    {
        playerObj.forward = direction.normalized;

        float angle = Mathf.Lerp(0, 360, timer);

        if (randomFlip == 0)
            playerObj.rotation = Quaternion.Euler(angle, playerObj.rotation.eulerAngles.y, 0);
        else if (randomFlip == 1)
            playerObj.rotation = Quaternion.Euler(-angle, playerObj.rotation.eulerAngles.y, 0);
        else if (randomFlip == 2)
            playerObj.rotation = Quaternion.Euler(0, playerObj.rotation.eulerAngles.y, angle);
        else if (randomFlip == 3)
            playerObj.rotation = Quaternion.Euler(0, playerObj.rotation.eulerAngles.y, -angle);

        timer += Time.deltaTime * playerFlips.FlipSpeedFactor * playerFlips.Curve.Evaluate(timer);

        if (timer > playerFlips.FlipDuration)
        {
            timer = 0.0f;
            flipping = false;
            playerFlips.IsFlipping = false;
        }
    }

    void HandleHalfpipeAlignment(Vector3 direction)
    {
        if (IsOnGround() || IsOnHalfpipe())
        {
            normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
            lastGroundState = GroundStates.AboveHalfpipe;
        }
        else
        {
            if (lastGroundState != GroundStates.AboveHalfpipe)
            {
                // Ich komme vom Boden oder von Slope und bin in der Luft über einer Halfpipe
                // Prüfe, ob ich kurz vorm Landen bin oder nicht
                if (PlayerStateHandler.Instance.GetDistanceToSurface <= Player_Aiming_Alignment.Instance.AirToHalfpipeTheshold)
                    normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
                else
                    normalVector = Vector3.zero;
            }
            else
            {
                normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
                lastGroundState = GroundStates.AboveHalfpipe;
            }
        }

        playerObj.forward = direction.normalized;
        playerObj.rotation = Quaternion.FromToRotation(playerObj.up, normalVector) * playerObj.rotation;
    }



    void OnWallride(WallrideScript.Wallinfo wallinfo)
    {
        playerFlips.IsFlipping = false;
        sliding = false;
        flipping = false;
        grinding = false;
        grindInfo = null;

        wallriding = true;
        wallInfo = wallinfo;
    }

    void OnEndWallride()
    {
        wallriding = false;
        wallInfo = null;
    }

    void OnGrinding(GrindScript.GrindInfo grindInfo)
    {
        playerFlips.IsFlipping = false;
        sliding = false;
        flipping = false;
        wallriding = false;
        wallInfo = null;

        grinding = true;
        this.grindInfo = grindInfo;
    }

    void OnEndGrinding()
    {
        grinding = false;
        grindInfo = null;
    }

    void OnStartSliding()
    {
        playerFlips.IsFlipping = false;
        wallriding = false;
        grinding = false;
        flipping = false;
        sliding = true;
    }

    void OnEndSliding()
    {
        sliding = false;
    }

    void OnStartFlipping()
    {
        wallriding = false;
        grinding = false;
        sliding = false;
        flipping = true;
        timer = 0;

        randomFlip = Random.Range(0, 4);
    }



    #region HELPER
    bool IsAboveHalfpipe() => PlayerStateHandler.Instance.CurrentGroundState == GroundStates.AboveHalfpipe;
    bool IsOnGround() => PlayerStateHandler.Instance.IsOnGround;
    bool IsOnHalfpipe() => PlayerStateHandler.Instance.IsHighOnHalfpipe || PlayerStateHandler.Instance.IsOnHalfpipe;
    bool IsOnRail() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnRail;
    #endregion
}
