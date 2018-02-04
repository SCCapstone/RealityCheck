using NUnit.Framework;
using UnityEngine;

public class UserAssetTests
{
    [Test]
    public void _Maintain_Locks_Scale()
    {
        var gameObject = new GameObject();

        gameObject.AddComponent<userAsset>();
        var au = gameObject.GetComponent<userAsset>();

        au.Maintain = false;
        var scaleVec = new Vector3(1.1f, 1.0f, 1.0f);
        au.Scale(scaleVec);

        Assert.AreEqual(au.transform.localScale, scaleVec);
    }
}