using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField]
    GameObject dialogBox;

    [SerializeField]
    Text dialogText;

    [SerializeField]
    int lettersPerSecond;

    public static DialogManager Instance { get; private set; }

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    private void Awake()
    {
        Instance = this;
    }

    Dialog dialog;
    Action onDialogFinished;
    int currentLine = 0;
    bool isTyping;

    public bool isShowing;

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished = null)
    {
        yield return new WaitForEndOfFrame();

        isShowing = true;
        OnShowDialog?.Invoke();
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true); //shows box
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
        {
            ++currentLine;
            if (currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else
            {
                currentLine = 0; //sets line to zero
                isShowing = false;
                dialogBox.SetActive(false); //Close Box
                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke(); //OnCloseDialog
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = " "; //empty string
        foreach (var letter in line.ToCharArray()) //looping one by one letter
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }
}
