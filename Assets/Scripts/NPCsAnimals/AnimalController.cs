using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles animal interaction — E to feed, P to pet.
/// Walking over an animal auto-collects its product if ready.
/// Animals can be fed multiple times per day.
/// </summary>
public class AnimalController : MonoBehaviour
{
    [SerializeField] private AnimalData _animalData;
    [SerializeField] private float _interactRange = 2f;
    [SerializeField] private int _feedAmount = 25;
    [SerializeField] private int _petAmount  = 15;
    [SerializeField] private float _collectRange = 1.2f;

    private Animator _animator;
    private Transform _playerTransform;
    private bool _pettedToday;
    private bool _collectedThisVisit;

    void Start()
    {
        _animator = GetComponent<Animator>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        GameEvents.OnDayAdvanced.AddListener(OnDayAdvanced);
    }

    void OnDestroy()
    {
        GameEvents.OnDayAdvanced.RemoveListener(OnDayAdvanced);
    }

    void Update()
    {
        if (_animator != null)
            _animator.SetFloat("Speed", 0f);

        if (_playerTransform == null || _animalData == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        // Press E to feed
        if (dist <= _interactRange && Keyboard.current[Key.E].wasPressedThisFrame)
        {
            if (_animalData.hunger < 100)
                Feed();
        }

        // Press P to pet
        if (dist <= _interactRange && Keyboard.current[Key.P].wasPressedThisFrame)
        {
            Pet();
        }

        // Auto-collect product when walking over the animal
        if (dist <= _collectRange && _animalData.productionReady && !_collectedThisVisit)
        {
            AutoCollect();
        }

        // Reset collect flag when player leaves range
        if (dist > _collectRange)
            _collectedThisVisit = false;
    }

    [ContextMenu("Interact")]
    public void Interact()
    {
        if (_animalData == null) return;

        // Feeding can be done multiple times
        if (_animalData.hunger < 100)
        {
            Feed();
        }
        else if (!_pettedToday)
        {
            Pet();
        }
        else
        {
            // Still allow petting for happiness
            Pet();
        }
    }

    private void Feed()
    {
        _animalData.hunger = Mathf.Clamp(_animalData.hunger + _feedAmount, 0, 100);
        GameEvents.OnAnimalFed.Invoke(_animalData.animalName);
        Debug.Log($"Fed {_animalData.animalName}! Hunger: {_animalData.hunger}");
    }

    private void Pet()
    {
        _animalData.happiness = Mathf.Clamp(_animalData.happiness + _petAmount, 0, 100);
        _pettedToday = true;
        GameEvents.OnAnimalPetted.Invoke(_animalData.animalName);
        Debug.Log($"Petted {_animalData.animalName}! Happiness: {_animalData.happiness}");
    }

    private void AutoCollect()
    {
        if (AnimalManager.Instance == null) return;
        string product = AnimalManager.Instance.CollectProduct(_animalData);
        if (product != null)
        {
            _collectedThisVisit = true;
            if (InventoryUI.Instance != null)
                InventoryUI.Instance.AddItem(product);
            GameEvents.OnAnimalFed.Invoke(_animalData.animalName); // flash feedback
            Debug.Log($"Auto-collected {product} from {_animalData.animalName}!");
        }
    }

    private void OnDayAdvanced()
    {
        _pettedToday = false;

        _animalData.hunger = Mathf.Clamp(_animalData.hunger - 15, 0, 100);
        _animalData.happiness = Mathf.Clamp(_animalData.happiness - 10, 0, 100);
    }

    public AnimalData GetAnimalData() => _animalData;
}
