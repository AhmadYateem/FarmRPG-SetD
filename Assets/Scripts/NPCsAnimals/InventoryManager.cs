using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton inventory — tracks collected products (Egg, Milk, Wool).
/// Always-visible HUD text shows current counts.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private Text _inventoryHudText;

    private Dictionary<string, int> _items = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameEvents.OnProductCollected.AddListener(OnProductCollected);
        RefreshHUD();
    }

    void OnDestroy()
    {
        GameEvents.OnProductCollected.RemoveListener(OnProductCollected);
    }

    private void OnProductCollected(string productName)
    {
        AddItem(productName);
    }

    public void AddItem(string itemName)
    {
        if (!_items.ContainsKey(itemName)) _items[itemName] = 0;
        _items[itemName]++;
        RefreshHUD();
    }

    public int GetCount(string itemName)
    {
        return _items.ContainsKey(itemName) ? _items[itemName] : 0;
    }

    private void RefreshHUD()
    {
        if (_inventoryHudText == null) return;

        int eggs = GetCount("Egg");
        int milk = GetCount("Milk");
        int wool = GetCount("Wool");

        _inventoryHudText.text = $"Egg: {eggs}  |  Milk: {milk}  |  Wool: {wool}";
    }
}
