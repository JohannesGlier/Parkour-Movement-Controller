using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color dyingColor;
    public MeshRenderer shieldMeshRenderer;
    public GameObject fracturedShield;

    public void StartDestroyShield()
    {
        StartCoroutine(DestroyShield());
    }

    private IEnumerator DestroyShield()
    {
        iTween.ColorTo(gameObject, dyingColor, 3);

        float counter = shieldMeshRenderer.material.GetFloat("CracksAmount_");

        while(counter < 1)
        {
            counter += 0.05f;
            shieldMeshRenderer.material.SetFloat("CracksAmount_", counter);
            yield return new WaitForSeconds (0.05f);
        }

        GameObject fractShield = Instantiate (fracturedShield, transform.position, transform.rotation) as GameObject;

        Destroy (gameObject);
    }
}
