using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shared event hub — all cross-team communication goes through here.
/// Every SET should use these events instead of direct references.
/// </summary>
public class GameEvents : MonoBehaviour
{
    // dialogue events — SET D fires these when NPC talks
    public static UnityEvent OnDialogueStart = new UnityEvent();
    public static UnityEvent OnDialogueEnd   = new UnityEvent();

    // time event — SET C fires this, we subscribe in NPCSchedule (milestone 3)
    public static UnityEvent<int> OnTimeChanged = new UnityEvent<int>();

    // day advancement — mock system fires this each "new day"
    public static UnityEvent OnDayAdvanced = new UnityEvent();

    // friendship — fired when any NPC friendship value changes (npcName, newValue)
    public static UnityEvent<string, int> OnFriendshipChanged = new UnityEvent<string, int>();

    // animal interaction events
    public static UnityEvent<string> OnAnimalFed    = new UnityEvent<string>();
    public static UnityEvent<string> OnAnimalPetted = new UnityEvent<string>();

    // product collected — fired when egg/milk/wool is collected (productName)
    public static UnityEvent<string> OnProductCollected = new UnityEvent<string>();
}
