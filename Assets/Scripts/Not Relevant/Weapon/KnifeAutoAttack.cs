using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeAutoAttack : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Debug.Log("Getroffen");
            Destroy(other.gameObject);
        }
    }
}
