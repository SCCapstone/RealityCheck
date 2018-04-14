﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using System;
using System.Collections;

public class SaveLoadService : Singleton<SaveLoadService>
{
    private int slot = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int Slot
    {
        get { return slot; }
        set { slot = value; }
    }

    private string FilePath()
    {
        return Application.persistentDataPath + Path.DirectorySeparatorChar + "save" + slot + ".json";
    }

    public IEnumerator Save(int roomId, string roomName, List<GameObject> userAssets)
    {
        var screenshotFn = Application.persistentDataPath + Path.DirectorySeparatorChar + "save" + slot + ".png";
        ScreenCapture.CaptureScreenshot(screenshotFn);

        //Wait for 4 frames
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }


        var states = userAssets.Where(ua => ua != null).Select(ua => UserAssetState.FromGameObject(ua));
        var game = new GameState
        {
            RoomId = roomId
        };

        game.roomName = roomName;
        game.screenshotPNG = GameState.Img2B64(screenshotFn);
        FileUtil.DeleteFileOrDirectory(screenshotFn);

        foreach (var s in states)
        {
            game.Add(s);
        }

        var jsonContent = JsonUtility.ToJson(game);
        var fn = FilePath();

        Debug.Log("Saving Game: " + fn);
        Debug.Log(jsonContent);

        try
        {
            FileUtil.DeleteFileOrDirectory(fn);
        } catch (Exception ex)
        {
            Debug.Log(ex);
        }

        using (var fs = new FileStream(fn, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        {
            using (var writer = new StreamWriter(fs))
            {
                writer.Write(jsonContent);
            }
        }
    }

    public GameState Load(int slot)
    {
        this.slot = slot;
        var fn = FilePath();

        Debug.Log("Loading Game: " + fn);

        var jsonContent = File.ReadAllText(fn);

        Debug.Log(jsonContent);

        return JsonUtility.FromJson<GameState>(jsonContent);
    }

    public GameState Load(string fn)
    {

        Debug.Log("Loading Game: " + fn);

        var jsonContent = File.ReadAllText(fn);

        Debug.Log(jsonContent);

        return JsonUtility.FromJson<GameState>(jsonContent);
    }

    public List<GameState> Saves()
    {
        var jsonFiles = Directory.GetFiles(Application.persistentDataPath + Path.DirectorySeparatorChar, "*.json", SearchOption.AllDirectories).ToList();
        var saveFiles = jsonFiles.Where(f => f.Contains("save")).ToList();
        
        return saveFiles.Select(s => Load(s)).ToList(); ;
    }
}
