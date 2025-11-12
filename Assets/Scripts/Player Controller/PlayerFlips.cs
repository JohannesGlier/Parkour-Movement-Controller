using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFlips : MonoBehaviour
{
    public static event Action StartFlip;   // Subscribers: Player_Aiming_Alignment
    public static event Action EndFlip;     // Subscribers: Player_Aiming_Alignment

    #region INSPECTOR FIELDS
    [Header("Player References")]
    [SerializeField] Transform playerObj;
    [SerializeField] Animator knivesAnimator;

    [Header("General Settings")]
    [SerializeField] float sphereCastRadius = 5f;
    [SerializeField] LayerMask obstacles;

    [Header("Flip Settings")]
    [SerializeField] bool isFlipping;
    [SerializeField] float flipSpeedFactor;
    [SerializeField] float flipDuration = 1.0f;
    [SerializeField] AnimationCurve curve;
    [SerializeField] string[] kniveAnimName;
    #endregion

    #region PROPERTIES
    public AnimationCurve Curve => curve;
    public float FlipSpeedFactor => flipSpeedFactor;
    public float FlipDuration => flipDuration;
    public bool IsFlipping
    {
        get => isFlipping;
        set => isFlipping = value;
    }
    #endregion

    Rigidbody rb;
    ControllerType activeControllerType;
    float timer = 0f;
    float flipAngle; 



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sphereCastRadius);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeControllerType = ControllerSettings.Instance.ActiveControllerType;
    }

    void FixedUpdate()
    {
        if (!isFlipping && IsPlayerAtHighestPointInAir())
        {
            // Überprüfe, ob sich etwas um mich herum befindet
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereCastRadius, obstacles);

            if (hitColliders.Length > 0) return;
 
            flipAngle = (UnityEngine.Random.Range(0, 4) == 0) ? 720 : 360;  // Decides whether to do a double or a single flip
            
            if (knivesAnimator != null && kniveAnimName.Length > 0)
                knivesAnimator.SetTrigger(kniveAnimName[UnityEngine.Random.Range(0, kniveAnimName.Length)]);

            StartFlip?.Invoke();
            isFlipping = true;
        }

        if (isFlipping && activeControllerType == ControllerType.TOP_DOWN)
            DoFlip();    
    }


    void DoFlip()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition * Player_Aiming_Alignment.Instance.MouseSensitivity);
        Vector3 pos = transform.position;
        pos.y += 1;
        Plane p = new Plane(Vector3.up, pos);

        if (p.Raycast(mouseRay, out float hitDist))
        {
            Vector3 hitPoint = mouseRay.GetPoint(hitDist);
            playerObj.LookAt(hitPoint);

            float angle = Mathf.Lerp(0, flipAngle, timer);
            Vector3 mouseY_Flip = Quaternion.Euler(angle, 0, 0) * new Vector3(0, playerObj.rotation.eulerAngles.y, 0);
            playerObj.rotation = Quaternion.FromToRotation(playerObj.up, mouseY_Flip) * playerObj.rotation;
        }

        UpdateFlipTimer();
    }

    void UpdateFlipTimer()
    {
        timer += Time.deltaTime * flipSpeedFactor * curve.Evaluate(timer);

        if (timer > flipDuration)
        {
            timer = 0.0f;
            isFlipping = false;
            EndFlip?.Invoke();
        }
    }


    bool PlayerInAir() => PlayerStateHandler.Instance.CurrentPlayerState == PlayerStates.InAir;
    bool IsPlayerAtHighestPointInAir() => PlayerInAir() && rb.velocity.y <= 0.25f && rb.velocity.y >= -0.25f;

}
