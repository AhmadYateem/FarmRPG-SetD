using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Comprehensive scene fix — sprite slicing, positions, UI, camera.
/// Run from menu: Tools > Fix M3 Scene
/// </summary>
public class FixM3Scene
{
    [MenuItem("Tools/Fix M3 Scene")]
    public static void Fix()
    {
        SliceAndImportSprites();
        AssetDatabase.Refresh();

        FixPositions();
        AssignFirstFrameSprites();
        FixDialoguePanel();
        FixCamera();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== M3 Scene Fix Complete ===");
    }

    // ── SPRITE SLICING ──────────────────────────────────────

    static void SliceAndImportSprites()
    {
        // NPC sheets: 864x64, 9 frames, grid cell 96x64
        SliceSheet("Assets/Art/Sprites/npc_farmer.png",   "npc_farmer",   96, 64, 9);
        SliceSheet("Assets/Art/Sprites/npc_villager.png", "npc_villager", 96, 64, 9);

        // Animal sheets: 128x32, 4 frames, grid cell 32x32
        SliceSheet("Assets/Art/Sprites/animal_cow.png",   "animal_cow",   32, 32, 4);
        SliceSheet("Assets/Art/Sprites/animal_sheep.png", "animal_sheep", 32, 32, 4);
    }

    static void SliceSheet(string path, string baseName, int cellW, int cellH, int count)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { Debug.LogWarning($"[Fix] No importer: {path}"); return; }

        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // Use ISpriteEditorDataProvider for Unity 6+
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = new List<SpriteRect>();
        for (int i = 0; i < count; i++)
        {
            var sr = new SpriteRect
            {
                name = $"{baseName}_{i}",
                rect = new Rect(i * cellW, 0, cellW, cellH),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                spriteID = GUID.Generate()
            };
            spriteRects.Add(sr);
        }

        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();

        var assetImporter = dataProvider.targetObject as AssetImporter;
        assetImporter.SaveAndReimport();
        Debug.Log($"[Fix] Sliced {path}: {count} frames @ {cellW}x{cellH}, PPU=16");
    }

    // ── SPRITE ASSIGNMENT ───────────────────────────────────

    static void AssignFirstFrameSprites()
    {
        AssignSubSprite("NPC_ShopKeeper", "Assets/Art/Sprites/npc_shopkeeper.png", "npc_shopkeeper_0");
        AssignSubSprite("NPC_Farmer",     "Assets/Art/Sprites/npc_farmer.png",     "npc_farmer_0");
        AssignSubSprite("NPC_Villager",   "Assets/Art/Sprites/npc_villager.png",   "npc_villager_0");
        AssignSubSprite("Animal_Chicken", "Assets/Art/Sprites/animal_chicken.png", "animal_chicken_0");
        AssignSubSprite("Animal_Cow",     "Assets/Art/Sprites/animal_cow.png",     "animal_cow_0");
        AssignSubSprite("Animal_Sheep",   "Assets/Art/Sprites/animal_sheep.png",   "animal_sheep_0");
    }

    static void AssignSubSprite(string goName, string texturePath, string spriteName)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[Fix] Not found: {goName}"); return; }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogWarning($"[Fix] No SpriteRenderer on {goName}"); return; }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        Sprite target = null;
        foreach (Object asset in assets)
        {
            if (asset is Sprite s && s.name == spriteName)
            {
                target = s;
                break;
            }
        }

        if (target != null)
        {
            sr.sprite = target;
            EditorUtility.SetDirty(sr);
            Debug.Log($"[Fix] Assigned {spriteName} to {goName}");
        }
        else
        {
            Debug.LogWarning($"[Fix] Sprite '{spriteName}' not found in {texturePath}");
        }
    }

    // ── POSITIONS ───────────────────────────────────────────

    static void FixPositions()
    {
        SetPos("Player",          0f,    0f);

        SetPos("NPC_ShopKeeper", -4f,    0f);
        SetPos("NPC_Farmer",      5f,    2f);
        SetPos("NPC_Villager",   -6f,    3f);

        SetPos("Animal_Chicken",  3f,   -3f);
        SetPos("Animal_Cow",      7f,   -3f);
        SetPos("Animal_Sheep",   -3f,   -4f);

        SetPos("WP_Home",       -8f,    4f);
        SetPos("WP_Shop",       -4f,   -2f);
        SetPos("WP_Farm",        7f,   -2f);
        SetPos("WP_Park",        2f,    5f);

        SetPos("EventSystem",    0f,    0f);
        SetPos("GameManagers",   0f,    0f);

        Debug.Log("[Fix] Positions set (Z=0, well spaced)");
    }

    static void SetPos(string name, float x, float y)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) return;
        go.transform.position = new Vector3(x, y, 0f);
    }

    // ── UI FIXES ────────────────────────────────────────────

    static void FixDialoguePanel()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        foreach (Transform child in canvas.transform)
        {
            if (child.name.Contains("Dialogue") || child.name.Contains("dialogue"))
            {
                child.gameObject.SetActive(false);
                Debug.Log($"[Fix] Hidden dialogue panel: {child.name}");
                break;
            }
        }
    }

    // ── CAMERA ──────────────────────────────────────────────

    static void FixCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.25f);
        EditorUtility.SetDirty(cam);
        EditorUtility.SetDirty(cam.gameObject);

        Debug.Log("[Fix] Camera: orthographic, size 8");
    }
}
