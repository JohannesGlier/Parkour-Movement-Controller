using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviour
{
    [Header("Cursor")]
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public float radius = 5f;
    public float mouseSensitivity = 3;

    [Header("Gun Attributes")]
    public float damage = 10;
    public float range = 10;
    public float fireRate = 15f;
    public int reflects = 1;
    public LayerMask shootable;

    //public LayerMask shootingLayer;
    public Transform gunTip;
    public LineRenderer laserLineRenderer;
    public Transform player;
    public Material mat;

    private bool shooting = false;

    public Vector2 mousePosition;
    public Vector2 originalMousePos;

    void Start()
    {
        //Vector2 cursorOffset = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        //Cursor.SetCursor(cursorTexture, cursorOffset, cursorMode);
        //Cursor.visible = false;

        laserLineRenderer.SetPosition(0, gunTip.position);
        laserLineRenderer.SetPosition(1, gunTip.position);
        laserLineRenderer.SetPosition(2, gunTip.position);
        laserLineRenderer.enabled = true;
    }

    public void OnGUI()
    {
        GUI.DrawTexture(new Rect(mousePosition.x - (32 / 2), Screen.height - mousePosition.y - (32 / 2), 32, 32), cursorTexture);
    }

    public static Vector3 ClampMagnitudeMin(Vector3 vector, float minLength)
    {
        if (vector.sqrMagnitude < minLength * minLength)
            return vector.normalized * minLength;
        return vector;
    }

    public void Update()
    {
        Vector2 pos = new Vector2(Input.mousePosition.x * mouseSensitivity, Input.mousePosition.y * mouseSensitivity);
        Vector2 center = new Vector2(Screen.width/2, Screen.height/2);

        Vector2 direction = pos - center;
        direction = ClampMagnitudeMin(direction, radius); //direction from Center to Cursor

        mousePosition = direction + center;
        originalMousePos = Input.mousePosition * mouseSensitivity;

        if (Input.GetMouseButtonDown(0))
        {
            shooting = true;
            Shoot();
        }
        if(!shooting)
        {
            Ray screenRay = Camera.main.ScreenPointToRay(mousePosition);

            Plane p = new Plane(Vector3.up, player.position);
            RaycastHit hit;
            if (p.Raycast(screenRay, out float hitDist))
            {
                Vector3 myPoint = new Vector3(screenRay.GetPoint(hitDist).x, gunTip.position.y, screenRay.GetPoint(hitDist).z);
                Vector3 rayDirection = -(gunTip.position - myPoint).normalized;

                if (Physics.Raycast(gunTip.position, rayDirection, out hit, shootable))
                {
                    laserLineRenderer.startWidth = 0.02f;
                    laserLineRenderer.endWidth = 0.02f;
                    laserLineRenderer.SetPosition(0, gunTip.position);
                    laserLineRenderer.SetPosition(1, gunTip.position + 90 * gunTip.transform.forward);
                    laserLineRenderer.SetPosition(2, gunTip.position + 90 * gunTip.transform.forward);
                }
            }
        }  
    }

    private void Shoot()
    {
        // Get Mouse Position
        Ray screenRay = Camera.main.ScreenPointToRay(mousePosition);
        Vector3 pos = player.position;
        pos.y += 1;
        Plane p = new Plane(Vector3.up, pos);
        RaycastHit hit;     

        if (p.Raycast(screenRay, out float hitDist))
        {
            Vector3 myPoint = new Vector3(screenRay.GetPoint(hitDist).x, gunTip.position.y, screenRay.GetPoint(hitDist).z);
            Vector3 rayDirection = -(gunTip.position - myPoint).normalized;
           
            if (Physics.Raycast(gunTip.position, rayDirection, out hit, shootable))
            {
                Debug.Log("Raycast Performed: " + hit + " | Start Vector: " + gunTip.position + " | Direction: " + rayDirection);
                laserLineRenderer.startWidth = 0.2f;
                laserLineRenderer.endWidth = 0.2f;
                laserLineRenderer.positionCount = 2 + reflects;
                // Habe ich einen Spieler getroffen?
                if (hit.collider.tag == "Player")
                {
                    Debug.Log("Spieler getroffen");

                    laserLineRenderer.SetPosition(0, gunTip.position);
                    laserLineRenderer.SetPosition(1, gunTip.position + 90 * gunTip.transform.forward);
                    laserLineRenderer.SetPosition(2, gunTip.position + 90 * gunTip.transform.forward);

                    Destroy(hit.collider.gameObject);

                    StartCoroutine(DisableLine());
                    return;
                }

                DoWallShot(rayDirection, hit, gunTip.position, 0, range);

                StartCoroutine(DisableLine());
            }
        }
    }

    private void DoWallShot(Vector3 rayDirection, RaycastHit hit, Vector3 oldPos, int reflectsCounter, float newRange)
    {
        if (reflectsCounter < reflects)
        {
            reflectsCounter++;
            RaycastHit newHit;

            // Wall shot case (Man trifft auf einen Collider, hat aber noch Reichweite übrig)
            if (hit.distance <= newRange)
            {
                laserLineRenderer.SetPosition(reflectsCounter-1, oldPos);
                laserLineRenderer.SetPosition(reflectsCounter, hit.point);

                // Berechne übrige Reichweite
                newRange = newRange - Vector3.Distance(hit.point, oldPos);
                // Einfallswinkel = Ausfallswinkel
                Vector3 newDir = Vector3.Reflect(rayDirection, hit.normal);

                // Erneut Schießen
                if (Physics.Raycast(hit.point, newDir, out newHit, shootable))
                {
                    // Habe ich einen Spieler getroffen?
                    if (newHit.collider.tag == "Player")
                    {
                        Debug.Log("Spieler getroffen");

                        laserLineRenderer.SetPosition(reflectsCounter + 1, newHit.point);

                        Destroy(newHit.collider.gameObject);

                        StartCoroutine(DisableLine());
                        return;
                    }

                    // Wallshot nur einmal erlaubt, er kann nicht 2 mal abprallen
                    if (newHit.distance <= newRange)
                        DoWallShot(newDir, newHit, hit.point, reflectsCounter, newRange);
                    else
                        laserLineRenderer.SetPosition(reflectsCounter + 1, hit.point + (newRange * newDir));
                }
            }
            else
            {
                // Schuss der nichts trifft
                laserLineRenderer.SetPosition(reflectsCounter - 1, oldPos);
                laserLineRenderer.SetPosition(reflectsCounter, oldPos + (newRange * rayDirection));
                laserLineRenderer.SetPosition(reflectsCounter + 1, oldPos + (newRange * rayDirection));
                Debug.Log("Guntip: " + gunTip.transform.forward);
            }
        }
        else
        {
            laserLineRenderer.SetPosition(reflectsCounter, oldPos);
            laserLineRenderer.SetPosition(reflectsCounter + 1, oldPos + (newRange * rayDirection));
        }
    }

    private IEnumerator DisableLine()
    {
        
        float fadeSpeed = 1f;
        float timeElapsed = 0f;
        float alpha = 1f;

        while (timeElapsed < fadeSpeed)
        {
            alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeSpeed);

            laserLineRenderer.material.SetColor("_BaseColor", new Color(0, 147, 191, alpha));

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        //yield return null;
        shooting = false;     
        //mat.SetColor("_BaseColor", new Color(0, 195, 255, 1));
    }
}
