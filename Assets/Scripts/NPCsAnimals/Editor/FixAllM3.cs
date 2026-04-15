using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Comprehensive Milestone 3 fixer — run once to fix EVERYTHING.
/// Menu: Tools > Fix All M3
/// </summary>
public class FixAllM3
{
    [MenuItem("Tools/Fix All M3")]
    public static void FixAll()
    {
        SetupPlayer();
        FixUILayers();
        RebuildTimePanel();
        FixDialoguePanelLayout();
        SetDialogueTrees();
        FixAnimalData();
        AddDuplicateAnimals();
        AddInteractionFeedback();
        RewireAnimalManager();
        FixAnimalInfoPanelLayout();
        FixFriendshipPanelLayout();
        FixProductionPanelLayout();
        WireCollectAllButton();
        FixNPCNames();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== Fix All M3 Complete — ALL systems ready ===");
    }

    // ═══════════════════════════════════════════════════════
    //  FIND MAIN CANVAS (skip world-space NameCanvas)
    // ═══════════════════════════════════════════════════════

    static Canvas FindMainCanvas()
    {
        Canvas[] all = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in all)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.transform.parent == null)
                return c;
        }
        foreach (Canvas c in all)
        {
            if (c.name == "Canvas" && c.transform.parent == null)
                return c;
        }
        Debug.LogWarning("[FixAll] No main Canvas found!");
        return null;
    }

    // ═══════════════════════════════════════════════════════
    //  1.  SETUP PLAYER (controller + sprite + tag)
    // ═══════════════════════════════════════════════════════

    static void SetupPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) { Debug.LogWarning("[FixAll] Player not found"); return; }

        player.tag = "Player";

        if (player.GetComponent<PlayerController>() == null)
            player.AddComponent<PlayerController>();

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 5;
            if (sr.sprite == null)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/npc_shopkeeper.png");
                foreach (Object a in assets)
                {
                    if (a is Sprite s && s.name == "npc_shopkeeper_0")
                    { sr.sprite = s; break; }
                }
            }
            EditorUtility.SetDirty(sr);
        }

        EditorUtility.SetDirty(player);
        Debug.Log("[FixAll] Player: tagged, controller attached, sprite set");
    }

    // ═══════════════════════════════════════════════════════
    //  2.  FIX UI LAYERS (all children → layer 5)
    // ═══════════════════════════════════════════════════════

    static void FixUILayers()
    {
        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;
        SetLayerRecursive(canvas.gameObject, 5);
        Debug.Log("[FixAll] All UI children set to layer 5");
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        for (int i = 0; i < go.transform.childCount; i++)
            SetLayerRecursive(go.transform.GetChild(i).gameObject, layer);
    }

    // ═══════════════════════════════════════════════════════
    //  3.  REBUILD TIME PANEL (clean, no overflow)
    // ═══════════════════════════════════════════════════════

    static void RebuildTimePanel()
    {
        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;

        // destroy old
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            if (canvas.transform.GetChild(i).name == "TimePanel")
                Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        GameObject panel = new GameObject("TimePanel");
        panel.layer = 5;
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10);
        rt.sizeDelta = new Vector2(260, 120);
        var img = panel.AddComponent<Image>();
        img.color = new Color(0.06f, 0.06f, 0.12f, 0.93f);

        // Time display
        GameObject timeText = MakeText("TimeText", panel.transform,
            "Day 1  6:00", 20, TextAnchor.MiddleCenter,
            new Vector2(0, -5), new Vector2(0, 36), true);

        // Buttons row
        GameObject btnTime = MakeButton("AdvanceTimeBtn", panel.transform,
            "+1 Hour", new Vector2(14, -50), new Vector2(110, 38));
        GameObject btnDay = MakeButton("AdvanceDayBtn", panel.transform,
            "Next Day", new Vector2(136, -50), new Vector2(110, 38));

        // Hint text
        MakeText("HintText", panel.transform,
            "WASD move | E interact | F friends | G products",
            10, TextAnchor.MiddleCenter,
            new Vector2(0, -93), new Vector2(0, 22), true);

        // Wire MockTimeManager
        MockTimeManager tm = Object.FindFirstObjectByType<MockTimeManager>();
        if (tm != null)
        {
            var so = new SerializedObject(tm);
            so.FindProperty("_timeDisplay").objectReferenceValue = timeText.GetComponent<Text>();
            so.FindProperty("_advanceTimeButton").objectReferenceValue = btnTime.GetComponent<Button>();
            so.FindProperty("_advanceDayButton").objectReferenceValue = btnDay.GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        Debug.Log("[FixAll] TimePanel rebuilt — clean layout");
    }

    // ═══════════════════════════════════════════════════════
    //  4.  FIX DIALOGUE PANEL LAYOUT
    // ═══════════════════════════════════════════════════════

    static void FixDialoguePanelLayout()
    {
        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;

        Transform dp = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            if (canvas.transform.GetChild(i).name.Contains("Dialogue"))
            { dp = canvas.transform.GetChild(i); break; }
        }
        if (dp == null) { Debug.LogWarning("[FixAll] DialoguePanel not found"); return; }

        // Bottom-center dialogue bar
        RectTransform rt = dp.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0f);
        rt.anchorMax = new Vector2(0.9f, 0f);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta = new Vector2(0, 200);

        Image img = dp.GetComponent<Image>();
        if (img != null) img.color = new Color(0.06f, 0.06f, 0.14f, 0.94f);

        dp.gameObject.layer = 5;

        // NPC_Name_Text — top of panel
        Transform nameT = dp.Find("NPC_Name_Text");
        if (nameT != null)
        {
            nameT.gameObject.layer = 5;
            RectTransform nrt = nameT.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0, 1);
            nrt.anchorMax = new Vector2(1, 1);
            nrt.pivot = new Vector2(0.5f, 1);
            nrt.anchoredPosition = new Vector2(0, -8);
            nrt.sizeDelta = new Vector2(-20, 30);

            Text txt = nameT.GetComponent<Text>();
            if (txt != null)
            {
                txt.fontSize = 22;
                txt.fontStyle = FontStyle.Bold;
                txt.color = new Color(1f, 0.85f, 0.3f);
                txt.alignment = TextAnchor.MiddleCenter;
                txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        // NPC_Text — middle
        Transform npcT = dp.Find("NPC_Text");
        if (npcT != null)
        {
            npcT.gameObject.layer = 5;
            RectTransform nrt = npcT.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0, 0.3f);
            nrt.anchorMax = new Vector2(1, 0.8f);
            nrt.pivot = new Vector2(0.5f, 0.5f);
            nrt.anchoredPosition = Vector2.zero;
            nrt.offsetMin = new Vector2(20, 0);
            nrt.offsetMax = new Vector2(-20, 0);

            Text txt = npcT.GetComponent<Text>();
            if (txt != null)
            {
                txt.fontSize = 18;
                txt.color = Color.white;
                txt.alignment = TextAnchor.UpperLeft;
                txt.horizontalOverflow = HorizontalWrapMode.Wrap;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        // ChoicesContainer — bottom
        Transform choicesT = dp.Find("ChoicesContainer");
        if (choicesT != null)
        {
            choicesT.gameObject.layer = 5;
            RectTransform crt = choicesT.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 0);
            crt.anchorMax = new Vector2(1, 0.3f);
            crt.pivot = new Vector2(0.5f, 0);
            crt.anchoredPosition = Vector2.zero;
            crt.offsetMin = new Vector2(20, 5);
            crt.offsetMax = new Vector2(-20, 0);

            // Ensure vertical layout
            var vlg = choicesT.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = choicesT.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        dp.gameObject.SetActive(false);
        EditorUtility.SetDirty(dp.gameObject);
        Debug.Log("[FixAll] DialoguePanel layout fixed");
    }

    // ═══════════════════════════════════════════════════════
    //  5.  SET DIALOGUE TREES (3 tiers per NPC)
    // ═══════════════════════════════════════════════════════

    static void SetDialogueTrees()
    {
        SetNPCDialogue("NPC_ShopKeeper", "ShopKeeper",
            "Welcome to the shop. Have a look around.",
            new[] { "What do you sell?", "Just browsing." },
            new[] { "Seeds, tools, and animal feed — everything a farmer needs.", "Let me know if you need anything." },
            "Hey, good to see you again! Need supplies?",
            new[] { "Got anything new?", "How's business?" },
            new[] { "Just got some rare seeds in stock! Check them out.", "Can't complain — the farm keeps us all busy." },
            "My favourite customer! Come in, come in!",
            new[] { "Any discounts for me?", "How are you?" },
            new[] { "For you? Always! Take 10 percent off today.", "Better now that you're here, friend!" }
        );

        SetNPCDialogue("NPC_Farmer", "Farmer",
            "Mornin'. The fields need tending.",
            new[] { "Need any help?", "Nice farm you have." },
            new[] { "I could always use an extra pair of hands out here.", "Thanks — it's hard work, but worth every bit." },
            "Oh hey! Weather's been great for the crops lately.",
            new[] { "What are you growing?", "Any farming tips?" },
            new[] { "Tomatoes, corn, and some wheat this season.", "Water your crops early morning — they'll love you for it!" },
            "Partner! Want to see the new harvest?",
            new[] { "What did you grow?", "You're the best farmer around!" },
            new[] { "Best pumpkins I've ever grown! Take some home with you.", "Aw shucks, you're making me blush!" }
        );

        SetNPCDialogue("NPC_Villager", "Villager",
            "Oh, hello. Are you new around here?",
            new[] { "Yes, just moved in.", "What is this place?" },
            new[] { "Welcome to the village! It's peaceful here.", "Just a small farming village — nothing fancy, but we like it." },
            "Hey neighbour! Beautiful day, isn't it?",
            new[] { "It sure is!", "What's new in town?" },
            new[] { "I love the sunsets here — you should visit the park!", "The harvest festival is coming soon!" },
            "There you are! I was hoping to see you today!",
            new[] { "Miss me?", "Want to hang out?" },
            new[] { "You're my best friend in the whole village!", "Let's go to the park together sometime!" }
        );
    }

    static void SetNPCDialogue(string goName, string npcName,
        string lowRoot,  string[] lowChoices,  string[] lowResponses,
        string midRoot,  string[] midChoices,  string[] midResponses,
        string highRoot, string[] highChoices, string[] highResponses)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) return;

        NPCDialogue dialogue = go.GetComponent<NPCDialogue>();
        if (dialogue == null) return;

        var so = new SerializedObject(dialogue);

        // _rootNode = low tier (fallback)
        SetDialogueNode(so.FindProperty("_rootNode"), lowRoot, lowChoices, lowResponses);

        // _npcData with all 3 tiers
        var npcData = so.FindProperty("_npcData");
        npcData.FindPropertyRelative("npcName").stringValue = npcName;
        SetDialogueNode(npcData.FindPropertyRelative("lowFriendshipDialogue"), lowRoot, lowChoices, lowResponses);
        SetDialogueNode(npcData.FindPropertyRelative("midFriendshipDialogue"), midRoot, midChoices, midResponses);
        SetDialogueNode(npcData.FindPropertyRelative("highFriendshipDialogue"), highRoot, highChoices, highResponses);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(dialogue);
        Debug.Log($"[FixAll] Dialogue set for {goName} — 3 tiers with 2 choices each");
    }

    static void SetDialogueNode(SerializedProperty nodeProp,
        string rootText, string[] choiceTexts, string[] responseTexts)
    {
        nodeProp.FindPropertyRelative("npcText").stringValue = rootText;
        var choices = nodeProp.FindPropertyRelative("choices");
        int count = Mathf.Min(choiceTexts.Length, responseTexts.Length);
        choices.arraySize = count;
        for (int i = 0; i < count; i++)
        {
            var choice = choices.GetArrayElementAtIndex(i);
            choice.FindPropertyRelative("choiceText").stringValue = choiceTexts[i];
            var nextNode = choice.FindPropertyRelative("nextNode");
            nextNode.FindPropertyRelative("npcText").stringValue = responseTexts[i];
            var nextChoices = nextNode.FindPropertyRelative("choices");
            if (nextChoices != null) nextChoices.arraySize = 0;
        }
    }

    // ═══════════════════════════════════════════════════════
    //  6.  FIX ANIMAL DATA (hunger/happiness = 50)
    // ═══════════════════════════════════════════════════════

    static void FixAnimalData()
    {
        string[] goNames     = { "Animal_Chicken", "Animal_Cow", "Animal_Sheep" };
        string[] displayNames = { "Clucky", "Bessie", "Woolly" };
        AnimalData.AnimalType[] types = {
            AnimalData.AnimalType.Chicken,
            AnimalData.AnimalType.Cow,
            AnimalData.AnimalType.Sheep
        };

        for (int i = 0; i < goNames.Length; i++)
        {
            GameObject go = GameObject.Find(goNames[i]);
            if (go == null) continue;

            AnimalController ac = go.GetComponent<AnimalController>();
            if (ac == null) continue;

            var so = new SerializedObject(ac);
            var data = so.FindProperty("_animalData");
            data.FindPropertyRelative("animalName").stringValue = displayNames[i];
            data.FindPropertyRelative("animalType").enumValueIndex = (int)types[i];
            data.FindPropertyRelative("hunger").intValue = 50;
            data.FindPropertyRelative("happiness").intValue = 50;
            data.FindPropertyRelative("productionReady").boolValue = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ac);
            Debug.Log($"[FixAll] {goNames[i]}: {displayNames[i]}, hunger=50, happiness=50");
        }
    }

    // ═══════════════════════════════════════════════════════
    //  7.  ADD DUPLICATE ANIMALS (multiple of each type)
    // ═══════════════════════════════════════════════════════

    static void AddDuplicateAnimals()
    {
        AddAnimalClone("Animal_Chicken", "Nugget",  AnimalData.AnimalType.Chicken, new Vector3(4.5f, -3.5f, 0f));
        AddAnimalClone("Animal_Cow",     "Daisy",   AnimalData.AnimalType.Cow,     new Vector3(8.5f, -2f, 0f));
        AddAnimalClone("Animal_Sheep",   "Cotton",  AnimalData.AnimalType.Sheep,   new Vector3(-1.5f, -5f, 0f));
    }

    static void AddAnimalClone(string sourceGoName, string newAnimalName,
        AnimalData.AnimalType animalType, Vector3 position)
    {
        string newGoName = sourceGoName + "_2";
        if (GameObject.Find(newGoName) != null) return; // already exists

        GameObject source = GameObject.Find(sourceGoName);
        if (source == null) return;

        GameObject go = new GameObject(newGoName);
        go.transform.position = position;

        SpriteRenderer srcSR = source.GetComponent<SpriteRenderer>();
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (srcSR != null && srcSR.sprite != null)
        {
            sr.sprite = srcSR.sprite;
            sr.sortingOrder = srcSR.sortingOrder;
        }

        AnimalController ac = go.AddComponent<AnimalController>();
        var so = new SerializedObject(ac);
        var data = so.FindProperty("_animalData");
        data.FindPropertyRelative("animalName").stringValue = newAnimalName;
        data.FindPropertyRelative("animalType").enumValueIndex = (int)animalType;
        data.FindPropertyRelative("hunger").intValue = 50;
        data.FindPropertyRelative("happiness").intValue = 50;
        data.FindPropertyRelative("productionReady").boolValue = false;
        so.ApplyModifiedProperties();

        go.AddComponent<InteractionFeedback>();
        Debug.Log($"[FixAll] Created {newGoName} ({newAnimalName}) at {position}");
    }

    // ═══════════════════════════════════════════════════════
    //  8.  ADD INTERACTION FEEDBACK TO ALL ANIMALS
    // ═══════════════════════════════════════════════════════

    static void AddInteractionFeedback()
    {
        AnimalController[] animals = Object.FindObjectsByType<AnimalController>(FindObjectsSortMode.None);
        foreach (AnimalController ac in animals)
        {
            if (ac.GetComponent<InteractionFeedback>() == null)
            {
                ac.gameObject.AddComponent<InteractionFeedback>();
                EditorUtility.SetDirty(ac.gameObject);
            }
        }

        // Also add to NPCs
        NPCDialogue[] npcs = Object.FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
        foreach (NPCDialogue npc in npcs)
        {
            if (npc.GetComponent<InteractionFeedback>() == null)
            {
                npc.gameObject.AddComponent<InteractionFeedback>();
                EditorUtility.SetDirty(npc.gameObject);
            }
        }

        Debug.Log($"[FixAll] InteractionFeedback on all animals and NPCs");
    }

    // ═══════════════════════════════════════════════════════
    //  9.  RE-WIRE ANIMAL MANAGER LIST
    // ═══════════════════════════════════════════════════════

    static void RewireAnimalManager()
    {
        AnimalManager am = Object.FindFirstObjectByType<AnimalManager>();
        if (am == null) return;

        AnimalController[] animals = Object.FindObjectsByType<AnimalController>(FindObjectsSortMode.None);
        var so = new SerializedObject(am);
        var list = so.FindProperty("_animals");
        list.arraySize = animals.Length;
        for (int i = 0; i < animals.Length; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = animals[i];
        so.ApplyModifiedProperties();
        Debug.Log($"[FixAll] AnimalManager wired with {animals.Length} animals");
    }

    // ═══════════════════════════════════════════════════════
    //  10. FIX ANIMAL INFO PANEL LAYOUT
    // ═══════════════════════════════════════════════════════

    static void FixAnimalInfoPanelLayout()
    {
        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;

        Transform aip = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            if (canvas.transform.GetChild(i).name == "AnimalInfoPanel")
            { aip = canvas.transform.GetChild(i); break; }
        }
        if (aip == null) return;

        // Bottom-right corner
        RectTransform rt = aip.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-10, 10);
        rt.sizeDelta = new Vector2(280, 230);

        Image img = aip.GetComponent<Image>();
        if (img != null) img.color = new Color(0.06f, 0.08f, 0.16f, 0.93f);

        // Fix child layout positions
        FixText(aip, "AnimalName",  new Vector2(0, -8),   new Vector2(-20, 28), TextAnchor.MiddleCenter, 20, true, FontStyle.Bold);
        FixText(aip, "AnimalType",  new Vector2(0, -38),  new Vector2(-20, 22), TextAnchor.MiddleCenter, 14, true, FontStyle.Italic);
        FixText(aip, "HungerLabel", new Vector2(12, -68), new Vector2(70, 22),  TextAnchor.MiddleLeft, 14, false, FontStyle.Normal);
        FixText(aip, "HappyLabel",  new Vector2(12, -98), new Vector2(70, 22),  TextAnchor.MiddleLeft, 14, false, FontStyle.Normal);
        FixText(aip, "ProductionText", new Vector2(0,-135), new Vector2(-20,22), TextAnchor.MiddleCenter, 16, true, FontStyle.Bold);

        FixSlider(aip, "HungerSlider",    new Vector2(90, -68), new Vector2(170, 22), new Color(0.9f, 0.3f, 0.2f));
        FixSlider(aip, "HappinessSlider", new Vector2(90, -98), new Vector2(170, 22), new Color(0.3f, 0.8f, 0.4f));

        // Add "Press E" instruction
        Transform instrT = aip.Find("InstructionText");
        if (instrT == null)
        {
            GameObject obj = new GameObject("InstructionText");
            obj.layer = 5;
            obj.transform.SetParent(aip, false);
            RectTransform irt = obj.AddComponent<RectTransform>();
            irt.anchorMin = new Vector2(0, 1);
            irt.anchorMax = new Vector2(1, 1);
            irt.pivot = new Vector2(0.5f, 1);
            irt.anchoredPosition = new Vector2(0, -170);
            irt.sizeDelta = new Vector2(-20, 45);
            obj.AddComponent<CanvasRenderer>();
            Text txt = obj.AddComponent<Text>();
            txt.text = "Press E: Feed first, then Pet\nHunger/Happiness reset daily";
            txt.fontSize = 12;
            txt.fontStyle = FontStyle.Italic;
            txt.color = new Color(0.7f, 0.7f, 0.7f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        EditorUtility.SetDirty(aip.gameObject);
        Debug.Log("[FixAll] AnimalInfoPanel layout fixed");
    }

    // ═══════════════════════════════════════════════════════
    //  11. FIX FRIENDSHIP PANEL LAYOUT
    // ═══════════════════════════════════════════════════════

    static void FixFriendshipPanelLayout()
    {
        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;

        Transform fp = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            if (canvas.transform.GetChild(i).name == "FriendshipPanel")
            { fp = canvas.transform.GetChild(i); break; }
        }
        if (fp == null) return;

        RectTransform rt = fp.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(320, 400);

        Image img = fp.GetComponent<Image>();
        if (img != null) img.color = new Color(0.06f, 0.06f, 0.14f, 0.93f);

        // Add close hint
        Transform instrT = fp.Find("CloseHint");
        if (instrT == null)
        {
            GameObject obj = new GameObject("CloseHint");
            obj.layer = 5;
            obj.transform.SetParent(fp, false);
            RectTransform irt = obj.AddComponent<RectTransform>();
            irt.anchorMin = new Vector2(0, 0);
            irt.anchorMax = new Vector2(1, 0);
            irt.pivot = new Vector2(0.5f, 0);
            irt.anchoredPosition = new Vector2(0, 5);
            irt.sizeDelta = new Vector2(-20, 25);
            obj.AddComponent<CanvasRenderer>();
            Text txt = obj.AddComponent<Text>();
            txt.text = "Press F to close";
            txt.fontSize = 13;
            txt.fontStyle = FontStyle.Italic;
            txt.color = new Color(0.6f, 0.6f, 0.6f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        EditorUtility.SetDirty(fp.gameObject);
        Debug.Log("[FixAll] FriendshipPanel layout fixed");
    }

    // ═══════════════════════════════════════════════════════
    //  12. FIX PRODUCTION PANEL LAYOUT
    // ═══════════════════════════════════════════════════════

    static void FixProductionPanelLayout()
    {
        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;

        Transform pp = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            if (canvas.transform.GetChild(i).name == "ProductionPanel")
            { pp = canvas.transform.GetChild(i); break; }
        }
        if (pp == null) return;

        RectTransform rt = pp.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = new Vector2(-10, 0);
        rt.sizeDelta = new Vector2(270, 320);

        Image img = pp.GetComponent<Image>();
        if (img != null) img.color = new Color(0.06f, 0.08f, 0.16f, 0.93f);

        // Add close hint
        Transform instrT = pp.Find("CloseHint");
        if (instrT == null)
        {
            GameObject obj = new GameObject("CloseHint");
            obj.layer = 5;
            obj.transform.SetParent(pp, false);
            RectTransform irt = obj.AddComponent<RectTransform>();
            irt.anchorMin = new Vector2(0, 0);
            irt.anchorMax = new Vector2(1, 0);
            irt.pivot = new Vector2(0.5f, 0);
            irt.anchoredPosition = new Vector2(0, 5);
            irt.sizeDelta = new Vector2(-20, 25);
            obj.AddComponent<CanvasRenderer>();
            Text txt = obj.AddComponent<Text>();
            txt.text = "Press G to close";
            txt.fontSize = 13;
            txt.fontStyle = FontStyle.Italic;
            txt.color = new Color(0.6f, 0.6f, 0.6f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        EditorUtility.SetDirty(pp.gameObject);
        Debug.Log("[FixAll] ProductionPanel layout fixed");
    }

    // ═══════════════════════════════════════════════════════
    //  13. WIRE COLLECT-ALL BUTTON
    // ═══════════════════════════════════════════════════════

    static void WireCollectAllButton()
    {
        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;

        Transform prodPanel = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            if (canvas.transform.GetChild(i).name == "ProductionPanel")
            { prodPanel = canvas.transform.GetChild(i); break; }
        }
        if (prodPanel == null) return;

        Transform btnTransform = prodPanel.Find("CollectAllBtn");
        if (btnTransform == null) return;

        Button btn = btnTransform.GetComponent<Button>();
        AnimalProductionUI apui = Object.FindFirstObjectByType<AnimalProductionUI>();
        if (btn == null || apui == null) return;

        // Wire via SerializedProperty (safer)
        var so = new SerializedObject(btn);
        var callsProp = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        callsProp.arraySize = 1;
        var call = callsProp.GetArrayElementAtIndex(0);
        call.FindPropertyRelative("m_Target").objectReferenceValue = apui;
        call.FindPropertyRelative("m_MethodName").stringValue = "CollectAllProducts";
        call.FindPropertyRelative("m_Mode").intValue = 1;
        call.FindPropertyRelative("m_CallState").intValue = 2;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(btn);

        Debug.Log("[FixAll] CollectAll button wired");
    }

    // ═══════════════════════════════════════════════════════
    //  14. FIX NPC NAMES
    // ═══════════════════════════════════════════════════════

    static void FixNPCNames()
    {
        FixOneName("NPC_ShopKeeper", "ShopKeeper");
        FixOneName("NPC_Farmer", "Farmer");
        FixOneName("NPC_Villager", "Villager");
    }

    static void FixOneName(string goName, string displayName)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) return;

        NPCDialogue dialogue = go.GetComponent<NPCDialogue>();
        if (dialogue != null)
        {
            var so = new SerializedObject(dialogue);
            so.FindProperty("_npcName").stringValue = displayName;
            so.ApplyModifiedProperties();
        }

        NPCNameDisplay nameDisplay = go.GetComponent<NPCNameDisplay>();
        if (nameDisplay != null)
        {
            var so = new SerializedObject(nameDisplay);
            so.FindProperty("_npcName").stringValue = displayName;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(go);
    }

    // ═══════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════

    static void FixText(Transform parent, string childName, Vector2 pos, Vector2 size,
        TextAnchor anchor, int fontSize, bool fullWidth, FontStyle style)
    {
        Transform child = parent.Find(childName);
        if (child == null) return;
        child.gameObject.layer = 5;

        RectTransform rt = child.GetComponent<RectTransform>();
        if (fullWidth)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
        }
        else
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
        }
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text txt = child.GetComponent<Text>();
        if (txt != null)
        {
            txt.fontSize = fontSize;
            txt.alignment = anchor;
            txt.fontStyle = style;
            txt.color = Color.white;
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    static void FixSlider(Transform parent, string childName, Vector2 pos, Vector2 size, Color fillColor)
    {
        Transform child = parent.Find(childName);
        if (child == null) return;
        child.gameObject.layer = 5;

        RectTransform rt = child.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Slider slider = child.GetComponent<Slider>();
        if (slider != null && slider.fillRect != null)
        {
            Image fillImg = slider.fillRect.GetComponent<Image>();
            if (fillImg != null) fillImg.color = fillColor;
        }

        // Fix child layers
        SetLayerRecursive(child.gameObject, 5);
    }

    /// <summary>
    /// Create a Text element. When fullWidth=true, stretches across parent width.
    /// </summary>
    static GameObject MakeText(string name, Transform parent, string content,
        int fontSize, TextAnchor anchor, Vector2 pos, Vector2 size, bool fullWidth)
    {
        GameObject obj = new GameObject(name);
        obj.layer = 5;
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        if (fullWidth)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size; // x is offset from edges, y is height
        }
        else
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
        obj.AddComponent<CanvasRenderer>();
        Text txt = obj.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = anchor;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return obj;
    }

    static GameObject MakeButton(string name, Transform parent,
        string label, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.layer = 5;
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        btnObj.AddComponent<CanvasRenderer>();
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        btnObj.AddComponent<Button>();

        // label text
        GameObject textObj = new GameObject("Text");
        textObj.layer = 5;
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        textObj.AddComponent<CanvasRenderer>();
        Text txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = 15;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btnObj;
    }
}
