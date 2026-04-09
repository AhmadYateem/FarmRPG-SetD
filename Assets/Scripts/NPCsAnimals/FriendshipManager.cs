using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that tracks friendship levels for all NPCs.
/// Friendship increases when the player talks to an NPC.
/// </summary>
public class FriendshipManager : MonoBehaviour
{
    public static FriendshipManager Instance { get; private set; }

    [SerializeField] private int _friendshipPerTalk = 5;
    [SerializeField] private int _maxFriendship = 100;

    // npcName → friendship points
    private Dictionary<string, int> _friendshipData = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddFriendship(string npcName, int amount)
    {
        if (!_friendshipData.ContainsKey(npcName))
            _friendshipData[npcName] = 0;

        _friendshipData[npcName] = Mathf.Clamp(_friendshipData[npcName] + amount, 0, _maxFriendship);
        GameEvents.OnFriendshipChanged.Invoke(npcName, _friendshipData[npcName]);
    }

    public void OnTalkedToNPC(string npcName)
    {
        AddFriendship(npcName, _friendshipPerTalk);
    }

    public int GetFriendship(string npcName)
    {
        return _friendshipData.ContainsKey(npcName) ? _friendshipData[npcName] : 0;
    }

    public Dictionary<string, int> GetAllFriendships() => _friendshipData;

    /// <summary>
    /// Register an NPC so it shows up in the UI even at 0 friendship.
    /// </summary>
    public void RegisterNPC(string npcName)
    {
        if (!_friendshipData.ContainsKey(npcName))
            _friendshipData[npcName] = 0;
    }
}
