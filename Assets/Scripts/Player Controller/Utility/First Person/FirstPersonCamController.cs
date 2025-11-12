using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamController : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;

    [Header("Mouse Settings")]
    public float sensX;
    public float sensY;
    
    [Header("Lerp Settings")]
    public float normalCamLerpSpeed = 100;
    public float specialCamLerpSpeed = 10;
    public float lerpDuration = 0.5f;

    [Header("Wallride Cam Settings")]
    public int wallrideAngle;
    
    
    private float xRotation;
    private float yRotation;

    private float lerpingTimer = 0;

    private bool wallriding = false;
    private bool endWallride = false;
    private WallrideScript.Wallinfo wallInfo;
    
    private bool grinding = false;
    private bool endGrinding = false;
    private GrindScript.GrindInfo grindInfo;
    


    // ****** UNITY METHODS ******

    private void Start()
    {
        WallrideScript.StartWallrideWithInfos += OnWallride;
        WallrideScript.EndWallride += OnEndWallride;

        GrindScript.Grinding += OnGrinding;
        GrindScript.EndGrinding += OnEndGrinding;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        WallrideScript.StartWallrideWithInfos -= OnWallride;
        WallrideScript.EndWallride -= OnEndWallride;

        GrindScript.Grinding -= OnGrinding;
        GrindScript.EndGrinding -= OnEndGrinding;
    }

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        // rotate cam and orientation
        if (wallriding)
        {
            Quaternion targetQuaternion = Quaternion.Euler((-wallInfo.wallRotation.x * wallrideAngle) + xRotation, (wallInfo.wallRotation.y * wallrideAngle) + yRotation, -wallInfo.wallRotation.z * wallrideAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, specialCamLerpSpeed * Time.deltaTime);
        }
        else if(endWallride)
        {
            if (lerpingTimer < lerpDuration)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(xRotation, yRotation, 0), specialCamLerpSpeed * Time.deltaTime);
                lerpingTimer += Time.deltaTime;
            }
            else
            {
                lerpingTimer = 0;
                endWallride = false;
            }
        }
        else if (grinding)
        {
            float angle = Vector3.SignedAngle(orientation.up, grindInfo.pathCreator.path.GetNormalAtDistance(grindInfo.distanceTravelled), orientation.forward);
            Quaternion targetQuaternion = Quaternion.Euler(xRotation, yRotation, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, specialCamLerpSpeed * Time.deltaTime);
        }
        else if (endGrinding)
        {
            if (lerpingTimer < lerpDuration)
            {              
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(xRotation, yRotation, 0), specialCamLerpSpeed * Time.deltaTime);
                lerpingTimer += Time.deltaTime;
            }
            else
            {
                lerpingTimer = 0;
                endGrinding = false;
            }
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(xRotation, yRotation, 0), normalCamLerpSpeed * Time.deltaTime);
        }

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        playerObj.rotation = Quaternion.Euler(0, yRotation, 0);
    }



    // ****** EVENT METHODS ******

    private void OnWallride(WallrideScript.Wallinfo wallinfo)
    {
        grinding = false;
        grindInfo = null;

        wallriding = true;
        wallInfo = wallinfo;
        lerpingTimer = 0;
    }

    private void OnEndWallride()
    {
        wallriding = false;
        wallInfo = null;
        endWallride = true;
        lerpingTimer = 0;
    }

    private void OnGrinding(GrindScript.GrindInfo grindInfo)
    {
        wallriding = false;
        wallInfo = null;

        grinding = true;
        this.grindInfo = grindInfo;

        lerpingTimer = 0;
    }

    private void OnEndGrinding()
    {
        grinding = false;
        grindInfo = null;
        endGrinding = true;
        lerpingTimer = 0;
    }
}
