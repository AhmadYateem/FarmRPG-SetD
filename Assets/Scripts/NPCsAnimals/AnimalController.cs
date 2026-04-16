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
    private bool _collectedThisVisit;

    void Start()
    {
        _animator = GetComponent<Animator>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;
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

    private void Feed()
    {
        if (_animalData == null) return;
        _animalData.hunger = Mathf.Clamp(_animalData.hunger + _feedAmount, 0, 100);
        GameEvents.OnAnimalFed.Invoke(_animalData.animalName);
        Debug.Log($"Fed {_animalData.animalName}! Hunger: {_animalData.hunger}");
    }

    private void Pet()
    {
        if (_animalData == null) return;
        _animalData.happiness = Mathf.Clamp(_animalData.happiness + _petAmount, 0, 100);
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
            GameEvents.OnAnimalFed.Invoke(_animalData.animalName); // flash feedback
            Debug.Log($"Auto-collected {product} from {_animalData.animalName}!");
        }
    }

    public AnimalData GetAnimalData() => _animalData;
}
