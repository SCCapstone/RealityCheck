using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class GameState
{
    public int roomId = 0;
    public string roomName;
    public string screenshotPNG;

    [SerializeField]
    public List<UserAssetState> assets = new List<UserAssetState>();

   public int RoomId
    {
        get { return roomId; }
        set { roomId = value; }
    }
    
	public GameState()
	{
	}

    public void Add(UserAssetState asset)
    {
        assets.Add(asset);
    }

    public static string Img2B64(string fn)
    {
        var pngContent = File.ReadAllText(fn);
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(pngContent));

        return b64;
    }

    public static byte[] B642Img(string png64)
    {
        return Convert.FromBase64String(png64);
    }
}
