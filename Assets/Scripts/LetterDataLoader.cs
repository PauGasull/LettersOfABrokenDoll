using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class LetterDataLoader : MonoBehaviour
{
    public static LetterDataLoader Instance { get; private set; }

    public Dictionary<string, CompositionTemplate> compositionData;
    public Dictionary<string, LetterMeta> letterMetaData;
    public Dictionary<string, ResponseTemplate> responseData;

    [Header("Assets/")]
    public string jsonFolder = "letters";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        LoadAllData();
    }

    void LoadAllData()
    {
        string basePath = Path.Combine(Application.dataPath, jsonFolder);

        compositionData = LoadFromJsonFile<Dictionary<string, CompositionTemplate>>(Path.Combine(basePath, "composition.json"));
        letterMetaData = LoadFromJsonFile<Dictionary<string, LetterMeta>>(Path.Combine(basePath, "letter.json"));
        responseData = LoadFromJsonFile<Dictionary<string, ResponseTemplate>>(Path.Combine(basePath, "responses.json"));
    }

    T LoadFromJsonFile<T>(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"JSON file not found at: {path}");
            return default;
        }

        string json = File.ReadAllText(path);
        Debug.Log($"JSON file ({path}) loaded");
        return JsonConvert.DeserializeObject<T>(json);
    }
}
