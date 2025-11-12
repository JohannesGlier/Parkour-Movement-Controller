using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    private float speed;
    private float damage;
    private GameObject target;

    public void Init(float speed, float damage, GameObject target)
    {
        this.speed = speed;
        this.damage = damage;
        this.target = target;
    }

    public void Update()
    {
        // Move our position a step closer to the target.
        var step = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, target.transform.Find("Capsule Mesh").transform.position, step);

        // Check if the position of the cube and sphere are approximately equal.
        if (Vector3.Distance(transform.position, target.transform.Find("Capsule Mesh").transform.position) < 0.001f)
        {
            // Deal Damage To Target
            Destroy(this.gameObject);
        }
    }
}
