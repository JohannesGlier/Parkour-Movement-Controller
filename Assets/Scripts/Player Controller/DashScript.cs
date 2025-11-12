using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DashScript : MonoBehaviour
{
    [Header("Dash References")]
    [SerializeField] Animator knifeAnimator;
    [SerializeField] PlayerMovement playerMovementScript;

    [Header("Dash Settings")]
    [SerializeField] float dashForce;
    [SerializeField] float dashTime;

    Rigidbody rb;
    ControllerType activeControllerType;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeControllerType = ControllerSettings.Instance.ActiveControllerType;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectDashBasedOnController();
        }
    }

    void SelectDashBasedOnController()
    {
        switch (activeControllerType)
        {
            case ControllerType.TOP_DOWN:
                ExecuteTopDownDash();
                return;
            case ControllerType.FIRST_PERSON:
                ExecuteFirstPersonDash();
                return;
            case ControllerType.THIRD_PERSON:
                ExecuteThirdPersonDash();
                return;
            default:
                return;
        }
    }

    void ExecuteTopDownDash()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane p = new Plane(Vector3.up, transform.position);

        if (p.Raycast(mouseRay, out float hitDist))
        {
            Vector3 myPoint = new Vector3(mouseRay.GetPoint(hitDist).x, transform.position.y, mouseRay.GetPoint(hitDist).z);
            Vector3 dashDir = -(transform.position - myPoint).normalized;

            StartCoroutine(Dash(dashDir));
        }
    }

    void ExecuteFirstPersonDash()
    {
        Vector3 rayDirection = ControllerSettings.Instance.ActiveCamera.transform.forward.normalized;
        StartCoroutine(Dash(rayDirection));
    }

    void ExecuteThirdPersonDash()
    {
        Vector3 rayDirection = transform.Find("Orientation").forward.normalized;
        StartCoroutine(Dash(rayDirection));
    }

    IEnumerator Dash(Vector3 dashDirection)
    {
        playerMovementScript.enabled = false;

        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        knifeAnimator.SetTrigger("Hit");

        yield return new WaitForSeconds(dashTime);
        playerMovementScript.enabled = true;
    } 
}
