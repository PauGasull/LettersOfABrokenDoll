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
    // Utilitzem una extensió custon .loabd
    static string SavePath => Path.Combine(Application.persistentDataPath, "save.loabd");

    static const string HEADER = "Letters of a Broken Doll - Save File (v1) \n";
    static const string WARNING = "# DO NOT EDIT MANUALLY # \n";
    static const string FOOTER = "== END OF SAVE ==";

    public static void SaveGame()
    {
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
            json + "\n" + // CONTINGUT
            FOOTER;

        // Guardem l'arxiu com a fitxer a SavePath
        File.WriteAllText(SavePath, wrapped);
        Debug.Log("Game State saved on: " + SavePath);
    }

    public static bool LoadGame()
    {
        // Si el fixter no existeix, no tenim res a carregar
        if (!File.Exists(SavePath))
            return false;

        // Llegim totes les linies del fitxer 
        string[] lines = File.ReadAllLines(SavePath);

        // Trobem l'index del warning, el JSON comença a la seguent linia
        int start = Array.IndexOf(lines, WARNING) + 1;
        int end = Array.IndexOf(lines, FOOTER); // Acaba a l'index del FOOTER

        // Extreiem el JSON entre start <-> end
        string json = string.Join("\n", lines, start, end - start);

        // Convertim el JSON a un data object
        GameStateData data = JsonUtility.FromJson<GameStateData>(json);

        // Appliquem l'estat
        GameManager.Instance.LoadState(data);
        return true;
    }
}
