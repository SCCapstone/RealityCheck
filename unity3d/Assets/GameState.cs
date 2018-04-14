using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    public int roomId = 0;

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

}
