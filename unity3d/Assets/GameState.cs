using System;
using UnityEngine;

[Serializable]
public class GameState
{
    private int roomId = 0;
    private List<UserAssetState> assets = new List<UserAssetState>();

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

}
