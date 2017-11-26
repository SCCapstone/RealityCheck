using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewRoomScript : MonoBehaviour {

    public GameObject PlayerText;
    private bool isHold = false;
    private string pauseText;

	// Use this for initialization
	void Start () {
        pauseText = "Pause Menu\n\nPress \"A\" to exit to main menu.";

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Start"))
        {
            if (PlayerText.GetComponent<Text>().text == pauseText && isHold == false)
            {
                PlayerText.GetComponent<Text>().text = "";
            }
            else
            {
              if(isHold == false)
                PlayerText.GetComponent<Text>().text = pauseText;
            }
            isHold = true;
        }
        if(!Input.GetButton("Start")) {
          isHold  = false;
        }

        if (Input.GetButton("AButton"))
        {
            if (PlayerText.GetComponent<Text>().text == pauseText)
            {
                PlayerText.GetComponent<Text>().text = "Goodbye";
                SceneManager.LoadScene("scene", LoadSceneMode.Single);
            }
        }
    }
}
