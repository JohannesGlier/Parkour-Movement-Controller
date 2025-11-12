using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // PRIVATE VARIABLES
    

    // PRIVATE REFERENCES
    private PlayerMovement playerMovement;
    private NewMovement movementScript;


    public void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        movementScript = GetComponent<NewMovement>();
    }

    
}
