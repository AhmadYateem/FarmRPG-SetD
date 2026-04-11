using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI panel showing a selected animal's stats — hunger bar, happiness bar, name.
/// Activated when the player gets close to an animal.
/// </summary>
public class AnimalInfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Text   _animalNameText;
    [SerializeField] private Text   _animalTypeText;
    [SerializeField] private Slider _hungerSlider;
    [SerializeField] private Slider _happinessSlider;
    [SerializeField] private Text   _productionText;

    [SerializeField] private float _showRange = 3f;

    private Transform _playerTransform;
    private AnimalController[] _animals;

    void Start()
    {
        if (_panel != null) _panel.SetActive(false);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        _animals = FindObjectsByType<AnimalController>(FindObjectsSortMode.None);

        // listen to feed/pet events to refresh
        GameEvents.OnAnimalFed.AddListener(OnAnimalInteracted);
        GameEvents.OnAnimalPetted.AddListener(OnAnimalInteracted);
    }

    void OnDestroy()
    {
        GameEvents.OnAnimalFed.RemoveListener(OnAnimalInteracted);
        GameEvents.OnAnimalPetted.RemoveListener(OnAnimalInteracted);
    }

    void Update()
    {
        if (_playerTransform == null || _animals == null) return;

        AnimalController closest = null;
        float closestDist = _showRange;

        foreach (AnimalController animal in _animals)
        {
            float dist = Vector3.Distance(_playerTransform.position, animal.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = animal;
            }
        }

        if (closest != null)
        {
            ShowAnimalInfo(closest.GetAnimalData());
        }
        else
        {
            if (_panel != null) _panel.SetActive(false);
        }
    }

    private void OnAnimalInteracted(string animalName)
    {
        // refresh if panel is showing
        if (_panel != null && _panel.activeSelf)
        {
            // re-check closest animal
        }
    }

    private void ShowAnimalInfo(AnimalData data)
    {
        if (data == null || _panel == null) return;

        _panel.SetActive(true);

        if (_animalNameText != null) _animalNameText.text = data.animalName;
        if (_animalTypeText != null) _animalTypeText.text = data.animalType.ToString();

        if (_hungerSlider != null)
        {
            _hungerSlider.maxValue = 100;
            _hungerSlider.value = data.hunger;
        }

        if (_happinessSlider != null)
        {
            _happinessSlider.maxValue = 100;
            _happinessSlider.value = data.happiness;
        }

        if (_productionText != null)
            _productionText.text = data.productionReady ? "Product Ready!" : "Not ready";
    }
}
