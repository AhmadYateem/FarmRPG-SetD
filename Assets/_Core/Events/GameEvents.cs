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
}
