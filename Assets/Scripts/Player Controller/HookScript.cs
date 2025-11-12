using System.Collections;
using UnityEngine;
using System;


public class HookScript : Singleton<HookScript>
{
    #region EVENTS
    public static event Action Statichook;      // Subscribers: GrindScript --> Cancle Grind, wenn Wand gehookt wird
    public static event Action Enemyhook;       // Subscribers: None
    public static event Action Objecthook;      // Subscribers: None
    #endregion

    #region INSPECTOR FIELDS
    [Header("Hook References")]   
    [SerializeField] Transform hookObj;

    [Header("General Hook Settings")]
    [SerializeField] LayerMask staticHookingLayers;
    [SerializeField] LayerMask enemyHookingLayers;

    [Header("Hook Settings")]
    [SerializeField] float maxHookingRange = 20f;
    [SerializeField] float bigSphereCastRadius = 1.5f;
    [SerializeField] float smallSphereCastRadius = 0.3f;
    [SerializeField] float overlapSphereRadius = 4;

    [Header("Spring Joint Settings")]
    [SerializeField] float spring = 20f;
    [SerializeField] float damper = 3f;
    [SerializeField] float massScale = 1f;
    #endregion

    #region PRIVATE FIELDS 
    ControllerType activeControllerType;
    Vector3 grapplePoint;
    Vector3 grappleDistanceVector;
    SpringJoint springJoint;
    GameObject target;
    GrapplingRope grapplingRope;
    bool staticHook = false;
    #endregion

    #region PROPERTIES
    public Vector3 GetGrapplePoint => grapplePoint;
    public Vector3 GetGrappleDistanceVector => grappleDistanceVector;
    #endregion


    protected override void Awake()
    {
        base.Awake();
        grapplingRope = hookObj.GetComponent<GrapplingRope>();
        activeControllerType = ControllerSettings.Instance.ActiveControllerType;
    }

    void OnEnable()
    {
        GrindScript.StartGrinding += StopStaticHook;
    }

    void OnDisable()
    {
        GrindScript.StartGrinding -= StopStaticHook;
    }



    void Update()
    {
        if (Input.GetMouseButtonDown(1)) StartHooking();
        if (Input.GetMouseButtonUp(1)) StopHooking();
    }

    void LateUpdate()
    {
        UpdateGrapplePoint();
    }


    void StartHooking()
    {
        if (IsOnGround()) SelectGroundHook();    
        else AirHook();   
    }


    void SelectGroundHook()
    {
        switch (activeControllerType)
        {
            case ControllerType.TOP_DOWN:       GroundHook_TopDown();       return;
            case ControllerType.FIRST_PERSON:   GroundHook_FirstPerson();   return;
            case ControllerType.THIRD_PERSON:   GroundHook_ThirdPerson();   return;
            default: return;
        }
    }

    void GroundHook_TopDown()
    {
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 pos = transform.position;
        pos.y += 1;
        Plane p = new Plane(Vector3.up, pos);
        RaycastHit hit;

        if (p.Raycast(screenRay, out float hitDist))
        {
            Vector3 myPoint = new Vector3(screenRay.GetPoint(hitDist).x, hookObj.position.y, screenRay.GetPoint(hitDist).z);
            Vector3 rayDirection = -(hookObj.position - myPoint).normalized;
            GroundHook_Algorithm(rayDirection);
        }
    }

    void GroundHook_FirstPerson()
    {
        Vector3 rayDirection = transform.Find("Orientation").forward;
        rayDirection = new Vector3(rayDirection.x, 0, rayDirection.z);
        GroundHook_Algorithm(rayDirection);
    }

    void GroundHook_ThirdPerson()
    {
        Vector3 rayDirection = transform.Find("Orientation").forward;
        rayDirection = new Vector3(rayDirection.x, 0, rayDirection.z);
        GroundHook_Algorithm(rayDirection);
    }

    void GroundHook_Algorithm(Vector3 rayDirection)
    {
        // First: Search for enemies with a big spherecast
        GameObject hookableObj = ExecuteSphereCastHook(rayDirection, bigSphereCastRadius, enemyHookingLayers);

        if (hookableObj != null)
        {
            EnemyHook(hookableObj);
            return;
        }

        // Second: Search for enemies that are close to you with a thin sphereCast
        hookableObj = ExecuteSphereCastHook(rayDirection, smallSphereCastRadius, enemyHookingLayers);

        if (hookableObj != null)
        {
            EnemyHook(hookableObj);
            return;
        }

        // Third: Search for static objects with a precise raycast
        Vector3 hitPoint = ExecuteRaycastHook(rayDirection, staticHookingLayers);

        if (hitPoint != Vector3.zero)
        {
            StaticHook(hitPoint);
            return;
        }

        // Fourth: Nothing found --> Show empty hook line
        grapplePoint = hookObj.position + (rayDirection * maxHookingRange);
        StartCoroutine(ShowEmptyHook());
    }


    void AirHook()
    {
        // First: Search for enemies in the air
        GameObject enemy = FindClosestEnemyInAir();

        if (enemy != null)
        {
            EnemyHook(enemy);
            return;
        }

        // Second: Raycast to the ground, then check for nearby enemies at that location
        Vector3 pointOnGround = ShootRayOnGround();
        enemy = FindClosestEnemyOnGround(pointOnGround);

        if (enemy != null)
        {
            EnemyHook(enemy);
            return;
        }

        // Third: Look for static objects on the players air level
        Vector3 hitPoint = ShootRayParellelToPlayer();

        if (hitPoint != Vector3.zero)
        {
            StaticHook(hitPoint);
            return;
        }

        // Fourth: Cast a ray from the player to the point on the ground and check if it collides with anything (e.g., a wall)
        Vector3 rayDirection = -(hookObj.position - pointOnGround).normalized;
        hitPoint = ExecuteRaycastHook(rayDirection, staticHookingLayers);

        if (hitPoint != Vector3.zero)
        {
            StaticHook(hitPoint);
            return;
        }

        // Fourth: Nothing found --> Show empty hook line
        grapplePoint = hookObj.position + (rayDirection * maxHookingRange);
        StartCoroutine(ShowEmptyHook());
    }


    Vector3 ShootRayParellelToPlayer()
    {
        if (activeControllerType == ControllerType.TOP_DOWN)
        {
            Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 pos = transform.position;
            pos.y += 1;
            Plane p = new Plane(Vector3.up, pos);

            if (p.Raycast(screenRay, out float hitDist))
            {
                Vector3 myPoint = new Vector3(screenRay.GetPoint(hitDist).x, hookObj.position.y, screenRay.GetPoint(hitDist).z);
                Vector3 rayDirection = -(hookObj.position - myPoint).normalized;

                Vector3 hitPoint = ExecuteRaycastHook(rayDirection, staticHookingLayers);
                return hitPoint;
            }

            return Vector3.zero;
        }
        else if (activeControllerType == ControllerType.FIRST_PERSON)
        {
            Vector3 rayDirection = ControllerSettings.Instance.ActiveCamera.transform.forward;

            Vector3 hitPoint = ExecuteRaycastHook(rayDirection, staticHookingLayers);
            return hitPoint;
        }
        else if(activeControllerType == ControllerType.FIRST_PERSON)
        {
            Vector3 rayDirection = ControllerSettings.Instance.ActiveCamera.transform.forward;

            Vector3 hitPoint = ExecuteRaycastHook(rayDirection, staticHookingLayers);
            return hitPoint;
        }

        return Vector3.zero;
    }

    GameObject ExecuteSphereCastHook(Vector3 rayDirection, float collisionRadius, LayerMask whatIsHookable)
    {
        RaycastHit hit;

        if (Physics.SphereCast(hookObj.position, collisionRadius, rayDirection, out hit, maxHookingRange, whatIsHookable))
        {
            float distanceFromPoint = Vector3.Distance(transform.position, hit.point);
            grappleDistanceVector = hit.point - transform.position;

            if (distanceFromPoint <= maxHookingRange) return hit.collider.gameObject;
        }
        return null;
    }

    Vector3 ExecuteRaycastHook(Vector3 rayDirection, LayerMask whatIsHookable)
    {
        RaycastHit hit;

        if (Physics.Raycast(hookObj.position, rayDirection, out hit, maxHookingRange, whatIsHookable))
        {
            float distanceFromPoint = Vector3.Distance(transform.position, hit.point);
            grappleDistanceVector = hit.point - transform.position;

            if (distanceFromPoint <= maxHookingRange) return hit.point;
        }
        return Vector3.zero;
    }

    GameObject FindClosestEnemyInAir()
    {
        if (!EnemiesInsideScanner()) return null;

        GameObject closestEnemyInAir = null;

        float minDistance = Mathf.Infinity;
        foreach (GameObject enemy in ScannerScript.Instance.EnemyList)
        {
            float distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (distance < minDistance && !enemy.GetComponent<EnemyStates>().IsGrounded)
                closestEnemyInAir = enemy;
        }

        if (closestEnemyInAir != null) { 
            float distanceFromPoint = Vector3.Distance(transform.position, closestEnemyInAir.transform.position);
            grappleDistanceVector = closestEnemyInAir.transform.position - transform.position;

            if (distanceFromPoint <= maxHookingRange) return closestEnemyInAir;
        }
        return null;
    }

    GameObject FindClosestEnemyOnGround(Vector3 pointOnGround)
    {
        Collider[] hitColliders = Physics.OverlapSphere(pointOnGround, overlapSphereRadius, enemyHookingLayers);

        // Find closest enemy
        GameObject enemyToHook = null;

        float shortestDistanceToPoint = Mathf.Infinity;
        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(hitCollider.transform.position, pointOnGround);
            if (distance < shortestDistanceToPoint)
                enemyToHook = hitCollider.gameObject;
        }

        // Wenn Enemy gefunden, dann
        if (enemyToHook != null)
        {
            float distanceFromPoint = Vector3.Distance(transform.position, enemyToHook.transform.position);
            grappleDistanceVector = enemyToHook.transform.position - transform.position;

            if (distanceFromPoint <= maxHookingRange)
                return enemyToHook;
        }

        return null;
    }

    Vector3 ShootRayOnGround()
    {
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 pos = transform.position;
        pos = new Vector3(pos.x, 0, pos.z);
        Plane p = new Plane(Vector3.up, pos);
        RaycastHit hit;

        if (p.Raycast(screenRay, out float hitDist))
        {
            Vector3 myPoint = new Vector3(screenRay.GetPoint(hitDist).x, screenRay.GetPoint(hitDist).y, screenRay.GetPoint(hitDist).z);
            Vector3 rayDirection = -(hookObj.position - myPoint).normalized;

            if (Physics.Raycast(hookObj.position, rayDirection, out hit, maxHookingRange, staticHookingLayers))
            {
                pos = new Vector3(pos.x, hit.point.y, pos.z);
                p = new Plane(Vector3.up, pos);

                if (p.Raycast(screenRay, out hitDist))
                {
                    myPoint = new Vector3(screenRay.GetPoint(hitDist).x, screenRay.GetPoint(hitDist).y, screenRay.GetPoint(hitDist).z);
                    return myPoint;
                }
            }
            return myPoint;
        }
        return Vector3.zero;
    }


    void EnemyHook(GameObject enemy)
    {
        target = enemy;

        springJoint = gameObject.AddComponent<SpringJoint>();

        springJoint.enableCollision = true;
        springJoint.connectedBody = target.GetComponent<Rigidbody>();

        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = new Vector3(0, 0, 0);
        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.massScale = massScale;

        grapplingRope.enabled = true;
    }

    void StaticHook(Vector3 grapplepoint)
    {
        staticHook = true;
        Statichook?.Invoke();

        grapplePoint = grapplepoint;

        springJoint = gameObject.AddComponent<SpringJoint>();

        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = grapplePoint;
        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.massScale = massScale;

        grapplingRope.enabled = true;    
    }


    void UpdateGrapplePoint()
    {
        if (!springJoint) return;
        if (target == null) return;

        Transform targetGrapplePoint = target.transform.Find("GrapplePoint");
        grapplePoint = new Vector3(target.transform.position.x, targetGrapplePoint.position.y, target.transform.position.z);
    }

    IEnumerator ShowEmptyHook()
    {
        grapplingRope.enabled = true;
        yield return new WaitForSecondsRealtime(0.1f);
        grapplingRope.enabled = false;
    }


    void StopStaticHook()
    {
        if (staticHook) StopHooking();
    }

    void StopHooking()
    {
        grapplingRope.enabled = false;
        Destroy(springJoint);
        target = null;
        staticHook = false;
    }


    #region HELPER
    bool IsOnGround() => PlayerStateHandler.Instance.IsOnGround;
    bool EnemiesInsideScanner() => ScannerScript.Instance.EnemyList.Count > 0;
    #endregion
}
