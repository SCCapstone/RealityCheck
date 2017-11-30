using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewBoxRoomDoorKnob : MonoBehaviour {

    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject KnobGlow;
    public GameObject PlayerText;

    private GameObject rayCastEndSphere;
    private string onCollidedText;

    // Use this for initialization
    void Start () {
        onCollidedText = "Press \"A\" to create a new box room";
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
            PlayerText.GetComponent<Text>().text = onCollidedText;
            if (Input.GetButton("AButton"))
            {
                SceneManager.LoadScene("newBoxRoom", LoadSceneMode.Single);
            }
        }
        else
        {
            if (PlayerText.GetComponent<Text>().text == onCollidedText)
            {
                PlayerText.GetComponent<Text>().text = "";
            }
            
            KnobGlow.SetActive(false);
        }

    }
    

}
