using UnityEngine;

/// <summary>
/// Handles NPC dialogue — stores lines in an array and cycles through them.
/// Fires GameEvents so other systems know when dialogue starts/ends.
/// </summary>
public class NPCDialogue : MonoBehaviour
{
    // fill these from the Inspector — TA said at least 4 lines
    // like "Hello!", "Good morning!", "We're closed.", "Come back tomorrow!"
    [SerializeField] private string[] _dialogueLines;

    private int  _currentLine = 0;
    private bool _isActive    = false;

    // call this to start talking to the NPC
    public void StartDialogue()
    {
        _currentLine = 0;
        _isActive    = true;
        Debug.Log(_dialogueLines[_currentLine]);
        GameEvents.OnDialogueStart.Invoke();
    }

    // moves to next line, loops back to start when it reaches the end
    public void NextLine()
    {
        if (!_isActive) return;

        _currentLine = (_currentLine + 1) % _dialogueLines.Length;
        Debug.Log(_dialogueLines[_currentLine]);
    }

    // ends the conversation
    public void EndDialogue()
    {
        _isActive = false;
        GameEvents.OnDialogueEnd.Invoke();
    }
}
