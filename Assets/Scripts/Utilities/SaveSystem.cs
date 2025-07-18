using System.IO;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameStateData
{
    public string compositionId;
    public string blockId;
    public List<string> path;
    public string letter;
    public GameState state;
}

public static class SaveSystem
{
    static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void SaveGame()
    {
        GameStateData data = new GameStateData
        {
            compositionId = GameManager.Instance.currentCompositionId,
            blockId = GameManager.Instance.currentBlockId,
            path = GameManager.Instance.choicePath,
            letter = GameManager.Instance.letterBuffer,
            state = GameManager.Instance.CurrentState
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static void LoadGame()
    {
        if (!File.Exists(SavePath)) return;
        string json = File.ReadAllText(SavePath);
        GameStateData data = JsonUtility.FromJson<GameStateData>(json);
        GameManager.Instance.LoadState(data);
    }
}
