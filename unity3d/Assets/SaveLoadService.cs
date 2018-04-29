using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Collections;

public class SaveLoadService : Singleton<SaveLoadService>
{
    private static string SAVE_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "RealityCheck" + Path.DirectorySeparatorChar;
    private int slot = 0;

    // Use this for initialization
    void Start()
    {
        try
        {
            if (!Directory.Exists(SAVE_FOLDER))
            {
                Directory.CreateDirectory(SAVE_FOLDER);
            }
        }
        catch (IOException ex)
        {
            Debug.Log(ex);
        }
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
        return SAVE_FOLDER + "save" + slot + ".json";
    }

    public void Save(int roomId, string roomName, List<GameObject> userAssets)
    {
        var screenshotFn = SAVE_FOLDER + "save" + slot + ".png";

        if (File.Exists(screenshotFn))
        {
            File.Delete(screenshotFn);
        }

        ScreenCapture.CaptureScreenshot(screenshotFn);
        
        var states = userAssets.Where(ua => ua != null).Select(ua => UserAssetState.FromGameObject(ua));
        var game = new GameState
        {
            RoomId = roomId
        };
        
        game.roomName = roomName;
        
        foreach (var s in states)
        {
            game.Add(s);
        }
        
        var jsonContent = JsonUtility.ToJson(game);
        var fn = FilePath();
        
        Debug.Log("Saving Game: " + fn);
        Debug.Log(jsonContent);

        if (File.Exists(fn))
        {
            File.Delete(fn);
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
        var jsonFiles = Directory.GetFiles(SAVE_FOLDER, "*.json", SearchOption.AllDirectories).ToList();
        var saveFiles = jsonFiles.Where(f => f.Contains("save")).ToList();
        
        return saveFiles.Select(s => Load(s)).ToList(); ;
    }
}
