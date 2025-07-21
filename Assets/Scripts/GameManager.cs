using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
            return;
        }
        // Assignem la instància
        Instance = this;
    }

    /***
    * StartNewGame(): Inicia una nova partida o carrega l'estat desat
    * PRE: --
    * POST: Si hi ha una partida desada, la carrega; altrament, inicialitza una de nova
    ***/
    public void StartNewGame()
    {
        // Carreguem la partidam, si funciona no fem res.
        if (SaveSystem.LoadGame())
            return;

        currentTemplate = LetterDataLoader.Instance.compositionData[currentCompositionId]; // Carreguem la template des del data loader
        currentBlockId = currentTemplate.root_block; // Posem el block actual a l'arrel delcomposition

        choicePath.Clear(); // Netejem cualsevol historial de opcions. 
        letterBuffer = ""; // Fem un resset del Buffer

        CurrentState = initialState; // Posem el joc en estat inicial
    }

    /***
    * SelectOption(): Gestiona l'opció triada pel jugador
    * PRE: optionId és vàlid (A, B, C...)
    * POST: Actualitza path, buffer i avança al següent bloc o envia la carta
    ***/
    public void SelectOption(string optionId)
    {
        // Obtenim la opcio seleccionada a partir del block actual
        var option = currentTemplate.blocks[currentBlockId].options[optionId];
        // Add the chosen option's ID to the path history
        choicePath.Add(optionId); // Afegim la 
        // Append the option's long text to the letter buffer
        letterBuffer += option.long_text;

        // If there is no next block, submit the letter; otherwise advance to next block
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
        return currentTemplate.blocks[currentBlockId].prompt;
    }

    /***
    * GetCurrentOptions(): Retorna les opcions disponibles del bloc actual
    ***/
    public Dictionary<string, CompositionOption> GetCurrentOptions()
    {
        return currentTemplate.blocks[currentBlockId].options;
    }

    /***
    * SubmitLetter(): Envia la carta i programa la resposta
    * PRE: --
    * POST: Canvia l'estat a WaitingResponse i desa la partida
    ***/
    void SubmitLetter()
    {
        CurrentState = GameState.WaitingResponse; // Cambiem l'estat a Waiting Response 
        float delay = LetterDataLoader.Instance.letterMetaData[currentCompositionId].delay_seconds; // obtenim el delay
        Invoke(nameof(DeliverResponse), delay); // 'Rebem' Carta despres del delay

        // Guardem la partida
        SaveSystem.SaveGame();
    }

    /***
    * DeliverResponse(): Mostra la resposta un cop passat el delay
    * PRE: --
    * POST: Concatena i mostra la resposta corresponent al path
    ***/
    void DeliverResponse()
    {
        // Creem la clau del Path
        string pathKey = string.Join("_", choicePath);
        var responseTemplate = LetterDataLoader.Instance.responseData[currentCompositionId]; // Obtenim la resposta segons el path

        if (responseTemplate.paths.TryGetValue(pathKey, out var responsePath)) // Si hi ha path
        {
            string fullReply = "";
            foreach (string blockId in responsePath.blocks) // Fer cada block dins de la resposta
                if (responseTemplate.responses.TryGetValue(blockId, out var resp)) // controlem que existeixi 
                    fullReply += resp.content + "\n"; // concatenem
        }
        else
            Debug.warning($"No path {{pathKey}} found");
    }

    /***
    * LoadState(): Aplica un estat carregat al joc
    * PRE: data obtinguda de SaveSystem.LoadGame()
    * POST: S'ajusta tot l'estat intern i s'actualitza la UI
    ***/
    public void LoadState(GameStateData data)
    {
        currentCompositionId = data.compositionId; // Obtenim la ID de la carta
        currentTemplate = LetterDataLoader.Instance.compositionData[currentCompositionId]; // Obtenim la template

        currentBlockId = data.blockId; // Obtenim la id del Block
        choicePath = new List<string>(data.path); // Restaurem l'historial de Respostes
        letterBuffer = data.letter; // Restaurem el contingut de la carta en el buffer
        CurrentState = data.state; // Establim l'estat acual del joc

/*
        
        // Update the UI with the restored letter buffer
        letterUI.SetLetter(letterBuffer);

        // Show the correct UI based on the restored state
        if (CurrentState == GameState.WritingLetter)
            letterUI.ShowPrompt(GetCurrentPrompt(), GetCurrentOptions());
        else if (CurrentState == GameState.WaitingResponse)
            letterUI.ShowWaiting();
        else
            DeliverResponse();
*/
    }
}
