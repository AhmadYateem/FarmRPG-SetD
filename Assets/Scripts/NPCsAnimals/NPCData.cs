using System.Collections.Generic;

// persistent data about one NPC — friendship tiers and dialogue sets
[System.Serializable]
public class NPCData
{
    public string npcName;
    public int friendshipPoints;  // 0-100

    // dialogue root changes based on friendship tier
    public DialogueNode lowFriendshipDialogue;   // 0-33
    public DialogueNode midFriendshipDialogue;   // 34-66
    public DialogueNode highFriendshipDialogue;  // 67-100

    public DialogueNode GetDialogueForCurrentTier()
    {
        if (friendshipPoints >= 67 && highFriendshipDialogue != null)
            return highFriendshipDialogue;
        if (friendshipPoints >= 34 && midFriendshipDialogue != null)
            return midFriendshipDialogue;
        return lowFriendshipDialogue;
    }
}
