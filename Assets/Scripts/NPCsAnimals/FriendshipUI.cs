using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// UI panel showing friendship levels for all known NPCs.
/// Toggle with F key. Shows hearts (0-10) based on 0-100 points.
/// Creates rows dynamically — no prefab needed.
/// </summary>
public class FriendshipUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform  _listContainer;
    [SerializeField] private GameObject _friendshipRowPrefab;

    private Dictionary<string, Slider> _sliderMap = new Dictionary<string, Slider>();
    private Dictionary<string, Text> _heartTextMap = new Dictionary<string, Text>();

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
        int hearts = Mathf.CeilToInt(value / 10f);
        string heartStr = new string('\u2665', hearts) + new string('\u2661', 10 - hearts);

        if (_sliderMap.ContainsKey(npcName))
        {
            _sliderMap[npcName].value = value;
            if (_heartTextMap.ContainsKey(npcName))
                _heartTextMap[npcName].text = $"{npcName}  {heartStr}  ({value}/100)";
            return;
        }

        if (_listContainer == null) return;

        // Create row dynamically
        GameObject row = new GameObject($"Row_{npcName}");
        row.layer = 5;
        row.transform.SetParent(_listContainer, false);
        RectTransform rowRt = row.AddComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(280, 40);

        // Name + hearts text
        GameObject nameObj = new GameObject("NameText");
        nameObj.layer = 5;
        nameObj.transform.SetParent(row.transform, false);
        RectTransform nameRt = nameObj.AddComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 0);
        nameRt.anchorMax = new Vector2(1, 1);
        nameRt.offsetMin = new Vector2(5, 20);
        nameRt.offsetMax = new Vector2(-5, 0);
        nameObj.AddComponent<CanvasRenderer>();
        Text nameText = nameObj.AddComponent<Text>();
        nameText.text = $"{npcName}  {heartStr}  ({value}/100)";
        nameText.fontSize = 13;
        nameText.color = Color.white;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _heartTextMap[npcName] = nameText;

        // Slider bar
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.layer = 5;
        sliderObj.transform.SetParent(row.transform, false);
        RectTransform slRt = sliderObj.AddComponent<RectTransform>();
        slRt.anchorMin = new Vector2(0, 0);
        slRt.anchorMax = new Vector2(1, 0);
        slRt.pivot = new Vector2(0.5f, 0);
        slRt.anchoredPosition = new Vector2(0, 2);
        slRt.sizeDelta = new Vector2(-10, 14);
        slRt.offsetMin = new Vector2(5, 2);
        slRt.offsetMax = new Vector2(-5, 16);

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.layer = 5;
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        bgObj.AddComponent<CanvasRenderer>();
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.layer = 5;
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
        faRt.offsetMin = Vector2.zero; faRt.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.layer = 5;
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        fill.AddComponent<CanvasRenderer>();
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.3f, 0.5f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.maxValue = 100;
        slider.value = value;
        slider.interactable = false;

        _sliderMap[npcName] = slider;
    }
}
