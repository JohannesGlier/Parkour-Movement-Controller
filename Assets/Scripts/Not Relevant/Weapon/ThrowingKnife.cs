using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingKnife : MonoBehaviour
{
    public Knifes knifesScript;
    public GameObject particleEffect_1;
    public GameObject particleEffect_2;
    public GameObject particleEffect_3;
    public GameObject triggerZone;

    public void OnTriggerEnter(Collider other)
    {
        if((other.tag == "Wall" || other.tag == "Player") && !knifesScript.CanThrowKnife())
        {         
            knifesScript.StopThrowing();
            particleEffect_1.SetActive(true);
            particleEffect_2.SetActive(true);
            particleEffect_3.SetActive(true);
            triggerZone.SetActive(true);
        }
    }

    public void Holding()
    {
        particleEffect_1.SetActive(false);
        particleEffect_2.SetActive(false);
        particleEffect_3.SetActive(false);
        triggerZone.SetActive(false);
    }
}
