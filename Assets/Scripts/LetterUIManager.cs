using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LetterUIManager : MonoBehaviour
{
    public TMP_Text letterDisplay;
    public TMP_Text promptText;
    public Transform optionContainer;
    public GameObject optionButtonPrefab;
    public TMP_Text responseText;

    public void SetLetter(string text)
    {
        letterDisplay.text = text;
    }

    public void AppendLetter(string text)
    {
        letterDisplay.text += text;
    }

    public void ShowPrompt(string prompt, Dictionary<string, CompositionOption> options)
    {
        promptText.text = prompt;
        responseText.text = "";
        foreach (Transform child in optionContainer) Destroy(child.gameObject);

        foreach (var kv in options)
        {
            GameObject btnObj = Instantiate(optionButtonPrefab, optionContainer);
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            btnText.text = $"{kv.Key}) {kv.Value.short_text}";

            Button btn = btnObj.GetComponent<Button>();
            string opt = kv.Key;
            btn.onClick.AddListener(() => GameManager.Instance.SelectOption(opt));
        }
    }

    public void ShowWaiting()
    {
        promptText.text = "Wating response...";
        foreach (Transform child in optionContainer) Destroy(child.gameObject);
    }

    public void ShowResponse(string reply)
    {
        responseText.text = reply;
    }
}
