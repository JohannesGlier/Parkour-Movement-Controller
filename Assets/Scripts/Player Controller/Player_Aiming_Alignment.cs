using UnityEngine;

public class Player_Aiming_Alignment : Singleton<Player_Aiming_Alignment>
{
    [Header("Aiming Settings")]
    [SerializeField] Transform playerObj;
    [SerializeField] float mouseSensitivity = 1;

    [Header("Thresholds")]
    [SerializeField] float airToSlopeThreshold = 1.3f;
    [SerializeField] float airToHalfpipeThreshold = 3.0f;


    public float MouseSensitivity => mouseSensitivity;
    public Vector3 NormalVector => normalVector;
    public float AirToHalfpipeTheshold => airToHalfpipeThreshold;


    bool isFlipping;
    Vector3 normalVector;
    GroundStates lastGroundState;



    void OnEnable()
    {
        PlayerFlips.StartFlip += StopUpdatingRotation;
        PlayerFlips.EndFlip += StartUpdatingRotation;
    }

    void OnDisable()
    {
        PlayerFlips.StartFlip -= StopUpdatingRotation;
        PlayerFlips.EndFlip -= StartUpdatingRotation;
    }

    void FixedUpdate()
    {
        if(!isFlipping) RotatePlayerToSurfaceAndMouse();
    }

    void Update()
    {
        UpdateNormalVector();
    }



    void UpdateNormalVector()
    {
        if (IsAboveGround() || IsAboveWall())
        {
            if (IsOnGround())
            {
                normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
                lastGroundState = GroundStates.AboveGround;
            }
            else
            {
                if (lastGroundState != GroundStates.AboveGround)
                {
                    // Ich komme von einer Slope oder Halfpipe und bin in der Luft über den Boden
                    // Rotiere mich zu Vector3.Zero
                    normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
                    lastGroundState = GroundStates.AboveGround;
                }
            }
        }
        else if (IsAboveSlope())    // 9 = Slope
        {
            if (IsOnGround())
            {
                normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
                lastGroundState = GroundStates.AboveSlope;
            }
            else
            {
                if (lastGroundState == GroundStates.AboveSlope)
                    normalVector = Vector3.zero;
                else
                {
                    // Ich komme vom Boden, von Halfpipe oder Slope und bin in der Luft über einer Slope
                    // Prüfe, ob ich kurz vorm Landen bin oder nicht
                    if (PlayerStateHandler.Instance.GetDistanceToSurface <= airToSlopeThreshold)
                        normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
                    else
                        normalVector = Vector3.zero;
                }
            }
        }
        else if (IsAboveHalfpipe())    // 8 = Halfpipe
        {
            if (IsOnGround() || IsHighOnHalfpipe() || IsOnHalfpipe())
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
                    if (PlayerStateHandler.Instance.GetDistanceToSurface <= airToHalfpipeThreshold)
                        normalVector = PlayerStateHandler.Instance.GetSurfaceNormal;
                    else
                        normalVector = Vector3.zero;
                }
            }
        }
    }

    void RotatePlayerToSurfaceAndMouse()
    {
        if (!IsWallriding() && !IsRailGrinding() && !IsSliding())
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition * mouseSensitivity);
            Vector3 pos = transform.position;
            pos.y += 1;

            Plane p = new Plane(Vector3.up, pos);

            if (p.Raycast(mouseRay, out float hitDist))
            {
                Vector3 hitPoint = mouseRay.GetPoint(hitDist);
                playerObj.LookAt(hitPoint);
                playerObj.rotation = Quaternion.FromToRotation(playerObj.up, normalVector) * playerObj.rotation;
                playerObj.rotation = Quaternion.Euler(playerObj.rotation.eulerAngles.x, playerObj.rotation.eulerAngles.y, playerObj.rotation.eulerAngles.z);
            }
        }    
    }


    #region HELPER
    bool IsWallriding() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnWallride;
    bool IsRailGrinding() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnRail;
    bool IsSliding() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.Sliding;
    bool IsOnGround() => PlayerStateHandler.Instance.IsOnGround;
    bool IsHighOnHalfpipe() => PlayerStateHandler.Instance.IsHighOnHalfpipe;
    bool IsOnHalfpipe() => PlayerStateHandler.Instance.IsOnHalfpipe;


    bool IsAboveGround() => PlayerStateHandler.Instance.CurrentGroundState == GroundStates.AboveGround;
    bool IsAboveWall() => PlayerStateHandler.Instance.CurrentGroundState == GroundStates.AboveWall;
    bool IsAboveSlope() => PlayerStateHandler.Instance.CurrentGroundState == GroundStates.AboveSlope;
    bool IsAboveHalfpipe() => PlayerStateHandler.Instance.CurrentGroundState == GroundStates.AboveHalfpipe;
    #endregion


    void StopUpdatingRotation()
    {
        isFlipping = true;
    }

    void StartUpdatingRotation()
    {
        isFlipping = false;
    }
}
