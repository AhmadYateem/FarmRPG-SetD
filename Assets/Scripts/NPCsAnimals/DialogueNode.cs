using System.Collections.Generic;

// one "bubble" in the conversation tree
// marked Serializable so Unity can display it in the Inspector
[System.Serializable]
public class DialogueNode
{
    [UnityEngine.TextArea(2, 5)]
    public string npcText;                  // what the NPC says

    public List<DialogueChoice> choices;    // player's response options

    // no choices means end of conversation
    public bool IsEndNode()
    {
        return choices == null || choices.Count == 0;
    }
}
