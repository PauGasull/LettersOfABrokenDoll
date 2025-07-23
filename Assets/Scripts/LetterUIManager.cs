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
    public GameObject promptArea;
    public GameObject promptContainer;
    public Input promptInput;

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
        Debug.Log("Letter Opened");
    }

    public void CloseLetter()
    {
        if (!isOpen)
            return;

        // GameManager.Instance.SubmitLetter();
        Debug.Log("Letter Closed");
    }

    // Detectar quan l'user escull opcio i 
}
