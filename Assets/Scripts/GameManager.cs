using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

// Estat general del joc
public enum GameState
{
    WaitingResponse,    // 1. Esperant la resposta
    ResponseReceived,   // 2. Resposta rebuda però no llegida
    PendingWriteLetter, // 3. Pendent escriure una carta
    WritingLetter,      // 4. Escrivint la carta
    ReadingResponse     // 5. Llegint la resposta
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Instància singleton per accés global
    public GameState CurrentState { get; private set; } // Estat actual del joc

    public string currentCompositionId = "COM001"; // ID de la carta o composició actual
    public CompositionTemplate currentTemplate; // Dades de la composició carregada
    public string currentBlockId; // ID del bloc actual dins la composició

    public List<string> choicePath = new(); // Seqüència d'opcions escollides
    [ReadOnly]
    public string letterBuffer = ""; // Text acumulat de la carta del jugador
    public LetterUIManager letterUIManager;

    public bool deleteSaveState; // Flag per esborrar la partida

    /***
    * Awake(): Prepara el singleton
    * PRE: --
    * POST: Només queda una instància de GameManager
    ***/
    private void Awake()
    {
        if (Instance != null) // si ja tenim una instància
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /***
    * Start(): Inicialitza les referencies. Inicia la partida, nova o carregada
    ***/
    private void Start() { 
        // Referenciem    
        if(letterUIManager == null)
            letterUIManager = gameObject.GetComponent<LetterUIManager>();

        // Iniciem el joc
        StartNewGame(); 
    }

    /***
    * StartNewGame(): Carrega o inicialitza nova partida
    * PRE: --
    * POST: Carrega estat o envia primera carta
    ***/
    public void StartNewGame()
    {
        if (deleteSaveState) // si està marcat, eliminem les partides guardades
            SaveSystem.DeleteAllSaveFiles();

        if (SaveSystem.LoadGame())
            return;  // Estat carregat, surt

        // Neteja historial per nova partida
        choicePath.Clear();
        letterBuffer = string.Empty;

        // Comença esperant la primera carta
        CurrentState = GameState.WaitingResponse;
        SubmitLetter();
    }

    /***
    * SubmitLetter(): Envia la carta (o sol·licitud) i programa la resposta
    * PRE: --
    * POST: Estat passa a WaitingResponse i invoca DeliverResponse després del delay
    ***/
    private void SubmitLetter()
    {
        CurrentState = GameState.WaitingResponse; // Canvi d'estat
        float delay = LetterDataLoader.Instance.letterMetaData[currentCompositionId].delay_seconds; // Obtenim el delay
        Invoke(nameof(DeliverResponse), delay); // Invoquem passat el temps
    }

    /***
    * DeliverResponse(): Rebre la resposta
    * PRE: Estat WaitingResponse
    * POST: Estat passa a ResponseReceived, es mostra resposta, després ReadingResponse
    ***/
    private void DeliverResponse()
    {
        // Rebem la resposta
        CurrentState = GameState.ResponseReceived;

        string pathKey = string.Join("_", choicePath); // Ajuntem les opcions
        var template = LetterDataLoader.Instance.responseData[currentCompositionId]; // Busquem la template
        string fullReply = string.Empty; // inicialitzem resposta

        if (template.paths.TryGetValue(pathKey, out var responsePath)) // si obtenim valor...
        {
            foreach (var blockId in responsePath.blocks) // Per cada bloc de la resposta
            {
                if (template.responses.TryGetValue(blockId, out var resp)) // si es accessible
                    fullReply += resp.content + "\n"; // concatenem
            }
        }
        else
        {
            Debug.LogWarning($"No s'ha trobat path '{pathKey}'"); // Si no, mostrem un avís
            fullReply = "NO PATH FOUND";
        }

        Debug.Log("Carta Rebuda:\n" + $"{fullReply}".Size(13).Italic());
        letterUIManager.setLetterText(fullReply); // letterUI.ShowResponse(fullReply);

        // CurrentState = GameState.ReadingResponse;
        SaveSystem.SaveGame(); // guardem partida
    }

    
    /***
    * FinishReading(): S'activa quan l'usuari tanca la lectura
    * PRE: Estat ReadingResponse
    * POST: Estat passa a PendingWriteLetter
    ***/
    public void FinishReading()
    {
        if (CurrentState == GameState.ReadingResponse)
            CurrentState = GameState.PendingWriteLetter;
    }

    /***
    * BeginWriting(): Prepara l'escriptura de la següent carta
    * PRE: Estat PendingWriteLetter
    * POST: Carrega composició i estat WritingLetter
    ***/
    public void BeginWriting(string compositionId)
    {
        if (!LetterDataLoader.Instance.compositionData.ContainsKey(compositionId))
        {
            Debug.LogError($"No composition per {compositionId}".Bold().Color("#fa8b8bff"));
            return;
        }

        currentCompositionId = compositionId;
        currentTemplate = LetterDataLoader.Instance.compositionData[compositionId];
        currentBlockId = currentTemplate.root_block;
        choicePath.Clear();
        letterBuffer = string.Empty;
        CurrentState = GameState.WritingLetter;
    }

    /***
    * SelectOption(): Afegeix opció, acumula text i envia si és l'última
    * PRE: Estat WritingLetter
    * POST: Pot cridar SubmitLetter o avançar bloc
    ***/
    public void SelectOption(string optionId)
    {
        // Si no estem en estat d'escriptura, no fem res
        if (CurrentState != GameState.WritingLetter)
            return; // Sortim de la funció

        // Obtenim l'objecte CompositionOption corresponent a l'ID d'opció
        var option = currentTemplate.blocks[currentBlockId].options[optionId];
        choicePath.Add(optionId); // Afegim l'ID de l'opció al camí de respostes
        letterBuffer += option.long_text; // Afegim el text llarg de l'opció al buffer de la carta

        // Si no hi ha bloc següent
        if (option.next == null)
            SubmitLetter(); // enviem
        else // si hi ha
            currentBlockId = option.next; // Anem al següent bloc
    }

    /***
    * LoadState(): Aplica l'estat carregat des de JSON
    * PRE: data ha estat parsejat correctament
    * POST: Restaura tots els camps del GameManager i l'estat de joc
    ***/
    public void LoadState(GameStateData data)
    {
        // Restaurem la ID de composició
        currentCompositionId = data.compositionId;

        // Si existeix composició, carreguem la plantilla i bloc
        if (LetterDataLoader.Instance.compositionData.ContainsKey(currentCompositionId))
        {
            currentTemplate = LetterDataLoader.Instance.compositionData[currentCompositionId];
            currentBlockId = data.blockId;
        }

        // Restaurem el camí de respostes i buffer de text
        choicePath = new List<string>(data.path);
        letterBuffer = data.letter;

        // Restaurem l'estat del joc
        CurrentState = data.state;
    }
}
