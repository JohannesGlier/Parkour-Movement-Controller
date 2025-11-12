using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile : MonoBehaviour
{
    // INSPECTOR VARIABLES
    [Header("Explosion Effect")]
    [SerializeField]
    private GameObject impactPrefab;

    [Header("Projectile Settings")]
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private float knockbackForce;
    [SerializeField]
    private int damage;
    [SerializeField]
    private float destroyTimer;

    // PRIVATE VARIABLES
    private Vector3 lastPosition;



    // ***** UNITY METHODS *****
    private void Start()
    {
        lastPosition = transform.position;
        Destroy(this.gameObject, destroyTimer);
    }

    private void FixedUpdate()
    {
        CheckCollision(lastPosition);

        lastPosition = transform.position;
        transform.position += transform.forward * Time.deltaTime * projectileSpeed;      
    }



    // ***** HELPER METHODS *****
    private void CheckCollision(Vector3 lastPos)
    {
        RaycastHit hit;
        Vector3 direction = transform.position - lastPos;
        Ray ray = new Ray(lastPos, direction);
        float dist = Vector3.Distance(transform.position, lastPos);
        if (Physics.Raycast(ray, out hit, dist))
        {
            // Hit a player?
            if (hit.collider.gameObject.tag == "Player")
            {
                // Knockback
                hit.rigidbody.AddForce(knockbackForce * direction);
                // Damage
                // ToDo...
            }

            // Calculations
            transform.position = hit.point;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            Vector3 pos = hit.point;

            // Explosion Effect
            Instantiate(impactPrefab, pos, rot);

            // Destroy projectile
            Destroy(gameObject);
        }
    }
}
