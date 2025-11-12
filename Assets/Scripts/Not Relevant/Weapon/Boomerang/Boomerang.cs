using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    [SerializeField]
    private GameObject shurikenAmmo;
    [SerializeField]
    private GameObject shurikenAnim;

    [SerializeField]
    private int maxShurikenCount;
    [SerializeField]
    private float throwDelay;

    private int currentShurikenCount;
    private bool canThrow = true;
    private float delayTimer;



    // ***** PUBLIC METHODS *****

    public void ReturnBoomerang()
    {
        if(currentShurikenCount <= 0)
        {
            shurikenAmmo.SetActive(true);
        }
        currentShurikenCount++;
    }



    // ***** UNITY METHODS *****

    private void Start()
    {
        currentShurikenCount = maxShurikenCount;
    }

    private void Update()
    {
        // Input Handling
        if (Input.GetButtonDown("Fire1") && canThrow)
        {
            ThrowShuriken();
            canThrow = false;
        }

        if (!canThrow)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= throwDelay)
            {
                delayTimer = 0;
                canThrow = true;
            }
        }
    }



    // ***** HELPER METHODS *****

    private void ThrowShuriken()
    {
        if (currentShurikenCount > 0)
        {
            shurikenAnim.SetActive(true);

            Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 pos = this.gameObject.transform.position;
            pos.y += 1;
            Plane p = new Plane(Vector3.up, pos);

            if (p.Raycast(screenRay, out float hitDist))
            {
                Vector3 myPoint = new Vector3(screenRay.GetPoint(hitDist).x, shurikenAnim.transform.GetChild(0).transform.position.y, screenRay.GetPoint(hitDist).z);
                Vector3 rayDirection = -(shurikenAnim.transform.GetChild(0).transform.position - myPoint).normalized;
                shurikenAnim.GetComponent<BoomerangInstantiator>().Initialize(rayDirection, this.gameObject);
            }

            currentShurikenCount--;

            if (currentShurikenCount <= 0)
            {
                shurikenAmmo.SetActive(false);
            }
        }
    }
}
