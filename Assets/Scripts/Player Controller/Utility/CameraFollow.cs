using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    Transform player;

    void LateUpdate()
    {
        transform.position = player.position + new Vector3(0, 23.08f, -8.63f);
    }
}
