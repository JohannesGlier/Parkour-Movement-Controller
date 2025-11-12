using UnityEngine;

public class Shotgun : MonoBehaviour
{
    [System.Serializable]
    public class Projectile
    {
        public Rigidbody projectilePrefab;
        public GameObject muzzleflare;        
        public float fireCooldown;
        public int shotgunPellets;
    }

    // INSPECTOR VARIABLES
    [Header("Spawnpoints")]
    [SerializeField]
    private Transform spawnLocator;
    [SerializeField]
    private Transform spawnLocatorMuzzleFlare;
    [SerializeField]
    private Transform[] shotgunLocator;

    [Header("Projectile Settings")]
    [SerializeField]
    private Projectile projectile;

    [Header("Gun Settings")]
    [SerializeField]
    private int maxAmmo;
    [SerializeField]
    private float reloadAmmoTime;


    // PRIVATES VARIABLES
    private bool canShoot = true;
    private float timer;   
    private int currentAmmo;
    private float reloadAmmoTimer;



    // ***** UNITY METHODS *****

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        // Input Handling
        if (Input.GetButtonDown("Fire1") && canShoot)
        {          
            Fire();
            canShoot = false;
        }

        if (!canShoot)
        {
            timer += Time.deltaTime;
            if(timer >= projectile.fireCooldown)
            {
                timer = 0;
                canShoot = true;
            }
        }

        /** Ammo Reloading
         * 
         *  When Firing:        reloading is slower
         *  When not firing:    reloading is faster
         */
        if (currentAmmo <= 0)
        {
            reloadAmmoTimer += Time.deltaTime;
            if (reloadAmmoTimer >= reloadAmmoTime)
            {
                currentAmmo = maxAmmo;
                reloadAmmoTimer = 0;
            }
        }
    }



    // ***** HELPER METHODS *****

    private void Fire()
    {
        if (currentAmmo > 0)
        {
            // Muzzle Flash
            Instantiate(projectile.muzzleflare, spawnLocatorMuzzleFlare.position, spawnLocatorMuzzleFlare.rotation);

            Instantiate(projectile.projectilePrefab, spawnLocator.position, spawnLocator.rotation);

            for (int i = 0; i < projectile.shotgunPellets; i++)
            {
                Instantiate(projectile.projectilePrefab, shotgunLocator[i].position, shotgunLocator[i].rotation);
            }

            currentAmmo--;
        }
    }
}