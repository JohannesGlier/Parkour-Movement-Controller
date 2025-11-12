using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangInstantiator : MonoBehaviour
{
    [SerializeField]
    private GameObject shurikenPrefab;

    private Vector3 direction;
    private GameObject player;

    public void SpawnBoomerang()
    {
        GameObject go = Instantiate(shurikenPrefab, this.gameObject.GetComponentInChildren<Transform>().position, this.gameObject.GetComponentInChildren<Transform>().rotation);
        go.GetComponent<BoomerangProjectile>().Initialize(direction, player);
    }

    public void DeactivateBoomerang()
    {
        this.gameObject.SetActive(false);
    }

    public void Initialize(Vector3 direction, GameObject player)
    {
        this.direction = direction;
        this.player = player;
    }
}
