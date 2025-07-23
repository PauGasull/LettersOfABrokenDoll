using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameStateData
{
    public string compositionId; // ID de la composició
    public string blockId;       // Bloc actual
    public List<string> path;    // Camí d'opcions escollides
    public string letter;        // Contingut acumulat de la carta
    public GameState state;      // Estat del joc en el moment de desar
}

public static class SaveSystem
{
    // Utilitzem una extensió custom .loabd
    static string SavePath => Path.Combine(Application.persistentDataPath, "save.loabd");

    const string HEADER  = "Letters of a Broken Doll - Save File (v1) \n";
    const string WARNING = "# DO NOT EDIT MANUALLY # \n";
    const string FOOTER  = "== END OF SAVE ==";

    /***
    * DeleteAllSaveFiles(): Elimina tots els fitxers de desament
    * PRE: --
    * POST: S'esborra save.loabd si existeix
    ***/
    public static void DeleteAllSaveFiles()
    {
        // Si el fitxer existeix, l'eliminem
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    /***
    * SaveGame(): Desa l'estat del joc en un fitxer custom
    * PRE: GameManager.Instance no és null
    * POST: S'escriu save.loabd amb l'estat actual, si deleteSaveState està desactivat
    ***/
    public static void SaveGame()
    {
        // Si deleteSaveState està actiu, no guardem cap partida
        if (GameManager.Instance != null && GameManager.Instance.deleteSaveState)
            return;

        // Creaem un data object obtenint l'estat actual de la partida
        GameStateData data = new GameStateData
        {
            compositionId = GameManager.Instance.currentCompositionId,
            blockId = GameManager.Instance.currentBlockId,
            path = GameManager.Instance.choicePath,
            letter = GameManager.Instance.letterBuffer,
            state = GameManager.Instance.CurrentState
        };

        // Serialitzem les dades en un JSON
        string json = JsonUtility.ToJson(data, true);

        // Afegim linies de control
        string wrapped =
            HEADER +
            WARNING +
            json + "\n" +
            FOOTER;

        // Guardem l'arxiu a SavePath
        File.WriteAllText(SavePath, wrapped);
        Debug.Log("Game State saved on: " + SavePath);
    }

    /***
    * LoadGame(): Carrega l'estat del joc de save.loabd
    * PRE: --
    * POST: Si deleteSaveState està actiu, esborra el fitxer i retorna false;
    *       Si el fitxer no existeix, retorna false;
    *       Altrament, llegeix, parseja i aplica l'estat
    ***/
    public static bool LoadGame()
    {
        // si deleteSaveState està actiu eliminem l'arxiu 
        if (GameManager.Instance != null && GameManager.Instance.deleteSaveState)
        {
            DeleteAllSaveFiles();
            return false;
        }

        // Si no hi ha res a carregar...
        if (!File.Exists(SavePath))
            return false;

        // Llegim tot el fitxer com a text
        string content = File.ReadAllText(SavePath);

        // Trobem el primer '{' i l'ultim '}' per delimitar el JSON
        int start = content.IndexOf('{');
        int end   = content.LastIndexOf('}');
        if (start < 0 || end < 0 || end <= start)
        {
            Debug.LogError("Could not locate JSON in save file");
            return false;
        }

        // Extraiem el json
        string json = content.Substring(start, end - start + 1);

        // Parse JSON
        try
        {
            GameStateData data = JsonUtility.FromJson<GameStateData>(json);
            GameManager.Instance.LoadState(data);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse save JSON: {ex.Message}");
            return false;
        }
    }

}
