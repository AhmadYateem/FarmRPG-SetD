using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Dialogue tree system — supports branching NPC conversations.
/// The whole tree is built in the Inspector using DialogueNode/DialogueChoice data.
/// Now integrates with FriendshipManager to pick dialogue based on friendship tier.
/// </summary>
public class NPCDialogue : MonoBehaviour
{
    // ── NPC identity ──────────────────────────────────────
    [SerializeField] private string _npcName = "Villager";

    // ── tree data ─────────────────────────────────────────
    [SerializeField] private DialogueNode _rootNode;

    // ── friendship-tier dialogue (optional — overrides _rootNode) ──
    [SerializeField] private NPCData _npcData;

    // ── UI references (drag from Hierarchy) ───────────────
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private Text       _npcNameText;
    [SerializeField] private Text       _npcText;
    [SerializeField] private GameObject _choiceButtonPrefab;
    [SerializeField] private Transform  _choicesContainer;

    // ── interaction settings ──────────────────────────────
    [SerializeField] private float   _interactRange = 2f;

    // ── internal state ────────────────────────────────────
    private DialogueNode    _currentNode;
    private bool            _isActive = false;
    private List<GameObject> _activeButtons = new List<GameObject>();
    private Transform       _playerTransform;

    void Start()
    {
        // find the player by tag — make sure your player object is tagged "Player"
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        // hide dialogue box at the start
        if (_dialoguePanel != null) _dialoguePanel.SetActive(false);

        // register NPC with friendship system
        if (FriendshipManager.Instance != null)
            FriendshipManager.Instance.RegisterNPC(_npcName);

        // sync NPCData name
        if (_npcData != null) _npcData.npcName = _npcName;
    }

    void Update()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        // press E near the NPC to start talking
        if (!_isActive && dist <= _interactRange && Keyboard.current[Key.E].wasPressedThisFrame)
        {
            StartDialogue();
        }

        // auto-close if the player walks out of range
        if (_isActive && dist > _interactRange * 1.5f)
        {
            EndDialogue();
        }
    }

    // ── public methods ────────────────────────────────────

    [ContextMenu("Start Dialogue")]
    public void StartDialogue()
    {
        // pick the right dialogue tree based on friendship tier
        DialogueNode root = GetDialogueRoot();
        if (root == null) { Debug.LogWarning("No dialogue tree — set up Root Node in Inspector!"); return; }

        _isActive = true;
        _dialoguePanel.SetActive(true);
        if (_npcNameText != null) _npcNameText.text = _npcName;
        GameEvents.OnDialogueStart.Invoke();
        ShowNode(root);
    }

    [ContextMenu("End Dialogue")]
    public void EndDialogue()
    {
        _isActive = false;
        _dialoguePanel.SetActive(false);
        ClearChoiceButtons();

        // increase friendship when conversation ends
        if (FriendshipManager.Instance != null)
            FriendshipManager.Instance.OnTalkedToNPC(_npcName);

        GameEvents.OnDialogueEnd.Invoke();
    }

    // ── private methods ───────────────────────────────────

    private DialogueNode GetDialogueRoot()
    {
        // if NPCData is wired up and has friendship-tier dialogues, use those
        if (_npcData != null && FriendshipManager.Instance != null)
        {
            _npcData.friendshipPoints = FriendshipManager.Instance.GetFriendship(_npcName);
            DialogueNode tierNode = _npcData.GetDialogueForCurrentTier();
            if (tierNode != null) return tierNode;
        }
        return _rootNode;
    }

    private void ShowNode(DialogueNode node)
    {
        _currentNode  = node;
        _npcText.text = node.npcText;
        ClearChoiceButtons();

        if (node.IsEndNode())
        {
            // dead end — just show a close button
            SpawnChoiceButton("[ Close ]", null);
        }
        else
        {
            // spawn one button per choice
            foreach (DialogueChoice choice in node.choices)
            {
                SpawnChoiceButton(choice.choiceText, choice.nextNode);
            }
        }
    }

    private void SpawnChoiceButton(string label, DialogueNode nextNode)
    {
        GameObject btn = Instantiate(_choiceButtonPrefab, _choicesContainer);
        btn.GetComponentInChildren<Text>().text = label;
        _activeButtons.Add(btn);

        // wire the click — lambda captures the nextNode reference
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (nextNode == null) EndDialogue();
            else                  ShowNode(nextNode);
        });
    }

    private void ClearChoiceButtons()
    {
        // Destroy tracked buttons
        foreach (GameObject btn in _activeButtons)
            if (btn != null) Destroy(btn);
        _activeButtons.Clear();

        // Also destroy any stale buttons left by a different NPC that shared this container
        if (_choicesContainer != null)
        {
            for (int i = _choicesContainer.childCount - 1; i >= 0; i--)
                Destroy(_choicesContainer.GetChild(i).gameObject);
        }
    }
}
