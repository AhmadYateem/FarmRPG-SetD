// one choice button the player can click
[System.Serializable]
public class DialogueChoice
{
    public string choiceText;       // text on the button e.g. "I want to buy something"
    [UnityEngine.SerializeReference]
    public DialogueNode nextNode;   // which node this leads to
}
