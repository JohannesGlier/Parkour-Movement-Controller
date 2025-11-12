using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    [SerializeField]
    public float maxHealth;
    [SerializeField]
    public float shockWaveRadius;
    [SerializeField]
    public float showWaveRate;
    [SerializeField]
    public GameObject shockWavePrefab;
    [SerializeField]
    public float shockWaveSpeed;
    [SerializeField]
    public float shockWaveDamage;

    [SerializeField]
    public float drag_ShieldActive;
    [SerializeField]
    public float mass_ShieldActive;
    [SerializeField]
    public float movementSpeed_ShieldActive;

    [SerializeField]
    public float drag_ShieldNotActive;
    [SerializeField]
    public float mass_ShieldNotActive;
    [SerializeField]
    public float movementSpeed_ShieldNotActive;

    [SerializeField]
    private float regenerationAmount;

    public GameObject shieldPrefab;
    public Transform spawnPoint_Shield;
    public Vector3 spawnOffset = new Vector3(0, 0.36f, 0);
    public Vector3 shieldScale = new Vector3(1.8f, 1.3f, 1.3f);

    private float regenerationTimer = 0;
    private float shockWaveTimer = 0;
    private float currentHealth;
    private bool pufferShield = false;
    private bool shieldActive = false;
    private bool regenerateShield = false;
    private bool destroyed = false;
    private GameObject shieldVFX;




    public void DamageShield(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // Destroy Shield
            destroyed = true;
            regenerateShield = true;
            shieldVFX.GetComponent<Shield>().StartDestroyShield();
        }
    }




    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!destroyed)
        {
            if (Input.GetMouseButtonDown(0) && !shieldActive && !pufferShield)
            {
                if (shieldPrefab != null)
                    StartCoroutine(ActivateShield());

                Debug.Log("Activate Shield");
            }
            else if (Input.GetMouseButtonDown(0) && shieldActive && !pufferShield)
            {
                if (shieldPrefab != null)
                    DisableShield();

                Debug.Log("Deactivate Shield");
            }
        }

        if (shieldActive && !pufferShield && !destroyed)
            ShockWaves();

        if (regenerateShield)
            Regnerate();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = shieldVFX.transform.Find("CenterCircle").position;

        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, shockWaveRadius);
    }




    private void ShockWaves()
    {
        shockWaveTimer += Time.deltaTime;
        if (shockWaveTimer >= showWaveRate) 
        {
            shockWaveTimer = 0;

            Vector3 center = shieldVFX.transform.Find("CenterCircle").position;

            Collider[] hitColliders = Physics.OverlapSphere(center, shockWaveRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.transform.tag == "Player" && hitCollider.gameObject != this.gameObject)
                {
                    GameObject shockwave = Instantiate(shockWavePrefab, center, Quaternion.identity) as GameObject;
                    shockwave.GetComponent<Shockwave>().Init(shockWaveSpeed, shockWaveDamage, hitCollider.gameObject);
                    Debug.Log("Damage Player: " + hitCollider.transform.name);
                }
            }
        }
    }

    private void Regnerate()
    {
        regenerationTimer += Time.deltaTime;

        if (regenerationTimer >= 1)
        {
            regenerationTimer = 0;
            currentHealth += regenerationAmount;

            if (currentHealth >= maxHealth)
            {
                currentHealth = maxHealth;
                regenerateShield = false;
                destroyed = false;
            }
        }
    }

    private IEnumerator ActivateShield()
    {
        pufferShield = true;

        shieldVFX = Instantiate(shieldPrefab, spawnPoint_Shield.transform) as GameObject;
        shieldVFX.transform.position += spawnOffset;
        shieldVFX.transform.localScale = shieldScale;

        // Set Player Stats (Drag, Speed, Mass)

        yield return new WaitForSecondsRealtime(shieldVFX.GetComponent<Animation>().clip.length);

        pufferShield = false;
        shieldActive = true;
    }

    private void DisableShield()
    {
        // Set Player Stats (Drag, Speed, Mass)

        shieldActive = false;
        regenerateShield = true;

        if (shieldVFX != null)
            StopAllCoroutines();

        Destroy(shieldVFX);
    }
}
