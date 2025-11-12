using UnityEngine;

public class FallingHandler : MonoBehaviour
{
    #region INSPECTOR FIELDS
    [Header("Animators")]
    [SerializeField] Animator knivesAnimator;

    [Header("Key Bindings")]
    [SerializeField] KeyCode fixAnimKey;
    [SerializeField] KeyCode steezyKey;
    [SerializeField] KeyCode fallKey;

    [Header("Falling")]
    [SerializeField] MonoBehaviour[] scriptsToDisable;
    [SerializeField] PhysicMaterial mat;
    [SerializeField] float torque;
    [SerializeField] float force;
    [SerializeField] float angularDrag = 0.5f;
    [SerializeField] float drag = 1.0f;
    [SerializeField] Transform forcePoint_1;
    [SerializeField] Transform forcePoint_2;
    #endregion

    Rigidbody rb;
    bool fallen;


    void Awake()
    {
        mat.bounciness = 0;
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        CheckForAnimationInputs();

        CheckForFallingInput();   
    }

    void CheckForFallingInput()
    {
        if (Input.GetKeyUp(fallKey))
        {
            Falling();
            
        }
    }

    void Falling()
    {
        foreach (MonoBehaviour mb in scriptsToDisable)
            mb.enabled = false;

        rb.constraints = RigidbodyConstraints.None;

        mat.bounciness = 0.6f;
        rb.drag = drag;
        rb.angularDrag = angularDrag;

        if (!fallen)
            rb.AddForceAtPosition(force * rb.velocity.normalized, forcePoint_1.position, ForceMode.Impulse);

        rb.AddForceAtPosition(2 * rb.velocity.normalized, forcePoint_1.position, ForceMode.Impulse);
        rb.AddRelativeTorque(Vector3.up * torque, ForceMode.Impulse);

        fallen = true;
    }

    void CheckForAnimationInputs()
    {
        if (Input.GetKeyUp(fixAnimKey))
            PlayAnimation("Fix");

        if (Input.GetKeyUp(steezyKey))
            PlayAnimation("Steezy");
    }

    void PlayAnimation(string animationName)
    {
        knivesAnimator.SetTrigger(animationName);
    }
}
