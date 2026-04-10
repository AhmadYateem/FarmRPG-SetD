using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// UI panel showing friendship levels for all known NPCs.
/// Toggle with F key.
/// </summary>
public class FriendshipUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform  _listContainer;
    [SerializeField] private GameObject _friendshipRowPrefab; // prefab with Text + Slider

    private Dictionary<string, Slider> _sliderMap = new Dictionary<string, Slider>();

    void Start()
    {
        if (_panel != null) _panel.SetActive(false);
        GameEvents.OnFriendshipChanged.AddListener(OnFriendshipChanged);
    }

    void OnDestroy()
    {
        GameEvents.OnFriendshipChanged.RemoveListener(OnFriendshipChanged);
    }

    void Update()
    {
        if (Keyboard.current[Key.F].wasPressedThisFrame)
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        bool show = !_panel.activeSelf;
        _panel.SetActive(show);
        if (show) RefreshAll();
    }

    private void RefreshAll()
    {
        if (FriendshipManager.Instance == null) return;

        foreach (var kvp in FriendshipManager.Instance.GetAllFriendships())
        {
            UpdateOrCreateRow(kvp.Key, kvp.Value);
        }
    }

    private void OnFriendshipChanged(string npcName, int newValue)
    {
        if (_panel.activeSelf)
            UpdateOrCreateRow(npcName, newValue);
    }

    private void UpdateOrCreateRow(string npcName, int value)
    {
        if (_sliderMap.ContainsKey(npcName))
        {
            _sliderMap[npcName].value = value;
            return;
        }

        // create a new row
        if (_friendshipRowPrefab == null || _listContainer == null) return;

        GameObject row = Instantiate(_friendshipRowPrefab, _listContainer);
        Text nameText = row.GetComponentInChildren<Text>();
        Slider slider = row.GetComponentInChildren<Slider>();

        if (nameText != null) nameText.text = npcName;
        if (slider != null)
        {
            slider.maxValue = 100;
            slider.value = value;
            _sliderMap[npcName] = slider;
        }
    }
}
