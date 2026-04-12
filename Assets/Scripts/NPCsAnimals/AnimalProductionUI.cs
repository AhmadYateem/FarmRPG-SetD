using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Shows collected animal products and allows collecting when ready.
/// Press G to toggle the production overview panel.
/// </summary>
public class AnimalProductionUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform  _listContainer;
    [SerializeField] private Text       _inventoryText;

    // mock inventory — just a list of strings
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
                    Debug.Log($"Collected {product} from {data.animalName}!");
                }
            }
        }
        Refresh();
    }

    private void Refresh()
    {
        if (_inventoryText == null) return;

        if (_collectedProducts.Count == 0)
        {
            _inventoryText.text = "No products collected yet.";
            return;
        }

        // count products
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (string product in _collectedProducts)
        {
            if (!counts.ContainsKey(product)) counts[product] = 0;
            counts[product]++;
        }

        string display = "Collected Products:\n";
        foreach (var kvp in counts)
        {
            display += $"  {kvp.Key} x{kvp.Value}\n";
        }
        _inventoryText.text = display;
    }
}
