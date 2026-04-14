using UnityEditor;
using UnityEngine;

/// <summary>
/// Comprehensive scene fix: positions, sprites, UI visibility, camera.
/// Run from menu: Tools > Fix M3 Scene
/// </summary>
public class FixM3Scene
{
    [MenuItem("Tools/Fix M3 Scene")]
    public static void Fix()
    {
        FixSpriteImportSettings();
        FixPositions();
        FixSprites();
        FixDialoguePanel();
        FixCamera();

        // Mark scene dirty so changes are saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== M3 Scene Fix Complete ===");
    }

    static void FixSpriteImportSettings()
    {
        // All pixel art sprites must use: pixelsPerUnit=16, filterMode=Point, spriteMode=Single
        string[] spritePaths = {
            "Assets/Art/Sprites/npc_farmer.png",
            "Assets/Art/Sprites/npc_villager.png",
            "Assets/Art/Sprites/animal_cow.png",
            "Assets/Art/Sprites/animal_sheep.png",
        };

        foreach (string path in spritePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning($"[Fix] No importer for {path}"); continue; }

            bool changed = false;

            if (importer.spritePixelsPerUnit != 16f)
            {
                importer.spritePixelsPerUnit = 16f;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log($"[Fix] Reimported {path}: PPU=16, Point filter, Uncompressed");
            }
            else
            {
                Debug.Log($"[Fix] {path} already correct");
            }
        }
    }

    static void FixPositions()
    {
        // All positions must be Z=0 for 2D, spread on X/Y
        SetPos("Player",          0f,    0f);

        // NPCs spread out so they're clearly visible
        SetPos("NPC_ShopKeeper", -3f,    0f);
        SetPos("NPC_Farmer",      4f,    0f);
        SetPos("NPC_Villager",   -6f,    2f);

        // Animals in a farm area (right side, lower)
        SetPos("Animal_Chicken",  3f,   -2f);
        SetPos("Animal_Cow",      6f,   -2f);
        SetPos("Animal_Sheep",    6f,    1f);

        // Waypoints spread across map — Z=0!
        SetPos("WP_Home",       -8f,    3f);
        SetPos("WP_Shop",       -3f,   -3f);
        SetPos("WP_Farm",        7f,   -3f);
        SetPos("WP_Park",        0f,    4f);

        // Non-visible objects
        SetPos("EventSystem",    0f,    0f);
        SetPos("GameManagers",   0f,    0f);

        Debug.Log("[Fix] All positions set to 2D layout (Z=0)");
    }

    static void SetPos(string name, float x, float y)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) { Debug.LogWarning($"[Fix] GameObject not found: {name}"); return; }
        go.transform.position = new Vector3(x, y, 0f);
    }

    static void FixSprites()
    {
        // Load all sprites from Assets/Art/Sprites/
        AssignSprite("NPC_ShopKeeper", "Assets/Art/Sprites/npc_shopkeeper.png");
        AssignSprite("NPC_Farmer",     "Assets/Art/Sprites/npc_farmer.png");
        AssignSprite("NPC_Villager",   "Assets/Art/Sprites/npc_villager.png");
        AssignSprite("Animal_Chicken", "Assets/Art/Sprites/animal_chicken.png");
        AssignSprite("Animal_Cow",     "Assets/Art/Sprites/animal_cow.png");
        AssignSprite("Animal_Sheep",   "Assets/Art/Sprites/animal_sheep.png");

        Debug.Log("[Fix] All sprites assigned");
    }

    static void AssignSprite(string goName, string assetPath)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[Fix] Not found: {goName}"); return; }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogWarning($"[Fix] No SpriteRenderer on {goName}"); return; }

        // Load the sprite from the asset
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            // Try loading all sub-assets (sprite might be a sub-asset of the texture)
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object asset in assets)
            {
                if (asset is Sprite s)
                {
                    sprite = s;
                    break;
                }
            }
        }

        if (sprite != null)
        {
            sr.sprite = sprite;
            Debug.Log($"[Fix] Sprite assigned to {goName}: {sprite.name}");
        }
        else
        {
            Debug.LogWarning($"[Fix] Could not load sprite from {assetPath}");
        }
    }

    static void FixDialoguePanel()
    {
        // Find dialogue panel in Canvas and ensure it's hidden
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

    static void FixCamera()
    {
        // Make camera orthographic with good size for 2D viewing
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.25f); // dark blue background

        Debug.Log("[Fix] Camera set to orthographic, size 7");
    }
}
