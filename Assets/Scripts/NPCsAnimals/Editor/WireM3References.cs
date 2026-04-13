using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires all Inspector references for Milestone 3 NPCs, animals, and managers.
/// Run from menu: Tools > Wire M3 References
/// </summary>
public class WireM3References
{
    [MenuItem("Tools/Wire M3 References")]
    public static void Wire()
    {
        WireNPCDialogueRefs();
        WireAnimalData();
        WireMockGridWaypoints();
        WireNPCSchedules();
        WireAnimalManagerList();

        Debug.Log("All Milestone 3 references wired!");
    }

    static void WireNPCDialogueRefs()
    {
        // Find the dialogue panel and its children
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Transform dialoguePanel = null;
        Transform npcNameText = null;
        Transform npcText = null;
        Transform choicesContainer = null;

        foreach (Transform child in canvas.transform)
        {
            if (child.name.Contains("Dialogue") || child.name.Contains("dialogue"))
            {
                dialoguePanel = child;
                break;
            }
        }
        if (dialoguePanel == null) { Debug.LogWarning("DialoguePanel not found"); return; }

        // Find child elements
        npcNameText = dialoguePanel.Find("NPC_Name_Text");
        npcText = dialoguePanel.Find("NPC_Text");
        choicesContainer = dialoguePanel.Find("ChoicesContainer");

        // Find the ChoiceButton prefab
        GameObject choiceButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ChoiceButton.prefab");

        // Wire all NPCDialogue components
        NPCDialogue[] npcs = Object.FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
        foreach (NPCDialogue npc in npcs)
        {
            var so = new SerializedObject(npc);

            var panelProp = so.FindProperty("_dialoguePanel");
            if (panelProp != null && panelProp.objectReferenceValue == null)
                panelProp.objectReferenceValue = dialoguePanel.gameObject;

            var nameTextProp = so.FindProperty("_npcNameText");
            if (nameTextProp != null && npcNameText != null)
                nameTextProp.objectReferenceValue = npcNameText.GetComponent<Text>();

            var textProp = so.FindProperty("_npcText");
            if (textProp != null && (textProp.objectReferenceValue == null) && npcText != null)
                textProp.objectReferenceValue = npcText.GetComponent<Text>();

            var prefabProp = so.FindProperty("_choiceButtonPrefab");
            if (prefabProp != null && prefabProp.objectReferenceValue == null && choiceButtonPrefab != null)
                prefabProp.objectReferenceValue = choiceButtonPrefab;

            var containerProp = so.FindProperty("_choicesContainer");
            if (containerProp != null && containerProp.objectReferenceValue == null && choicesContainer != null)
                containerProp.objectReferenceValue = choicesContainer;

            so.ApplyModifiedProperties();
            Debug.Log($"Wired NPCDialogue on {npc.gameObject.name}");
        }
    }

    static void WireAnimalData()
    {
        // Wire AnimalData for each animal
        string[] animalNames = { "Animal_Chicken", "Animal_Cow", "Animal_Sheep" };
        AnimalData.AnimalType[] types = { AnimalData.AnimalType.Chicken, AnimalData.AnimalType.Cow, AnimalData.AnimalType.Sheep };

        for (int i = 0; i < animalNames.Length; i++)
        {
            GameObject go = GameObject.Find(animalNames[i]);
            if (go == null) continue;

            AnimalController ac = go.GetComponent<AnimalController>();
            if (ac == null) continue;

            var so = new SerializedObject(ac);
            var dataProp = so.FindProperty("_animalData");
            if (dataProp != null)
            {
                dataProp.FindPropertyRelative("animalName").stringValue = animalNames[i].Replace("Animal_", "");
                dataProp.FindPropertyRelative("animalType").enumValueIndex = (int)types[i];
                dataProp.FindPropertyRelative("hunger").intValue = 50;
                dataProp.FindPropertyRelative("happiness").intValue = 50;
                dataProp.FindPropertyRelative("productionReady").boolValue = false;
            }
            so.ApplyModifiedProperties();
            Debug.Log($"Wired AnimalData on {animalNames[i]}");
        }
    }

    static void WireMockGridWaypoints()
    {
        MockGridManager mgm = Object.FindFirstObjectByType<MockGridManager>();
        if (mgm == null) return;

        Transform[] waypoints = new Transform[4];
        waypoints[0] = GameObject.Find("WP_Home")?.transform;
        waypoints[1] = GameObject.Find("WP_Shop")?.transform;
        waypoints[2] = GameObject.Find("WP_Farm")?.transform;
        waypoints[3] = GameObject.Find("WP_Park")?.transform;

        var so = new SerializedObject(mgm);
        var wpProp = so.FindProperty("_waypoints");
        wpProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            wpProp.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];
        }
        so.ApplyModifiedProperties();
        Debug.Log("Wired MockGridManager waypoints");
    }

    static void WireNPCSchedules()
    {
        // Give each NPC a simple schedule
        Transform wpHome = GameObject.Find("WP_Home")?.transform;
        Transform wpShop = GameObject.Find("WP_Shop")?.transform;
        Transform wpFarm = GameObject.Find("WP_Farm")?.transform;
        Transform wpPark = GameObject.Find("WP_Park")?.transform;

        // ShopKeeper: Shop in morning, Park in evening
        WireSchedule("NPC_ShopKeeper", new[] {
            (6, wpShop), (12, wpPark), (18, wpShop)
        });

        // Farmer: Farm in morning, Home at night
        WireSchedule("NPC_Farmer", new[] {
            (6, wpFarm), (12, wpHome), (18, wpFarm)
        });

        // Villager: Home in morning, Park in afternoon, Shop in evening
        WireSchedule("NPC_Villager", new[] {
            (6, wpHome), (10, wpPark), (16, wpShop)
        });
    }

    static void WireSchedule(string npcName, (int hour, Transform location)[] entries)
    {
        GameObject go = GameObject.Find(npcName);
        if (go == null) return;
        NPCSchedule schedule = go.GetComponent<NPCSchedule>();
        if (schedule == null) return;

        var so = new SerializedObject(schedule);
        var listProp = so.FindProperty("_schedule");
        listProp.arraySize = entries.Length;
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = listProp.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("hour").intValue = entries[i].hour;
            entry.FindPropertyRelative("location").objectReferenceValue = entries[i].location;
        }
        so.ApplyModifiedProperties();
        Debug.Log($"Wired schedule for {npcName}");
    }

    static void WireAnimalManagerList()
    {
        AnimalManager am = Object.FindFirstObjectByType<AnimalManager>();
        if (am == null) return;

        AnimalController[] animals = Object.FindObjectsByType<AnimalController>(FindObjectsSortMode.None);
        var so = new SerializedObject(am);
        var listProp = so.FindProperty("_animals");
        listProp.arraySize = animals.Length;
        for (int i = 0; i < animals.Length; i++)
        {
            listProp.GetArrayElementAtIndex(i).objectReferenceValue = animals[i];
        }
        so.ApplyModifiedProperties();
        Debug.Log($"Wired AnimalManager with {animals.Length} animals");
    }
}
