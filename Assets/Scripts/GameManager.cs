using System.Collections.Generic;
using UnityEngine;

// Estat general del joc
public enum GameState
{
    WritingLetter,   // L'usuari ha d'escriure la carta
    WaitingResponse, // L'usuari està esperant una carta
    ResponseRecived, // L'usuari ha rebut una carta, no l'ha llegit encara
    ReadingResponse  // L'usuari està llegint la carta
}

// TO DO: Letter UI Connection
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Instància singleton per accés global
    public GameState CurrentState { get; private set; }      // Estat actual del joc

    public GameState initialState = GameState.WaitingResponse; // Estat inicial en començar una nova partida
    public string currentCompositionId = "COM001";             // ID de la composició JSON actual
    public CompositionTemplate currentTemplate;                // Dades de la composició carregada
    public string currentBlockId;                              // Bloc actual dins la composició

    public List<string> choicePath = new(); // Seqüència d'opcions triades pel jugador
    public string letterBuffer = "";        // Buffer de text de la carta

    public bool deleteSaveState; // Flag per eliminar i desactivar els saves

    /***
    * Awake(): Preparem el Sigleton
    * PRE: --
    * POST: Garantim que només hi hagi una instància de GameManager
    ***/
    private void Awake()
    {
        // Si ja hi ha una instància, eliminem el duplicat
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.Log("Sigleton Forced");
            return;
        }
        // Assignem la instància
        Instance = this;
    }

    /***
    * Start(): Iniciem la partida
    ***/
    private void Start()
    {
        StartNewGame();
    }

    /***
    * StartNewGame(): Inicia una nova partida o carrega l'estat desat
    * PRE: --
    * POST: Si deleteSaveState és actiu esborra saves; si hi ha una partida desada, la carrega; 
    *       altrament, inicialitza una de nova i envia la primera carta
    ***/
    public void StartNewGame()
    {
        // Si deleteSaveState està actiu, eliminem tots els fitxers de desament
        if (deleteSaveState)
            SaveSystem.DeleteAllSaveFiles();

        // Carreguem la partida; si funciona, no fem res més
        if (SaveSystem.LoadGame())
            return;

        Debug.Log("No Game Loaded");

        // Només carreguem template si existeix (és una carta del jugador)
        if (LetterDataLoader.Instance.compositionData.ContainsKey(currentCompositionId))
        {
            currentTemplate = LetterDataLoader.Instance.compositionData[currentCompositionId];
            currentBlockId = currentTemplate.root_block;
            CurrentState = GameState.WritingLetter;
        }
        else
        {
            // Si no hi ha composició, vol dir que és una carta automàtica de Ku'umi
            CurrentState = GameState.WaitingResponse;
            SubmitLetter();
        }
        
        choicePath.Clear(); // Netejem qualsevol historial d'opcions
        letterBuffer = ""; // Resetejem el buffer de la carta

        // Posem el joc en estat inicial
        CurrentState = initialState;
        Debug.Log("Current State: " + CurrentState);

        // Enviem la primera carta sense necessitat de path
        SubmitLetter();
    }

    /***
    * SelectOption(): Gestiona l'opció triada pel jugador
    * PRE: optionId és vàlid (A, B, C...)
    * POST: Actualitza path, buffer i avança al següent bloc o envia la carta
    ***/
    public void SelectOption(string optionId)
    {
        // Obtenim l'opció seleccionada a partir del bloc actual
        var option = currentTemplate.blocks[currentBlockId].options[optionId];
        // Afegim la ID de l'opció al historial de camí
        choicePath.Add(optionId);
        // Afegim el text llarg de l'opció al buffer de la carta
        letterBuffer += option.long_text;

        // Si no hi ha següent bloc, enviem la carta; altrament, avancem al proper bloc
        if (option.next == null)
            SubmitLetter();
        else
            currentBlockId = option.next;
    }

    /***
    * GetCurrentPrompt(): Retorna el text del prompt del bloc actual
    ***/
    public string GetCurrentPrompt()
    {
        // Retornem la string del prompt del bloc actual
        return currentTemplate.blocks[currentBlockId].prompt;
    }

    /***
    * GetCurrentOptions(): Retorna les opcions disponibles del bloc actual
    ***/
    public Dictionary<string, CompositionOption> GetCurrentOptions()
    {
        // Retornem el diccionari d'opcions per al bloc actual
        return currentTemplate.blocks[currentBlockId].options;
    }

    /***
    * SubmitLetter(): Envia la carta i programa la resposta
    * PRE: --
    * POST: Canvia l'estat a WaitingResponse i desa la partida (si deleteSaveState és fals)
    ***/
    void SubmitLetter()
    {
        // Canviem l'estat a WaitingResponse
        CurrentState = GameState.WaitingResponse;
        // Obtenim el delay definit a la metadata
        float delay = LetterDataLoader.Instance.letterMetaData[currentCompositionId].delay_seconds;
        // Programem la invocació de DeliverResponse després del delay
        Invoke(nameof(DeliverResponse), delay);
    }

    /***
    * DeliverResponse(): Mostra la resposta un cop passat el delay
    * PRE: --
    * POST: Canvia l'estat a ResponseRecived i concatena la resposta corresponent al path
    ***/
    void DeliverResponse()
    {
        // Canviem l'estat a ResponseRecived perquè s'ha rebut una carta
        CurrentState = GameState.ResponseRecived;

        // Creem la clau del path buscant amb underscore
        string pathKey = string.Join("_", choicePath);
        // Obtenim la plantilla de resposta segons la composició actual
        var responseTemplate = LetterDataLoader.Instance.responseData[currentCompositionId];

        if (responseTemplate.paths.TryGetValue(pathKey, out var responsePath))
        {
            // Concatenem el contingut de cada bloc de la resposta
            string fullReply = "";
            foreach (string blockId in responsePath.blocks)
                if (responseTemplate.responses.TryGetValue(blockId, out var resp))
                    fullReply += resp.content + "\n";

            /* letterUI.ShowResponse(fullReply);*/

            // Mostrem la carta rebuda per consola
            Debug.Log($"Letter Recived:\n{fullReply}");


        }
        else
            Debug.LogWarning($"No path '{pathKey}' found");

        CurrentState = GameState.ReadingResponse;
        Debug.Log("Current State: " + CurrentState);
                
        // Guardem la partida
        SaveSystem.SaveGame();
    }

    /***
    * LoadState(): Aplica un estat carregat al joc
    * PRE: data obtinguda de SaveSystem.LoadGame()
    * POST: Restaura l'estat intern i actualitza la UI
    ***/
    public void LoadState(GameStateData data)
    {
        // Restaurem la ID de la composició
        currentCompositionId = data.compositionId;
        // Recarreguem la plantilla de la composició
        currentTemplate = LetterDataLoader.Instance.compositionData[currentCompositionId];

        currentBlockId = data.blockId; // Restaurem el bloc actual
        choicePath = new List<string>(data.path); // Restaurem l'historial de resposte
        letterBuffer = data.letter; // Restaurem el buffer de la carta
        CurrentState = data.state; // Restaurem l'estat del joc

        /* letterUI.SetLetter(letterBuffer);

        if (CurrentState == GameState.WritingLetter)
            letterUI.ShowPrompt(GetCurrentPrompt(), GetCurrentOptions());
        else if (CurrentState == GameState.WaitingResponse)
            letterUI.ShowWaiting();
        else
            DeliverResponse();*/
    }
}
