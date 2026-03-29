using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dialogue tree system — supports branching NPC conversations.
/// The whole tree is built in the Inspector using DialogueNode/DialogueChoice data.
/// </summary>
public class NPCDialogue : MonoBehaviour
{
    // ── tree data ─────────────────────────────────────────
    [SerializeField] private DialogueNode _rootNode;

    // ── UI references (drag from Hierarchy) ───────────────
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private Text       _npcText;
    [SerializeField] private GameObject _choiceButtonPrefab;
    [SerializeField] private Transform  _choicesContainer;

    // ── interaction settings ──────────────────────────────
    [SerializeField] private KeyCode _interactKey   = KeyCode.E;
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
    }

    void Update()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        // press E near the NPC to start talking
        if (!_isActive && dist <= _interactRange && Input.GetKeyDown(_interactKey))
        {
            StartDialogue();
        }
    }

    // ── public methods ────────────────────────────────────

    [ContextMenu("Start Dialogue")]
    public void StartDialogue()
    {
        if (_rootNode == null) { Debug.LogWarning("No dialogue tree — set up Root Node in Inspector!"); return; }

        _isActive = true;
        _dialoguePanel.SetActive(true);
        GameEvents.OnDialogueStart.Invoke();
        ShowNode(_rootNode);
    }

    [ContextMenu("End Dialogue")]
    public void EndDialogue()
    {
        _isActive = false;
        _dialoguePanel.SetActive(false);
        ClearChoiceButtons();
        GameEvents.OnDialogueEnd.Invoke();
    }

    // ── private methods ───────────────────────────────────

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
        foreach (GameObject btn in _activeButtons)
            Destroy(btn);
        _activeButtons.Clear();
    }
}
