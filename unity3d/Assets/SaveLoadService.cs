using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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

    public void Save(int roomId, List<GameObject> userAssets)
    {
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + Path.DirectorySeparatorChar + "save" + slot + ".png");

        var states = userAssets.Select(ua => UserAssetState.FromGameObject(ua));
        var game = new GameState
        {
            RoomId = roomId
        };

        foreach (var s in states)
        {
            game.Add(s);
        }

        var jsonContent = JsonUtility.ToJson(game);
        var fn = FilePath();

        Debug.Log("Saving Game: " + fn);
        Debug.Log(jsonContent);

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
        var fn = FilePath();

        Debug.Log("Loading Game: " + fn);

        var jsonContent = File.ReadAllText(fn);

        Debug.Log(jsonContent);

        return JsonUtility.FromJson<GameState>(jsonContent);
    }
}
