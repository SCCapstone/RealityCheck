using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewBoxRoomDoorKnob : MonoBehaviour {

    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject KnobGlow;

    private GameObject rayCastEndSphere;
    private string onCollidedText;

    private bool RTriggerHeld;
    private bool LTriggerHeld;

    private double LDownTime;
    private double RDownTime;
    private double triggerTime = 10.0; // 10 milliseconds
    private bool RTriggerDown;
    private bool LTriggerDown;

    // Use this for initialization
    void Start () {
        onCollidedText = "Press \"A\" to create a new box room";
        LTriggerDown = false;
        RTriggerDown = false;
    }

	// Update is called once per frame
	void Update () {

        if (null == rayCastEndSphere)
        {
            rayCastEndSphere = GameObject.Find("rayCastEndSphere");
        }

        RTriggerDown = getRightTriggerDown();
        LTriggerDown = getLeftTriggerDown();

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
            if (RTriggerDown)
            {
                SceneManager.LoadScene("newBoxRoom", LoadSceneMode.Single);
            }
        }
        else
        {
            KnobGlow.SetActive(false);
        }
    }

    private bool getRightTriggerDown()
    {
        float pressure = Input.GetAxisRaw("RightTrigger");
        bool down = pressure > 0.2f;
        if (down)
        {
            if (RTriggerHeld)
            {
                if (NowMilliseconds() - RDownTime < triggerTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                RDownTime = NowMilliseconds();
                RTriggerHeld = true;
                return true;
            }
        }
        else
        {
            RTriggerHeld = false;
            return false;
        }
    }

    private bool getLeftTriggerDown()
    {
        float pressure = Input.GetAxisRaw("LeftTrigger");
        bool down = pressure > 0.2f;
        if (down)
        {
            if (LTriggerHeld)
            {
                if (NowMilliseconds() - LDownTime < triggerTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LDownTime = NowMilliseconds();
                LTriggerHeld = true;
                return true;
            }
        }
        else
        {
            LTriggerHeld = false;
            return false;
        }
    }

    private double NowMilliseconds()
    {
        return (System.DateTime.UtcNow -
                new System.DateTime(1970, 1, 1, 0, 0, 0,
                    System.DateTimeKind.Utc)).TotalMilliseconds;
    }

}
