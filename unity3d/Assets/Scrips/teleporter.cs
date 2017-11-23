using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleporter : MonoBehaviour {
    public GameObject TeleportMarker;
    public OVRInput.Controller controller;
    public string buttonTeleport;
    public string buttonRingEnable;
    private bool isRingEnable = false;
    public Transform Player;
    public float RayLenght = 50;
	// Use this for initialization

	
	// Update is called once per frame
	void Update () {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;       
       
          
        if (Physics.Raycast(ray, out hit, RayLenght))
        {
            if (hit.collider.tag == "Ground")
            {
                if (!TeleportMarker.activeSelf)
                    TeleportMarker.SetActive(true);
                TeleportMarker.transform.position = hit.point;
                if (Input.GetAxis(buttonTeleport) > 0)
                {
                    Teleport();
                }
            }
            else
            {
                TeleportMarker.SetActive(false);
            }
                
            
        }
        else
        {
            TeleportMarker.SetActive(false);
        }
        
	}

    void Teleport()
    { 
        Vector3 markerPosition = TeleportMarker.transform.position;
        Player.position = new Vector3(markerPosition.x, Player.position.y, markerPosition.z);
    }
}
