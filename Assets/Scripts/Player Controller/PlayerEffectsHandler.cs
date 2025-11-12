using UnityEngine;

public class PlayerEffectsHandler : MonoBehaviour
{
    [Header("Effect References")]
    [SerializeField] GameObject grindEffect;
    [SerializeField] GameObject wallrideEffect;


    void Awake()
    {
        StopEffect();
    }

    private void OnEnable()
    {
        GrindScript.StartGrinding += PlayEffect;
        GrindScript.EndGrinding += StopEffect;

        WallrideScript.StartWallride += PlayEffect;
        WallrideScript.EndWallride += StopEffect;

        SlidingScript.StartSliding += PlayEffect;
        SlidingScript.EndSliding += StopEffect;
    }

    private void OnDisable()
    {
        GrindScript.StartGrinding -= PlayEffect;
        GrindScript.EndGrinding -= StopEffect;

        WallrideScript.StartWallride -= PlayEffect;
        WallrideScript.EndWallride -= StopEffect;

        SlidingScript.StartSliding -= PlayEffect;
        SlidingScript.EndSliding -= StopEffect;

        StopEffect();
    }

    private void PlayEffect()
    {
        grindEffect.GetComponent<ParticleSystem>().Play();
        wallrideEffect.GetComponent<ParticleSystem>().Play();
    }

    private void StopEffect()
    {
        grindEffect.GetComponent<ParticleSystem>().Stop();
        wallrideEffect.GetComponent<ParticleSystem>().Stop();
    }
}
