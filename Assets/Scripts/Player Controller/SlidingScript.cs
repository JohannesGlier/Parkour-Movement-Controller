using System;
using UnityEngine;

public class SlidingScript : MonoBehaviour
{
    public static event Action StartSliding;   // Subscribers: PlayerEffectsHandler, ThirdPersonCam
    public static event Action EndSliding;     // Subscribers: PlayerEffectsHandler, ThirdPersonCam

    #region INSPECTOR FIELDS
    [Header("Key Mapping")]
    [SerializeField] KeyCode slideKey = KeyCode.LeftShift;

    [Header("Player References")]
    [SerializeField] Transform playerObj;
    [SerializeField] GameObject sphere;

    [Header("Sliding Settings")]
    [SerializeField] float maxSlideDuration = 3.0f;
    [SerializeField] float slideBoost = 40.0f;
    [SerializeField] float slideGravity = 30.0f;
    [SerializeField] float slideAngle = -45.0f;
    #endregion

    ControllerType activeControllerType;
    Rigidbody rb;
    bool sliding;
    bool canSlide;
    bool cancledWithBulletJump;
    float slideTimer;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeControllerType = ControllerSettings.Instance.ActiveControllerType;
    }

    void OnEnable()
    {
        WallrideScript.StartWallride += CancleSliding;
        GrindScript.StartGrinding += CancleSliding;
        JumpingScript.BulletJumping += CancleWithBulletJump;
    }

    void OnDisable()
    {
        WallrideScript.StartWallride -= CancleSliding;
        GrindScript.StartGrinding -= CancleSliding;
        JumpingScript.BulletJumping -= CancleWithBulletJump;
    }



    void Update()
    {
        sliding = Input.GetKey(slideKey);

        if (cancledWithBulletJump && Input.GetKeyUp(slideKey))
            cancledWithBulletJump = false;
    }

    void FixedUpdate()
    {
        if (IsOnRail() || IsWallriding())
        {
            CancleSliding();
            return;
        }

        canSlide = true;

        if (sliding && canSlide && !cancledWithBulletJump)
            Sliding();
        else
            CancleSliding();
    }


    void Sliding()
    {
        PrepareSliding();
        UpdateSlidingTimer();
        DoSliding();
        UpdatePlayerAlignment();
    }

    void UpdateSlidingTimer()
    {
        slideTimer += Time.deltaTime;

        if (slideTimer >= maxSlideDuration)
        {
            cancledWithBulletJump = true;
            CancleSliding();
            return;
        }
    }

    void PrepareSliding()
    {
        if (!IsSliding())
        {
            PlayerStateHandler.Instance.SetSpecialPlayerState(PlayerStates.Sliding);
            slideTimer = 0;
            StartSliding?.Invoke();
        }
    }

    void DoSliding()
    {
        Vector3 force = rb.velocity.normalized * slideBoost + Vector3.down * slideGravity;
        rb.AddForce(force);
    }

    void UpdatePlayerAlignment()
    {
        if (activeControllerType != ControllerType.TOP_DOWN) return;

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition * Player_Aiming_Alignment.Instance.MouseSensitivity);
        Vector3 pos = this.transform.position;
        pos.y += 1;
        Plane p = new Plane(Vector3.up, pos);

        if (p.Raycast(mouseRay, out float hitDist))
        {
            if (IsAboveHalfpipe()) AlignPlayerToNormalVector(mouseRay, hitDist);
            else AlignPlayerToSlideAngle(mouseRay, hitDist);
        }
    }

    void AlignPlayerToNormalVector(Ray mouseRay, float hitDist)
    {
        Vector3 hitPoint = mouseRay.GetPoint(hitDist);
        playerObj.LookAt(hitPoint);
        playerObj.rotation = Quaternion.FromToRotation(playerObj.up, Player_Aiming_Alignment.Instance.NormalVector) * playerObj.rotation;
        playerObj.rotation = Quaternion.Euler(playerObj.rotation.eulerAngles.x, playerObj.rotation.eulerAngles.y, playerObj.rotation.eulerAngles.z);
    }

    void AlignPlayerToSlideAngle(Ray mouseRay, float hitDist)
    {
        Vector3 hitPoint = mouseRay.GetPoint(hitDist);
        playerObj.LookAt(hitPoint);

        Quaternion targetRotation = Quaternion.identity;
        Vector3 movementDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;

        if (movementDirection != Vector3.zero)
            targetRotation = Quaternion.LookRotation(movementDirection);

        Vector3 mouseY_playerAngleX = Quaternion.Euler(slideAngle, 0, 0) * new Vector3(0, -playerObj.rotation.eulerAngles.y, 0);
        playerObj.rotation = Quaternion.FromToRotation(-playerObj.up, mouseY_playerAngleX) * playerObj.rotation;

        if (targetRotation != Quaternion.identity)
        {
            playerObj.rotation = targetRotation * playerObj.rotation;
            //playerObj.rotation = Quaternion.Euler(playerObj.rotation.eulerAngles.x, playerObj.rotation.eulerAngles.y, playerObj.rotation.eulerAngles.z);
        }
    }


    bool IsOnRail() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnRail;
    bool IsWallriding() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.OnWallride;
    bool IsSliding() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.Sliding;
    bool IsAboveHalfpipe() => PlayerStateHandler.Instance.CurrentGroundState == GroundStates.AboveHalfpipe;


    void CancleSliding()
    {        
        slideTimer = 0;
        canSlide = false;
        if (PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.Sliding)
        {
            PlayerStateHandler.Instance.LeaveSpecialPlayerState();
            EndSliding?.Invoke();
        }
    }

    void CancleWithBulletJump()
    {
        cancledWithBulletJump = true;
        CancleSliding();
    }
}
