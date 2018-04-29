using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class userAsset : MonoBehaviour {

    //Variables used for maintaining scale
    private float xToY = 1.0f;
    private float xToZ = 1.0f;
    private float yToX = 1.0f;
    private float yToZ = 1.0f;
    private float zToX = 1.0f;
    private float zToY = 1.0f;

    //holds all the diffrent snaps
    private int[] rotationSnap = { 1, 5, 10, 15, 30, 45, 90 };

    //Properties of the Model
    private int rotationSnapIndex = 0;
    private bool maintain = true;
    private bool gravity = true;

    //the player gameobject
    private GameObject player;

    //Assign the player object once the model has spawned
    private void Start()
    {
        player = GameObject.Find("[CameraRig]");    
    }

    //Checks if the model is out of bounds each frame
    private void Update()
    {
        if(this.gameObject.transform.localPosition.y < -4)
        {
            this.gameObject.transform.localPosition = new Vector3(player.transform.localPosition.x, 2,
                player.transform.localPosition.z);
        }
    }

    //assignes rather it maintains proportions
    public bool Maintain
    {
        get { return maintain; }
        set { maintain = value; }
    }

    //assigns rather it obeys gravity
    public bool Gravity
    {
        get { return gravity; }
        set { gravity = value; }
    }

    //changes the position of the model
    public void Position(Vector3 pos){
        this.transform.position = pos;
        checkBounds ();
    }

    //changes the rotation of the model
    public void Rotation(Vector3 rotation){
        this.transform.localEulerAngles = rotation;
        checkBounds ();
    }

    //changes the scale of the model
    public void Scale(Vector3 scale)
    {
        if (maintain)
        {
            maintainProportions (scale);
        }
        else
        {
            scale.x = Mathf.Max(scale.x, 0.1f);
            scale.y = Mathf.Max(scale.y, 0.1f);
            scale.z = Mathf.Max(scale.z, 0.1f);

            this.transform.localScale = scale;
        }

        checkBounds();
    }

    //Assignes the phisics of the model
    public void Physics(){
        if (gravity)
        {
            this.gameObject.GetComponent<Rigidbody>().useGravity = true;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        else
        {
            this.gameObject.GetComponent<Rigidbody>().useGravity = false;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    //checks the bounds of the model
    void checkBounds(){
        Quaternion currentRotation = this.gameObject.transform.rotation;
        this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Bounds bounds = new Bounds(this.gameObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in this.gameObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        this.gameObject.transform.rotation = currentRotation;

        float diff = bounds.min.y;

        if (diff < 0) {
            this.gameObject.transform.position += new Vector3 (0, -diff, 0);
        }
    }

    //scales the model keeping its proportions
    void maintainProportions(Vector3 scale)
    {
        if (this.transform.localScale.x != scale.x)
        {
            xToY = this.transform.localScale.x / this.transform.localScale.y;
            xToZ = this.transform.localScale.x / this.transform.localScale.z;
            scale.y = scale.x / xToY;
            scale.z = scale.x / xToZ;

            scale.x = Mathf.Max(scale.x, 0.1f);
            scale.y = Mathf.Max(scale.y, 0.1f);
            scale.z = Mathf.Max(scale.z, 0.1f);

            this.transform.localScale = scale;
        }
        else if (this.transform.localScale.y != scale.y)
        {
            yToX = this.transform.localScale.y / this.transform.localScale.x;
            yToZ = this.transform.localScale.y / this.transform.localScale.z;
            scale.x = scale.y / yToX;
            scale.z = scale.y / yToZ;

            scale.x = Mathf.Max(scale.x, 0.1f);
            scale.y = Mathf.Max(scale.y, 0.1f);
            scale.z = Mathf.Max(scale.z, 0.1f);

            this.transform.localScale = scale;
        }
        else if (this.transform.localScale.z != scale.z)
        {
            zToX = this.transform.localScale.z / this.transform.localScale.x;
            zToY = this.transform.localScale.z / this.transform.localScale.y;
            scale.x = scale.z / zToX;
            scale.y = scale.z / zToY;

            scale.x = Mathf.Max(scale.x, 0.1f);
            scale.y = Mathf.Max(scale.y, 0.1f);
            scale.z = Mathf.Max(scale.z, 0.1f);

            this.transform.localScale = scale;
        }
    }

    //increases the rotaiomn snap
    public void RotationSnapInc()
    {
        Debug.Log("SnapRotIndex: " + rotationSnapIndex);
        if(rotationSnapIndex < rotationSnap.Length - 1)
        rotationSnapIndex++;
    }

    //decreases the rotation snap
    public void RotationSnapDec()
    {
        Debug.Log("SnapRotIndex: " + rotationSnapIndex);
        if (rotationSnapIndex > 0)
            rotationSnapIndex--;
    }

    //gets the rotation snap
    public int GetRotationSnap()
    {
        return rotationSnap[rotationSnapIndex];
    }
} 