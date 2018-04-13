using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class userAsset : MonoBehaviour {
    private float xToY = 1.0f;
    private float xToZ = 1.0f;
    private float yToX = 1.0f;
    private float yToZ = 1.0f;
    private float zToX = 1.0f;
    private float zToY = 1.0f;
    private int[] rotationSnap = { 1, 5, 10, 15, 30, 45, 90 };
    private int rotationSnapIndex = 0;
    private bool maintain = true;
    private bool gravity = true;


    public bool Maintain
    {
        get { return maintain; }
        set { maintain = value; }
    }

    public bool Gravity
    {
        get { return gravity; }
        set { gravity = value; }
    }

    public void Position(Vector3 pos){
        this.transform.position = pos;
        checkBounds ();
    }

    public void Rotation(Vector3 rotation){
        this.transform.localEulerAngles = rotation;
        checkBounds ();
    }

    public void Scale(Vector3 scale){
        if (maintain) {
            maintainProportions (scale);
        } else {
            this.transform.localScale = scale;
        }
        checkBounds ();
    }

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

    void maintainProportions(Vector3 scale){
        if (this.transform.localScale.x != scale.x) {
            xToY = this.transform.localScale.x / this.transform.localScale.y;
            xToZ = this.transform.localScale.x / this.transform.localScale.z;
            scale.y = scale.x / xToY;
            scale.z = scale.x / xToZ;
            this.transform.localScale = scale;
        }
        else if (this.transform.localScale.y != scale.y) {
            yToX = this.transform.localScale.y / this.transform.localScale.x;
            yToZ = this.transform.localScale.y / this.transform.localScale.z;
            scale.x = scale.y / yToX;
            scale.z = scale.y / yToZ;
            this.transform.localScale = scale;
        }
        else if (this.transform.localScale.z != scale.z) {
            zToX = this.transform.localScale.z / this.transform.localScale.x;
            zToY = this.transform.localScale.z / this.transform.localScale.y;
            scale.x = scale.z / zToX;
            scale.y = scale.z / zToY;
            this.transform.localScale = scale;
        }
    }

    public void RotationSnapInc()
    {
        Debug.Log("SnapRotIndex: " + rotationSnapIndex);
        if(rotationSnapIndex < rotationSnap.Length - 1)
        rotationSnapIndex++;
    }

    public void RotationSnapDec()
    {
        Debug.Log("SnapRotIndex: " + rotationSnapIndex);
        if (rotationSnapIndex > 0)
            rotationSnapIndex--;
    }

    public int GetRotationSnap()
    {
        return rotationSnap[rotationSnapIndex];
    }
} 