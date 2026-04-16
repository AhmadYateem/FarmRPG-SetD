using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager for all animals in the scene.
/// Subscribes to OnDayAdvanced to handle daily production checks.
/// </summary>
public class AnimalManager : MonoBehaviour
{
    public static AnimalManager Instance { get; private set; }

    [SerializeField] private List<AnimalController> _animals = new List<AnimalController>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameEvents.OnDayAdvanced.AddListener(OnDayAdvanced);
    }

    void OnDestroy()
    {
        GameEvents.OnDayAdvanced.RemoveListener(OnDayAdvanced);
    }

    private void OnDayAdvanced()
    {
        foreach (AnimalController animal in _animals)
        {
            AnimalData data = animal.GetAnimalData();
            if (data == null) continue;

            // Animal produces next day if both hunger and happiness >= 50 (half the bar)
            if (data.hunger >= 50 && data.happiness >= 50)
            {
                data.productionReady = true;
                Debug.Log($"{data.animalName} has a product ready to collect!");
            }

            // Daily decrease — hunger and happiness drop each day
            data.hunger    = Mathf.Clamp(data.hunger    - 15, 0, 100);
            data.happiness = Mathf.Clamp(data.happiness - 10, 0, 100);
        }
    }

    public void RegisterAnimal(AnimalController animal)
    {
        if (!_animals.Contains(animal))
            _animals.Add(animal);
    }

    public List<AnimalController> GetAllAnimals() => _animals;

    /// <summary>
    /// Collect product from an animal (milk, wool, egg).
    /// </summary>
    public string CollectProduct(AnimalData data)
    {
        if (data == null || !data.productionReady) return null;

        data.productionReady = false;
        string product;
        switch (data.animalType)
        {
            case AnimalData.AnimalType.Cow:     product = "Milk"; break;
            case AnimalData.AnimalType.Sheep:   product = "Wool"; break;
            case AnimalData.AnimalType.Chicken: product = "Egg";  break;
            default: product = "Product"; break;
        }

        GameEvents.OnProductCollected.Invoke(product);
        return product;
    }
}
