using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    WritingLetter,
    WaitingResponse,
    ReadingResponse
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    public string currentCompositionId = "COM001";
    public CompositionTemplate currentTemplate;
    public string currentBlockId;
    public List<string> choicePath = new();
    public string letterBuffer = "";

    public LetterUIManager letterUI;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGame()
    {
        currentTemplate = LetterDataLoader.Instance.compositionData[currentCompositionId];
        currentBlockId = currentTemplate.root_block;
        choicePath.Clear();
        letterBuffer = "";
        CurrentState = GameState.WritingLetter;
        letterUI.ShowPrompt(GetCurrentPrompt(), GetCurrentOptions());
    }

    public void SelectOption(string optionId)
    {
        var option = currentTemplate.blocks[currentBlockId].options[optionId];
        choicePath.Add(optionId);
        letterBuffer += option.long_text;

        letterUI.AppendLetter(option.long_text);

        if (option.next == null)
        {
            SubmitLetter();
        }
        else
        {
            currentBlockId = option.next;
            letterUI.ShowPrompt(GetCurrentPrompt(), GetCurrentOptions());
        }
    }

    public string GetCurrentPrompt() => currentTemplate.blocks[currentBlockId].prompt;
    public Dictionary<string, CompositionOption> GetCurrentOptions() => currentTemplate.blocks[currentBlockId].options;

    void SubmitLetter()
    {
        CurrentState = GameState.WaitingResponse;
        letterUI.ShowWaiting();

        float delay = LetterDataLoader.Instance.letterMetaData[currentCompositionId].delay_seconds;
        Invoke(nameof(DeliverResponse), delay);
        SaveSystem.SaveGame();
    }

    void DeliverResponse()
    {
        CurrentState = GameState.ReadingResponse;
        string pathKey = string.Join("_", choicePath);
        var responseTemplate = LetterDataLoader.Instance.responseData[currentCompositionId];

        if (responseTemplate.paths.TryGetValue(pathKey, out var responsePath))
        {
            string fullReply = "";
            foreach (string blockId in responsePath.blocks)
            {
                if (responseTemplate.responses.TryGetValue(blockId, out var resp))
                    fullReply += resp.content + "\n";
            }
            letterUI.ShowResponse(fullReply);
        }
        else
        {
            letterUI.ShowResponse("NO ANSWER");
        }
    }

    public void LoadState(GameStateData data)
    {
        currentCompositionId = data.compositionId;
        currentTemplate = LetterDataLoader.Instance.compositionData[currentCompositionId];
        currentBlockId = data.blockId;
        choicePath = new List<string>(data.path);
        letterBuffer = data.letter;
        CurrentState = data.state;

        letterUI.SetLetter(letterBuffer);

        if (CurrentState == GameState.WritingLetter)
            letterUI.ShowPrompt(GetCurrentPrompt(), GetCurrentOptions());
        else if (CurrentState == GameState.WaitingResponse)
            letterUI.ShowWaiting();
        else
            DeliverResponse(); // o bé recuperar resposta guardada
    }
}
