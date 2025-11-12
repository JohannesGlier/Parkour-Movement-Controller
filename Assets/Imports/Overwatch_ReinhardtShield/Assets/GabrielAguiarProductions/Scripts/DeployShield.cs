using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeployShield : MonoBehaviour
{
    public Camera cam;
    public GameObject shieldPrefab;
    public Transform spawnPoint;
    public float spawnMaxDistance = 7;
    public Vector3 spawnOffset = new Vector3 (0, 1, 0);
    public AudioSource audioSource;
    public AudioClip shieldUp;
    public AudioClip shieldDown;
    [Space]
    [Header("SHAKE OPTIONS & PP")]
    public float shakeDelay = 7;
    public Volume volume;
    public float chromaticGoal = 0.5f;
    public float chromaticRate = 0.1f;
    public CinemachineImpulseSource impulseSource;
    public float shakeDuration=1;
    public float shakeAmplitude=5;
    public float shakeFrequency=2.5f;

    private GameObject shieldVFX;
    private bool shieldActive;
    private Vector3 destination;    
    private ChromaticAberration chromatic;
    private bool chromaticIncrease;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(volume != null)
            volume.profile.TryGet<ChromaticAberration>(out chromatic);   
    }

    void Update()
    {
        if(Input.GetMouseButton(1) && !shieldActive)
        {
            if(shieldPrefab != null)
                ActivateShield();
        }

        if(Input.GetMouseButtonUp(1) && shieldActive)
        {            
            if(shieldPrefab != null)
                DisableShield();
        }
    }

    void ActivateShield ()
    {
        shieldActive = true;

        if(audioSource != null && shieldUp != null)
            audioSource.PlayOneShot(shieldUp);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, spawnMaxDistance))
            destination = hit.point;
        else
            destination = ray.GetPoint(spawnMaxDistance);

        shieldVFX = Instantiate (shieldPrefab, spawnPoint.transform) as GameObject;
        shieldVFX.transform.position = destination + spawnOffset;
        //shieldVFX.GetComponent<Shield>().SetAudioSource(audioSource);

        if(impulseSource != null)
            StartCoroutine (ShakeCameraWithImpulse());

        if(chromatic != null)
            StartCoroutine (ChromaticAberrationPunch());
    }

    void DisableShield ()
    {
        shieldActive = false;

        if(audioSource != null && shieldDown != null)
            audioSource.PlayOneShot(shieldDown);

        if(shieldVFX != null)
            StopAllCoroutines ();

        Destroy (shieldVFX);
    }

    IEnumerator ShakeCameraWithImpulse()
    {
        yield return new WaitForSeconds (shakeDelay);

        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = shakeDuration;
        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeAmplitude;
        impulseSource.m_ImpulseDefinition.m_FrequencyGain = shakeFrequency;
        impulseSource.GenerateImpulse();
    }

    IEnumerator ChromaticAberrationPunch()
    {    
        yield return new WaitForSeconds (shakeDelay);

        if(!chromaticIncrease)
        {    
            chromaticIncrease = true;
            float amount = 0;
            while (amount < chromaticGoal)
            {
                amount += chromaticRate;
                chromatic.intensity.value = amount;
                yield return new WaitForSeconds (0.05f);
            }
            while (amount > 0)
            {
                amount -= chromaticRate;
                chromatic.intensity.value = amount;
                yield return new WaitForSeconds (0.05f);
            }
            chromaticIncrease = false;
        }
    }    
}
