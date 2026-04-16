using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Always-visible inventory counter in the top-right corner.
/// Listens when products are collected and updates display.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private Text _inventoryText;

    private Dictionary<string, int> _items = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Refresh();
    }

    public void AddItem(string itemName)
    {
        if (!_items.ContainsKey(itemName)) _items[itemName] = 0;
        _items[itemName]++;
        Refresh();
    }

    private void Refresh()
    {
        if (_inventoryText == null) return;

        if (_items.Count == 0)
        {
            _inventoryText.text = "Inventory: empty";
            return;
        }

        string display = "Inventory: ";
        bool first = true;
        foreach (var kvp in _items)
        {
            if (!first) display += " | ";
            display += $"{kvp.Key} x{kvp.Value}";
            first = false;
        }
        _inventoryText.text = display;
    }
}
