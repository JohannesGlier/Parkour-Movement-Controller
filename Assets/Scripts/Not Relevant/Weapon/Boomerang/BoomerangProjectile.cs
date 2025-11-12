using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    [Header("Boomerang Settings")]
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private float returnSpeed;
    [SerializeField]
    private float rotationSpeed;
    [SerializeField]
    private float maxDistance;
    [SerializeField]
    private float catchThreshold = 0.2f;
    [SerializeField]
    private float catchForce;

    [Header("Boomerang Effects")]
    [SerializeField]
    private GameObject hitParticleEffect;
    [SerializeField]
    private GameObject catchParticleEffect;

    private Vector3 direction;
    private Vector3 oldPos;
    private GameObject player;
    private float currentDistance = 0;
    private bool wayBack = false;

    public void Initialize(Vector3 dir, GameObject player)
    {
        this.player = player;
        oldPos = transform.position;
        direction = dir;      
    }

    void FixedUpdate()
    {
        if (currentDistance <= maxDistance && !wayBack)
        {
            transform.Rotate(Vector3.down * Time.deltaTime * rotationSpeed);
            transform.position += direction * Time.deltaTime * projectileSpeed;
        }
        else
        {
            wayBack = true;
        }

        if (wayBack)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * (rotationSpeed + 200));

            float step = returnSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z), step);

            if (Vector3.Distance(transform.position, new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z)) < catchThreshold)
            {
                player.GetComponent<Boomerang>().ReturnBoomerang();
                if(player.GetComponent<Rigidbody>().velocity.magnitude > 0.5f)
                    player.GetComponent<Rigidbody>().AddForce(player.GetComponent<PlayerMovement>().CurrentDirection * catchForce, ForceMode.Impulse);
                Destroy(this.gameObject);
            }
        }

        Vector3 distanceVector = transform.position - oldPos;
        float distanceThisFrame = distanceVector.magnitude;
        currentDistance += distanceThisFrame;
        oldPos = transform.position;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall")
        {
            // Dadurch fliegt der Boomerang an der Wand entlang im Falle einer Kollision
            Vector3 collisionPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            Vector3 collisionNormal = transform.position - collisionPoint;
            Vector3 tangent = Vector3.ProjectOnPlane(this.transform.position, collisionNormal).normalized;
            tangent.y = 0;

            var angle = Vector3.Angle(direction, tangent);
            if(angle > 90)
                direction = -tangent;
            else
                direction = tangent;
        }
        else if (other.gameObject.tag == "Player" && other.gameObject != player)
        {
            Instantiate(hitParticleEffect, other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position), Quaternion.identity);
        }
        else if(other.gameObject.tag == "Player" && other.gameObject == player)
        {
            Instantiate(catchParticleEffect, other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position), Quaternion.identity);
        }
    }
}
