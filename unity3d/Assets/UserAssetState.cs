using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class UserAssetState : MonoBehaviour
{
    // unity's Vec3 and Quanternion are not serializible
    [Serializable]
    public class SVec {
        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;
    }

    [Serializable]
    public class SQuat
    {
        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;
        public float w = 0.0f;
    }

    public string uuid;

    public SVec pos = new SVec();
    public SQuat rot = new SQuat();

    public bool gravity;
    public bool maintainProportions;

    public float xToY = 1.0f;
    public float xToZ = 1.0f;
    public float yToX = 1.0f;
    public float yToZ = 1.0f;
    public float zToX = 1.0f;
    public float zToY = 1.0f;

    public static UserAssetState FromGameObject(GameObject obj)
    {
        var state = new UserAssetState();

        var ua = obj.GetComponent<userAsset>();

        state.pos.x = ua.transform.position.x;
        state.pos.y = ua.transform.position.y;
        state.pos.z = ua.transform.position.z;

        state.rot.x = ua.transform.rotation.x;
        state.rot.y = ua.transform.rotation.y;
        state.rot.z = ua.transform.rotation.z;
        state.rot.w = ua.transform.rotation.w;

        state.uuid = obj.name;

        state.xToY = ua.xToY;
        state.xToZ = ua.xToZ;

        state.yToX = ua.yToX;
        state.yToZ = ua.yToZ;

        state.zToX = ua.zToX;
        state.zToY = ua.zToY;

        state.maintainProportions = ua.Maintain;
        state.gravity = ua.Gravity;

        return state;
    }

    public string ToString()
    {
        string msg = "";

        msg += "uuid:" + uuid + "\n";
        msg += "proportions:" + maintainProportions + "\n";
        msg += "gravity:" + gravity + "\n";

        msg += "xToY:" + xToY + "\n";
        msg += "xToZ:" + xToZ + "\n";

        msg += "yToX:" + yToX + "\n";
        msg += "yToZ:" + yToZ + "\n";

        msg += "zToX:" + zToX + "\n";
        msg += "zToY:" + zToY + "\n";

        return msg;
    }

}