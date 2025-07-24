using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LetterUIManager : MonoBehaviour
{
    public GameObject letterAlertIcon;
    [ReadOnly] public bool isOpen;

    [Header("Letter UI")]
    public Canvas letterCanvas;
    public TMP_Text letterText;
    public GameObject letterObject;

    [Header("Options")]
    public HorizontalOrVerticalLayoutGroup promptArea;
    public TMP_InputField promptInput;


    private void Start()
    {
        toggleCanvas(false);
        togglePromptArea(false);
    }

    private void Update()
    {
        // Mostrem la icona de 
        if (letterAlertIcon != null)
            letterAlertIcon.SetActive(GameManager.Instance.CurrentState == GameState.ResponseReceived);
    }

    public void OpenLetter()
    {
        if (isOpen)
            return;

        // GameManager.Instance.BeginWriting();
        toggleCanvas(true);
        Debug.Log("Letter Opened");
    }

    public void CloseLetter()
    {
        if (!isOpen)
            return;

        // GameManager.Instance.SubmitLetter();
        toggleCanvas(false);
        Debug.Log("Letter Closed");
    }

    public void setLetterText(string text)
    {
        letterText.text = text;
    }

    private void toggleCanvas(bool state)
    {
        letterCanvas.enabled = state;
    }

    private void togglePromptArea(bool state)
    {
        promptArea.gameObject.SetActive(state);
    }
}
