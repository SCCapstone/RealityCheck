using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainRoomSettings : MonoBehaviour
{
    public GameObject musicVolumeSliderArea;
    public GameObject musicVolumeSlider;
    public GameObject musicVolumeText;
    public AudioSource music;
    public static float musicVolume = -1; //value from 0 to 100

    private GameObject rayCastEndSphere;
    
    // Use this for initialization
    void Start ()
    {
        if (MainRoomSettings.musicVolume == -1)
        {
            MainRoomSettings.musicVolume = music.volume * 100.0f;
        }

        updateSliderFromPercentage(MainRoomSettings.musicVolume);
        music.volume = MainRoomSettings.musicVolume * 0.01f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(rayCastEndSphere == null)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
        }
        
        if (Input.GetAxisRaw("RightTrigger") > 0.2f)
        {
            BoxCollider areaCollider = musicVolumeSliderArea.GetComponent<BoxCollider>();
            if (CheckBoxCollision(areaCollider, rayCastEndSphere.transform.position))
            {
                BoxCollider sliderCollider = musicVolumeSlider.GetComponent<BoxCollider>();
                float sliderHalfWidth = sliderCollider.bounds.size.z / 2.0f;
                float width = areaCollider.bounds.size.z - (sliderHalfWidth * 2.0f);
                
                float percentage = Mathf.Clamp((((rayCastEndSphere.transform.position.z - areaCollider.bounds.center.z) / width) * 100.0f) + 50.0f,
                    0.0f, 100.0f);
                
                music.volume = percentage * 0.01f;
                updateSliderFromPercentage(percentage);
            }
        }
    }
    
    private bool CheckBoxCollision(BoxCollider collider, Vector3 point)
    {
        Vector3 posToCheck = point;
        Vector3 offset = collider.bounds.center - posToCheck;
        posToCheck = point + offset * 0.25f;
        offset = collider.bounds.center - posToCheck;
        Ray inputRay = new Ray(posToCheck, offset.normalized);
        RaycastHit rHit;

        return !collider.Raycast(inputRay, out rHit, offset.magnitude * 1.1f);
    }

    //Takes in a value from 0 to 100
    private void updateSliderFromPercentage(float percentage)
    {
        percentage = Mathf.Clamp(percentage, 0.0f, 100.0f);
        MainRoomSettings.musicVolume = percentage;

        musicVolumeText.GetComponent<Text>().text = ((int)Mathf.Round(percentage)).ToString() + "%";

        BoxCollider areaCollider = musicVolumeSliderArea.GetComponent<BoxCollider>();
        BoxCollider sliderCollider = musicVolumeSlider.GetComponent<BoxCollider>();
        float sliderHalfWidth = sliderCollider.bounds.size.z / 2.0f;
        float width = areaCollider.bounds.size.z - (sliderHalfWidth * 2.0f);

        float newZPosition = Mathf.Clamp(areaCollider.bounds.center.z + (((percentage - 50.0f) * 0.01f) * width),
                    areaCollider.bounds.min.z + sliderHalfWidth,
                    areaCollider.bounds.max.z - sliderHalfWidth);

        Vector3 newSliderPosition = new Vector3(musicVolumeSlider.transform.position.x,
            musicVolumeSlider.transform.position.y, newZPosition);

        musicVolumeSlider.transform.position = newSliderPosition;
    }
}
