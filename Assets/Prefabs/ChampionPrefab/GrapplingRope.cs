using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingRope : MonoBehaviour
{
    #region INSPECTOR FIELDS
    [Header("General Settings:")]
    [SerializeField] 
    int percision = 40;

    [Range(0, 20)] 
    [SerializeField] 
    float straightenLineSpeed = 5;

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)] 
    [SerializeField] 
    float StartWaveSize = 2;
    
    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [SerializeField] 
    [Range(1, 50)] 
    float ropeProgressionSpeed = 1;
    #endregion

    #region PRIVATE FIELDS
    bool isGrappling = true;
    float waveSize = 0;
    float moveTime = 0;
    bool strightLine = true;
    LineRenderer m_lineRenderer;
    #endregion


    void Awake() => m_lineRenderer = GetComponent<LineRenderer>();

    void OnEnable()
    {
        moveTime = 0;
        m_lineRenderer.positionCount = percision;
        waveSize = StartWaveSize;
        strightLine = false;

        LinePointsToFirePoint();

        m_lineRenderer.enabled = true;
    }

    void OnDisable()
    {
        m_lineRenderer.enabled = false;
        isGrappling = false;
    }



    void Update()
    {
        moveTime += Time.deltaTime;
        DrawRope();
    }


    void LinePointsToFirePoint()
    {
        for (int i = 0; i < percision; i++)
            m_lineRenderer.SetPosition(i, transform.position);
    }

    void DrawRope()
    {
        if (!strightLine)
        {
            if (m_lineRenderer.GetPosition(percision - 1).x == HookScript.Instance.GetGrapplePoint.x)
                strightLine = true;
            else
                DrawRopeWaves();
        }
        else
        {
            if (!isGrappling) isGrappling = true;

            if (waveSize > 0)
            {
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                waveSize = 0;

                if (m_lineRenderer.positionCount != 2) 
                    m_lineRenderer.positionCount = 2;

                DrawRopeNoWaves();
            }
        }
    }

    void DrawRopeWaves()
    {
        for (int i = 0; i < percision; i++)
        {
            float delta = (float)i / ((float)percision - 1f);
            Vector3 offset = Vector2.Perpendicular(HookScript.Instance.GetGrappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            Vector3 targetPosition = Vector3.Lerp(this.transform.position, HookScript.Instance.GetGrapplePoint, delta) + offset;
            Vector3 currentPosition = Vector3.Lerp(this.transform.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            m_lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves()
    {
        m_lineRenderer.SetPosition(0, this.transform.position);
        m_lineRenderer.SetPosition(1, HookScript.Instance.GetGrapplePoint);
    }
}
