using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewCircleRoomDoorKnob : MonoBehaviour {

    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject KnobGlow;

    private GameObject rayCastEndSphere;
    private string onCollidedText;

    // Use this for initialization
    void Start () {
        onCollidedText = "Press \"A\" to create a new circle room";
	}

	// Update is called once per frame
	void Update () {

        if (null == rayCastEndSphere)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
        }

        SphereCollider sphereBox = GetComponent<SphereCollider>();
        Vector3 leftPos = leftHand.transform.position;
        Vector3 rightPos = rightHand.transform.position;

        Vector3 rayPos;
        if (null == rayCastEndSphere)
        {
            rayPos = new Vector3(-1000, -1000, -1000);
        }
        else
        {
            rayPos = rayCastEndSphere.transform.position;
        }

        if (sphereBox.bounds.Contains(leftPos) || sphereBox.bounds.Contains(rightPos)
            || sphereBox.bounds.Contains(rayPos))
        {
            KnobGlow.SetActive(true);
            if (Input.GetButton("AButton") || Input.GetAxis("RightTrigger") > 0.2f)
            {
                SceneManager.LoadScene("newCircleRoom", LoadSceneMode.Single);
            }
        }
        else
        {
            KnobGlow.SetActive(false);
        }

    }


}
