using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is used to control the music settings 
/// in the main room as well as store the global music volume setting
/// </summary>

public class MainRoomSettings : MonoBehaviour
{
    public GameObject musicVolumeSliderArea;
    public GameObject musicVolumeSlider;
    public GameObject musicVolumeText;
    public AudioSource music;
    public static float musicVolume = -1; //value from 0 to 100

    private GameObject rayCastEndSphere;
    
    // At start, check if the music volume from the audio souce has been found.
    // If if hasn't, set it to the audio source's volume, then update the slide
    // based on the music volume percentage
    void Start ()
    {
        if (MainRoomSettings.musicVolume == -1)
        {
            MainRoomSettings.musicVolume = music.volume * 100.0f;
        }

        updateSliderFromPercentage(MainRoomSettings.musicVolume);
        music.volume = MainRoomSettings.musicVolume * 0.01f;
    }
	
	// Each frame, check if the ray caster is pointing at the volume slider, if it is
    // update the slider and the current music volume based on where the raycaster is pointing
	void Update ()
    {
        if(rayCastEndSphere == null)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
        }
        
        if (rayCastEndSphere != null && (Input.GetAxisRaw("RightTrigger") > 0.2f || Input.GetAxisRaw("LeftTrigger") > 0.2f))
        {
            // Get the box collider of the slider area and check if the raycaster is colliding with it.
            BoxCollider areaCollider = musicVolumeSliderArea.GetComponent<BoxCollider>();
            if (CheckBoxCollision(areaCollider, rayCastEndSphere.transform.position))
            {
                // Find the location percentage based on where the raycaster is pointing
                // and the start and end points of the slider area
                BoxCollider sliderCollider = musicVolumeSlider.GetComponent<BoxCollider>();
                float sliderHalfWidth = sliderCollider.bounds.size.x / 2.0f;
                float width = areaCollider.bounds.size.x - (sliderHalfWidth * 2.0f);
                
                float percentage = Mathf.Clamp((((rayCastEndSphere.transform.position.x - areaCollider.bounds.center.x) / width) * 100.0f) + 50.0f,
                    0.0f, 100.0f);
                
                // asign the correct volume and update the slider UI
                music.volume = percentage * 0.01f;
                updateSliderFromPercentage(percentage);
            }
        }
    }

    // find if the raycaster is accurately colliding with the box collider
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

    // Updates the location of the position of the slider based on the
    // passed in percentage value.
    // Takes in a value from 0 to 100
    private void updateSliderFromPercentage(float percentage)
    {
        // Enfore the value to be between 0 and 100
        percentage = Mathf.Clamp(percentage, 0.0f, 100.0f);
        MainRoomSettings.musicVolume = percentage;

        musicVolumeText.GetComponent<Text>().text = ((int)Mathf.Round(percentage)).ToString() + "%";

        BoxCollider areaCollider = musicVolumeSliderArea.GetComponent<BoxCollider>();
        BoxCollider sliderCollider = musicVolumeSlider.GetComponent<BoxCollider>();
        float sliderHalfWidth = sliderCollider.bounds.size.x / 2.0f;
        float width = areaCollider.bounds.size.x - (sliderHalfWidth * 2.0f);

        float newXPosition = Mathf.Clamp(areaCollider.bounds.center.x + (((percentage - 50.0f) * 0.01f) * width),
                    areaCollider.bounds.min.x + sliderHalfWidth,
                    areaCollider.bounds.max.x - sliderHalfWidth);

        Vector3 newSliderPosition = new Vector3(newXPosition,
            musicVolumeSlider.transform.position.y, musicVolumeSlider.transform.position.z);

        musicVolumeSlider.transform.position = newSliderPosition;
    }
}
