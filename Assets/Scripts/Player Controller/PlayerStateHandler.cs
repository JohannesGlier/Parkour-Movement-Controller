using UnityEngine;
using System;

public enum PlayerStates
{
    InAir, OnGround_OnHalfpipe, OnGround, OnHalfpipe, HighOnHalfpipe, OnWallride, OnRail, Sliding
}

public enum GroundStates
{
    AboveHalfpipe, AboveSlope, AboveGround, AboveWall, AboveRail
}

public class PlayerStateHandler : Singleton<PlayerStateHandler>
{
    public static event Action Landing;

    #region INSPECTOR FIELDS
    [Header("Player States")]
    [SerializeField] PlayerStates currentPlayerState = PlayerStates.OnGround;
    [SerializeField] GroundStates currentGroundState = GroundStates.AboveGround;

    [Header("References")]
    [SerializeField] Transform playerObj;

    [Header("LayerMasks")]
    [SerializeField] LayerMask halfpipeLayer;
    [SerializeField] LayerMask groundLayers;

    [Header("Thresholds")]
    [SerializeField] float onGroundThreshold = 0.3f;
    [SerializeField] float onHalfpipeThreshold = 1.1f;
    [SerializeField] float highOnHalfpipeThreshold = 0.7f;

    [Header("Bool States")]
    [SerializeField] bool isOnHalfpipe;
    [SerializeField] bool isOnGround;
    [SerializeField] bool isHighOnHalfpipe;
    #endregion

    #region PROPERTIES
    public PlayerStates CurrentPlayerState => currentPlayerState;
    public GroundStates CurrentGroundState => currentGroundState;
    public bool IsOnHalfpipe => isOnHalfpipe;
    public bool IsOnGround => isOnGround;
    public bool IsHighOnHalfpipe => isHighOnHalfpipe;
    public Vector3 GetSurfaceNormal => surfaceNormal;
    public float GetDistanceToSurface => distanceToSurface;
    #endregion

    #region LAYER CACHES
    private int railLayerNumber;
    private int groundLayerNumber;
    private int slopeLayerNumber;
    private int halfpipeLayerNumber;
    private int wallLayerNumber;
    #endregion

    Vector3 surfaceNormal;
    float distanceToSurface;
    bool isInSpecialState = false;
    float raycastOriginOffset = 0.1f;
    Vector3 RaycastOrigin => transform.position + Vector3.up * raycastOriginOffset;



    public void SetSpecialPlayerState(PlayerStates playerState)
    {
        if (IsSpecialPlayerState(playerState))
        {
            currentPlayerState = playerState;
            isInSpecialState = true;
        }      
    }

    bool IsSpecialPlayerState(PlayerStates playerState)
    {
        return playerState switch
        {
            PlayerStates.OnWallride => true,
            PlayerStates.OnRail => true,
            PlayerStates.Sliding => true,
            _ => false // Alle anderen Fälle
        };
    }

    public void LeaveSpecialPlayerState() => isInSpecialState = false;



    protected override void Awake()
    {
        base.Awake();

        railLayerNumber = LayerMask.NameToLayer("Rail");
        groundLayerNumber = LayerMask.NameToLayer("Ground");
        slopeLayerNumber = LayerMask.NameToLayer("Slope");
        halfpipeLayerNumber = LayerMask.NameToLayer("Halfpipe");
        wallLayerNumber = LayerMask.NameToLayer("Wall");
    }

    void LateUpdate()
    {
        UpdatePlayerState();
        UpdateGroundState();
    }

    void UpdatePlayerState()
    {
        bool onGround = GroundCheck();

        if (IsPlayerLanding(onGround))
            Landing?.Invoke();

        isOnGround = onGround;

        // Re-Evaluate isOnGround
        if (currentPlayerState == PlayerStates.Sliding)
            isOnGround = SlidingGroundCheck();

        if (isInSpecialState) return;

        isHighOnHalfpipe = Physics.Raycast(RaycastOrigin, -playerObj.up, highOnHalfpipeThreshold + raycastOriginOffset, halfpipeLayer);
        isOnHalfpipe = Physics.Raycast(RaycastOrigin, Vector3.down, onHalfpipeThreshold + raycastOriginOffset, halfpipeLayer);

        if (On_Ground_But_Not_On_Halfpipe(isOnHalfpipe, isHighOnHalfpipe))
            currentPlayerState = PlayerStates.OnGround;
        else if (On_Ground_And_On_Halfpipe(isOnHalfpipe))
            currentPlayerState = PlayerStates.OnGround_OnHalfpipe;
        else if (Not_On_Ground_But_On_Halfpipe(isOnHalfpipe))
            currentPlayerState = PlayerStates.OnHalfpipe;
        else if (Not_On_Ground_But_High_On_Halfpipe(isOnHalfpipe, isHighOnHalfpipe))
            currentPlayerState = PlayerStates.HighOnHalfpipe;
        else if (Not_On_Ground_Not_On_Halfpipe(isOnHalfpipe, isHighOnHalfpipe))
            currentPlayerState = PlayerStates.InAir;
    }

    void UpdateGroundState()
    {
        RaycastHit hit;
        if (Physics.Raycast(RaycastOrigin, Vector3.down, out hit, Mathf.Infinity))
        {
            surfaceNormal = hit.normal;
            distanceToSurface = hit.distance;
            int hitLayer = hit.collider.gameObject.layer;
            SetCurrentGroundState(hitLayer);
        }
    }



    bool On_Ground_But_Not_On_Halfpipe(bool isOnHalfpipe, bool isHighOnHalfpipe) => isOnGround && !isOnHalfpipe && !isHighOnHalfpipe;
    bool On_Ground_And_On_Halfpipe(bool isOnHalfpipe) => isOnGround && isOnHalfpipe;
    bool Not_On_Ground_But_On_Halfpipe(bool isOnHalfpipe) => !isOnGround && isOnHalfpipe;
    bool Not_On_Ground_But_High_On_Halfpipe(bool isOnHalfpipe, bool isHighOnHalfpipe) => !isOnGround && !isOnHalfpipe && isHighOnHalfpipe;
    bool Not_On_Ground_Not_On_Halfpipe(bool isOnHalfpipe, bool isHighOnHalfpipe) => !isOnGround && !isOnHalfpipe && !isHighOnHalfpipe;

    bool GroundCheck() => Physics.Raycast(RaycastOrigin, Vector3.down, onGroundThreshold + raycastOriginOffset, groundLayers);

    bool IsPlayerLanding(bool isGroundedNow) => isGroundedNow && !isOnGround;

    bool SlidingGroundCheck()
    {
        Vector3 slidingOrigin = transform.position + Vector3.up * 0.2f;
        return Physics.Raycast(slidingOrigin, Vector3.down, onGroundThreshold + raycastOriginOffset, groundLayers);
    }

    void SetCurrentGroundState(int hitLayer)
    {
        if (hitLayer == railLayerNumber)
            currentGroundState = GroundStates.AboveRail;
        else if (hitLayer == groundLayerNumber)
            currentGroundState = GroundStates.AboveGround;
        else if (hitLayer == slopeLayerNumber)
            currentGroundState = GroundStates.AboveSlope;
        else if (hitLayer == halfpipeLayerNumber)
            currentGroundState = GroundStates.AboveHalfpipe;
        else if (hitLayer == wallLayerNumber)
            currentGroundState = GroundStates.AboveWall;
        else
            currentGroundState = GroundStates.AboveGround;
    }
}
