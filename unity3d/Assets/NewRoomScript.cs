using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewRoomScript : MonoBehaviour {

    public GameObject PlayerText;

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
            if (PlayerText.GetComponent<Text>().text == pauseText)
            {
                PlayerText.GetComponent<Text>().text = "";
            }
            else
            {
                PlayerText.GetComponent<Text>().text = pauseText;
            }
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
