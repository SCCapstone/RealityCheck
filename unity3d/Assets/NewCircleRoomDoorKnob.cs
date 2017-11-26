using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewCircleRoomDoorKnob : MonoBehaviour {

    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject KnobGlow;
    public GameObject PlayerText;

    private string onCollidedText;

    // Use this for initialization
    void Start () {
        onCollidedText = "Press \"A\" to create a new circle room";
	}
	
	// Update is called once per frame
	void Update () {

        SphereCollider sphereBox = GetComponent<SphereCollider>();
        Vector3 leftPos = leftHand.transform.position;
        Vector3 rightPos = rightHand.transform.position;
        if (sphereBox.bounds.Contains(leftPos) || sphereBox.bounds.Contains(rightPos))
        {
            KnobGlow.SetActive(true);
            PlayerText.GetComponent<Text>().text = onCollidedText;
            if (Input.GetButton("AButton"))
            {
                SceneManager.LoadScene("newCircleRoom", LoadSceneMode.Single);
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
