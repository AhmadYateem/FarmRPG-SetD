using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One-time editor script to build all Milestone 3 UI panels.
/// Run from menu: Tools > Build M3 UI
/// </summary>
public class BuildM3UI
{
    [MenuItem("Tools/Build M3 UI")]
    public static void Build()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No Canvas found!"); return; }

        BuildTimePanel(canvas.transform);
        BuildFriendshipPanel(canvas.transform);
        BuildAnimalInfoPanel(canvas.transform);
        BuildProductionPanel(canvas.transform);
        BuildNPCNameText(canvas.transform);

        Debug.Log("All Milestone 3 UI panels created!");
    }

    static void BuildTimePanel(Transform parent)
    {
        // Panel in top-left corner
        GameObject panel = CreatePanel("TimePanel", parent, new Vector2(10, -10), new Vector2(240, 110), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

        // Time display text — negative Y so it stays INSIDE the panel
        GameObject textObj = CreateText("TimeText", panel.transform, "Day 1  6:00", 18, TextAnchor.MiddleCenter, new Vector2(10, -8), new Vector2(220, 32));

        // Advance Time button
        GameObject btnTime = CreateButton("AdvanceTimeBtn", panel.transform, "+1 Hour", new Vector2(12, -48), new Vector2(104, 34));

        // Advance Day button
        GameObject btnDay = CreateButton("AdvanceDayBtn", panel.transform, "Next Day", new Vector2(124, -48), new Vector2(104, 34));

        // Wire MockTimeManager
        MockTimeManager tm = Object.FindFirstObjectByType<MockTimeManager>();
        if (tm != null)
        {
            var serialized = new SerializedObject(tm);
            serialized.FindProperty("_timeDisplay").objectReferenceValue = textObj.GetComponent<Text>();
            serialized.FindProperty("_advanceTimeButton").objectReferenceValue = btnTime.GetComponent<Button>();
            serialized.FindProperty("_advanceDayButton").objectReferenceValue = btnDay.GetComponent<Button>();
            serialized.ApplyModifiedProperties();
        }
    }

    static void BuildFriendshipPanel(Transform parent)
    {
        // Panel in center, starts hidden
        GameObject panel = CreatePanel("FriendshipPanel", parent, Vector2.zero, new Vector2(300, 400), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panel.SetActive(false);

        // Title
        CreateText("Title", panel.transform, "Friendships", 24, TextAnchor.UpperCenter, new Vector2(0, -10), new Vector2(280, 40));

        // List container with vertical layout
        GameObject listContainer = new GameObject("FriendshipList");
        listContainer.transform.SetParent(panel.transform, false);
        var listRect = listContainer.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0);
        listRect.anchorMax = new Vector2(1, 1);
        listRect.offsetMin = new Vector2(10, 10);
        listRect.offsetMax = new Vector2(-10, -60);
        var layout = listContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Create friendship row prefab
        GameObject rowPrefab = CreateFriendshipRowPrefab();

        // Wire FriendshipUI
        FriendshipUI fui = Object.FindFirstObjectByType<FriendshipUI>();
        if (fui == null)
        {
            // Add to GameManagers
            GameObject managers = GameObject.Find("GameManagers");
            if (managers != null) fui = managers.AddComponent<FriendshipUI>();
        }
        if (fui != null)
        {
            var serialized = new SerializedObject(fui);
            serialized.FindProperty("_panel").objectReferenceValue = panel;
            serialized.FindProperty("_listContainer").objectReferenceValue = listContainer.transform;
            serialized.FindProperty("_friendshipRowPrefab").objectReferenceValue = rowPrefab;
            serialized.ApplyModifiedProperties();
        }
    }

    static GameObject CreateFriendshipRowPrefab()
    {
        // Create in scene first, then save as prefab
        GameObject row = new GameObject("FriendshipRow");
        var rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(280, 35);
        var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 10;
        rowLayout.childForceExpandHeight = true;

        // NPC name text
        GameObject nameObj = new GameObject("NPCName");
        nameObj.transform.SetParent(row.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        var nameLe = nameObj.AddComponent<LayoutElement>();
        nameLe.preferredWidth = 100;
        var nameText = nameObj.AddComponent<Text>();
        nameText.text = "NPC";
        nameText.fontSize = 16;
        nameText.color = Color.white;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Slider
        GameObject sliderObj = CreateSlider("FriendshipSlider", row.transform);

        // Save as prefab
        string prefabPath = "Assets/Prefabs/FriendshipRow.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, prefabPath);
        Object.DestroyImmediate(row);
        return prefab;
    }

    static void BuildAnimalInfoPanel(Transform parent)
    {
        // Panel in bottom-right, starts hidden
        GameObject panel = CreatePanel("AnimalInfoPanel", parent, new Vector2(-10, 10), new Vector2(250, 200), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        panel.SetActive(false);

        CreateText("AnimalName", panel.transform, "Animal Name", 20, TextAnchor.UpperCenter, new Vector2(0, -10), new Vector2(230, 30));
        CreateText("AnimalType", panel.transform, "Type", 16, TextAnchor.UpperCenter, new Vector2(0, -40), new Vector2(230, 25));

        // Hunger label + slider
        CreateText("HungerLabel", panel.transform, "Hunger", 14, TextAnchor.MiddleLeft, new Vector2(10, -70), new Vector2(60, 20));
        GameObject hungerSlider = CreateSliderAt("HungerSlider", panel.transform, new Vector2(50, -70), new Vector2(150, 20));

        // Happiness label + slider
        CreateText("HappyLabel", panel.transform, "Happy", 14, TextAnchor.MiddleLeft, new Vector2(10, -95), new Vector2(60, 20));
        GameObject happySlider = CreateSliderAt("HappinessSlider", panel.transform, new Vector2(50, -95), new Vector2(150, 20));

        // Production text
        CreateText("ProductionText", panel.transform, "Not ready", 16, TextAnchor.MiddleCenter, new Vector2(0, -130), new Vector2(230, 25));

        // Wire AnimalInfoPanel
        AnimalInfoPanel aip = Object.FindFirstObjectByType<AnimalInfoPanel>();
        if (aip == null)
        {
            GameObject managers = GameObject.Find("GameManagers");
            if (managers != null) aip = managers.AddComponent<AnimalInfoPanel>();
        }
        if (aip != null)
        {
            var serialized = new SerializedObject(aip);
            serialized.FindProperty("_panel").objectReferenceValue = panel;
            serialized.FindProperty("_animalNameText").objectReferenceValue = panel.transform.Find("AnimalName").GetComponent<Text>();
            serialized.FindProperty("_animalTypeText").objectReferenceValue = panel.transform.Find("AnimalType").GetComponent<Text>();
            serialized.FindProperty("_hungerSlider").objectReferenceValue = hungerSlider.GetComponent<Slider>();
            serialized.FindProperty("_happinessSlider").objectReferenceValue = happySlider.GetComponent<Slider>();
            serialized.FindProperty("_productionText").objectReferenceValue = panel.transform.Find("ProductionText").GetComponent<Text>();
            serialized.ApplyModifiedProperties();
        }
    }

    static void BuildProductionPanel(Transform parent)
    {
        // Panel in center-right, starts hidden
        GameObject panel = CreatePanel("ProductionPanel", parent, new Vector2(-10, 0), new Vector2(250, 300), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        panel.SetActive(false);

        CreateText("Title", panel.transform, "Animal Products", 20, TextAnchor.UpperCenter, new Vector2(0, -10), new Vector2(230, 30));

        // Inventory text
        CreateText("InventoryText", panel.transform, "No products collected yet.", 14, TextAnchor.UpperLeft, new Vector2(10, -50), new Vector2(220, 150));

        // Collect All button
        GameObject collectBtn = CreateButton("CollectAllBtn", panel.transform, "Collect All", new Vector2(70, -220), new Vector2(100, 35));

        // Wire AnimalProductionUI
        AnimalProductionUI apui = Object.FindFirstObjectByType<AnimalProductionUI>();
        if (apui == null)
        {
            GameObject managers = GameObject.Find("GameManagers");
            if (managers != null) apui = managers.AddComponent<AnimalProductionUI>();
        }
        if (apui != null)
        {
            var serialized = new SerializedObject(apui);
            serialized.FindProperty("_panel").objectReferenceValue = panel;
            serialized.FindProperty("_inventoryText").objectReferenceValue = panel.transform.Find("InventoryText").GetComponent<Text>();
            serialized.ApplyModifiedProperties();
        }
    }

    static void BuildNPCNameText(Transform parent)
    {
        // Add NPC name text to dialogue panel
        Transform dialoguePanel = null;
        foreach (Transform child in parent)
        {
            if (child.name.Contains("Dialogue") || child.name.Contains("dialogue"))
            {
                dialoguePanel = child;
                break;
            }
        }
        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialoguePanel not found — NPC name text not wired.");
            return;
        }

        // Check if NPC_Name_Text already exists
        Transform existing = dialoguePanel.Find("NPC_Name_Text");
        if (existing != null) return;

        // Create NPC name text at top of dialogue panel
        GameObject nameObj = new GameObject("NPC_Name_Text");
        nameObj.transform.SetParent(dialoguePanel, false);
        var rect = nameObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -5);
        rect.sizeDelta = new Vector2(0, 30);
        var text = nameObj.AddComponent<Text>();
        text.text = "NPC Name";
        text.fontSize = 22;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(1f, 0.9f, 0.5f);
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    // ── helpers ──────────────────────────────────────────

    static GameObject CreatePanel(string name, Transform parent, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        var img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        return panel;
    }

    static GameObject CreateText(string name, Transform parent, string content, int fontSize, TextAnchor anchor, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        var text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return obj;
    }

    static GameObject CreateButton(string name, Transform parent, string label, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        var img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        btnObj.AddComponent<Button>();

        // Label
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btnObj;
    }

    static GameObject CreateSlider(string name, Transform parent)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        var le = sliderObj.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;
        var slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 0;
        slider.interactable = false;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.8f, 0.2f, 0.3f);

        slider.fillRect = fillRect;

        return sliderObj;
    }

    static GameObject CreateSliderAt(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(0, 1);
        sliderRect.pivot = new Vector2(0, 1);
        sliderRect.anchoredPosition = pos;
        sliderRect.sizeDelta = size;
        var slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 50;
        slider.interactable = false;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.3f);

        slider.fillRect = fillRect;

        return sliderObj;
    }
}
