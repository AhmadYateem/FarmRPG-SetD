using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Shows collected animal products and allows collecting when ready.
/// Press G to toggle the production overview panel.
/// Displays product type (Egg/Milk/Wool) with animal status.
/// </summary>
public class AnimalProductionUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform  _listContainer;
    [SerializeField] private Text       _inventoryText;

    private List<string> _collectedProducts = new List<string>();

    void Start()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current[Key.G].wasPressedThisFrame)
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        bool show = !_panel.activeSelf;
        _panel.SetActive(show);
        if (show) Refresh();
    }

    [ContextMenu("Collect All Products")]
    public void CollectAllProducts()
    {
        if (AnimalManager.Instance == null) return;

        foreach (AnimalController animal in AnimalManager.Instance.GetAllAnimals())
        {
            AnimalData data = animal.GetAnimalData();
            if (data != null && data.productionReady)
            {
                string product = AnimalManager.Instance.CollectProduct(data);
                if (product != null)
                {
                    _collectedProducts.Add(product);
                    if (InventoryUI.Instance != null)
                        InventoryUI.Instance.AddItem(product);
                    Debug.Log($"Collected {product} from {data.animalName}!");
                }
            }
        }
        Refresh();
    }

    private void Refresh()
    {
        if (_inventoryText == null) return;

        string display = "<b>== Animal Production ==</b>\n\n";

        // Show each animal's production status
        if (AnimalManager.Instance != null)
        {
            foreach (AnimalController animal in AnimalManager.Instance.GetAllAnimals())
            {
                AnimalData data = animal.GetAnimalData();
                if (data == null) continue;

                string productName = GetProductName(data.animalType);
                string icon = GetProductIcon(data.animalType);
                string status = data.productionReady ? "<color=#00FF00>READY</color>" : "<color=#888888>Not ready</color>";
                display += $"{icon} {data.animalName} ({productName}): {status}\n";
            }
        }

        // Show collected inventory
        if (_collectedProducts.Count > 0)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (string product in _collectedProducts)
            {
                if (!counts.ContainsKey(product)) counts[product] = 0;
                counts[product]++;
            }

            display += "\n<b>Collected:</b>\n";
            foreach (var kvp in counts)
            {
                string icon = GetIconForProduct(kvp.Key);
                display += $"  {icon} {kvp.Key} x{kvp.Value}\n";
            }
        }
        else
        {
            display += "\nNo products collected yet.";
        }

        _inventoryText.text = display;
    }

    private string GetProductName(AnimalData.AnimalType type)
    {
        switch (type)
        {
            case AnimalData.AnimalType.Chicken: return "Egg";
            case AnimalData.AnimalType.Cow:     return "Milk";
            case AnimalData.AnimalType.Sheep:   return "Wool";
            default: return "Product";
        }
    }

    private string GetProductIcon(AnimalData.AnimalType type)
    {
        switch (type)
        {
            case AnimalData.AnimalType.Chicken: return "\U0001F95A";
            case AnimalData.AnimalType.Cow:     return "\U0001F95B";
            case AnimalData.AnimalType.Sheep:   return "\U0001F9F6";
            default: return "\u2022";
        }
    }

    private string GetIconForProduct(string product)
    {
        switch (product)
        {
            case "Egg":  return "\U0001F95A";
            case "Milk": return "\U0001F95B";
            case "Wool": return "\U0001F9F6";
            default:     return "\u2022";
        }
    }
}
