using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour {
    public OVRInput.Controller controller;
    public string buttonName;
    private GameObject grabbingObject;
    private bool grabbing = false;
    public float grabRadius;
    public LayerMask grabMask;

    void GrabObject()
    {
        grabbing = true;
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(transform.position, grabRadius, transform.forward, 0f, grabMask);

        /*Hit something grab object*/
        if(hits.Length > 0)
        {
            int closestHit = 0;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].distance < hits[closestHit].distance)
                    closestHit = i;
            }
            grabbingObject = hits[closestHit].transform.gameObject;
            grabbingObject.GetComponent<Rigidbody>().isKinematic = true;
            grabbingObject.transform.position = transform.position;
            grabbingObject.transform.parent = transform;
        }
    }

    void DropObject()
    {
        grabbing = false;
        if(grabbingObject != null)
        {
            grabbingObject.transform.parent = null;
            grabbingObject.GetComponent<userAsset>().Physics();
            grabbingObject.GetComponent<Rigidbody>().velocity = OVRInput.GetLocalControllerVelocity(controller);
            grabbingObject.GetComponent<Rigidbody>().angularVelocity = OVRInput.GetLocalControllerAngularVelocity(controller);
            grabbingObject = null;
        }
    }
	// Update is called once per frame
	void Update () {
		if(!grabbing && Input.GetAxis(buttonName) == 1)
        {
            print("asd");
            GrabObject();
        }
        if (grabbing && Input.GetAxis(buttonName) < 1)
        {
            DropObject();
        }

    }
}
