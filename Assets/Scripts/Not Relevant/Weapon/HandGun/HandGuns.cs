using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGuns : MonoBehaviour
{
    [System.Serializable]
    public class Projectile
    {
        public Rigidbody projectilePrefab;
        public GameObject muzzleflare;
        public bool rapidFire;
        public float rapidFireCooldown;
    }

    // INSPECTOR VARIABLES
    [Header("Spawnpoints")]
    [SerializeField]
    private Transform spawnLocatorLeft;
    [SerializeField]
    private Transform spawnLocatorRight;
    [SerializeField]
    private Transform spawnLocatorMuzzleFlareLeft;
    [SerializeField]
    private Transform spawnLocatorMuzzleFlareRight;

    [Header("Projectile Settings")]
    [SerializeField]
    private Projectile projectile;

    [Header("Gun Settings")]
    [SerializeField]
    private int maxAmmo;
    [SerializeField]
    private float reloadAmmoTimeNotFiring;
    [SerializeField]
    private float reloadAmmoTimeFiring;

    // PRIVATES VARIABLES
    private bool firing;
    private bool left = true;
    private int currentAmmo;
    private float firingTimer;
    private float reloadAmmoTimerNotFiring;
    private float reloadAmmoTimerFiring;



    // ***** UNITY METHODS *****

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        // Input Handling
        if (Input.GetButtonDown("Fire1"))
        {
            firing = true;
            Fire();
        }
        if (Input.GetButtonUp("Fire1"))
        {
            firing = false;
            firingTimer = 0;
        }

        // Dauerfeuer unter Berücksichtigung des RapidFireCooldowns
        if (projectile.rapidFire && firing)
        {
            if (firingTimer > projectile.rapidFireCooldown)
            {
                Fire();
                firingTimer = 0;
            }
        }

        // Timer
        if (firing)
        {
            firingTimer += Time.deltaTime;
        }

        /** Ammo Reloading
         * 
         *  When Firing:        reloading is slower
         *  When not firing:    reloading is faster
         */
        if (firing)
        {
            reloadAmmoTimerNotFiring = 0;
            if (currentAmmo < maxAmmo)
            {
                reloadAmmoTimerFiring += Time.deltaTime;
                if (reloadAmmoTimerFiring >= reloadAmmoTimeFiring)
                {
                    currentAmmo++;
                    reloadAmmoTimerFiring = 0;
                }
            }
        }
        else
        {
            reloadAmmoTimerFiring = 0;
            if (currentAmmo < maxAmmo)
            {
                reloadAmmoTimerNotFiring += Time.deltaTime;
                if (reloadAmmoTimerNotFiring >= reloadAmmoTimeNotFiring)
                {
                    currentAmmo++;
                    reloadAmmoTimerNotFiring = 0;
                }
            }
        }
    }



    // ***** HELPER METHODS *****

    private void Fire()
    {
        if (currentAmmo > 0)
        {
            if (left)
            {
                // Muzzle Flash
                Instantiate(projectile.muzzleflare, spawnLocatorMuzzleFlareLeft.position, spawnLocatorMuzzleFlareLeft.rotation);

                // Projectile
                Instantiate(projectile.projectilePrefab, spawnLocatorLeft.position, spawnLocatorLeft.rotation);

                left = false;
            }
            else
            {
                // Muzzle Flash
                Instantiate(projectile.muzzleflare, spawnLocatorMuzzleFlareRight.position, spawnLocatorMuzzleFlareRight.rotation);

                // Projectile
                Instantiate(projectile.projectilePrefab, spawnLocatorRight.position, spawnLocatorRight.rotation);

                left = true;
            }

            currentAmmo--;
        }
    }
}
