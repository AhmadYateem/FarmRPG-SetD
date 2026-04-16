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
        // check production readiness for each animal
        foreach (AnimalController animal in _animals)
        {
            AnimalData data = animal.GetAnimalData();
            if (data == null) continue;

            // animal produces if hunger >= 50 and happiness >= 50 (needs feeding + petting)
            if (data.hunger >= 50 && data.happiness >= 50)
            {
                data.productionReady = true;
                Debug.Log($"{data.animalName} has a product ready to collect!");
            }
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
        switch (data.animalType)
        {
            case AnimalData.AnimalType.Cow:     return "Milk";
            case AnimalData.AnimalType.Sheep:   return "Wool";
            case AnimalData.AnimalType.Chicken: return "Egg";
            default: return "Product";
        }
    }
}
