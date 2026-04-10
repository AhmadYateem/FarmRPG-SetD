using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles animal interaction — feed and pet with E key when nearby.
/// Manages hunger/happiness via AnimalData and fires GameEvents.
/// </summary>
public class AnimalController : MonoBehaviour
{
    [SerializeField] private AnimalData _animalData;
    [SerializeField] private float _interactRange = 2f;
    [SerializeField] private int _feedAmount = 25;
    [SerializeField] private int _petAmount  = 15;

    private Animator _animator;
    private Transform _playerTransform;
    private bool _fedToday;
    private bool _pettedToday;

    void Start()
    {
        _animator = GetComponent<Animator>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        // reset daily flags on new day
        GameEvents.OnDayAdvanced.AddListener(OnDayAdvanced);
    }

    void OnDestroy()
    {
        GameEvents.OnDayAdvanced.RemoveListener(OnDayAdvanced);
    }

    void Update()
    {
        // idle animation
        if (_animator != null)
            _animator.SetFloat("Speed", 0f);

        if (_playerTransform == null || _animalData == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        if (dist <= _interactRange && Keyboard.current[Key.E].wasPressedThisFrame)
        {
            Interact();
        }
    }

    [ContextMenu("Interact")]
    public void Interact()
    {
        if (_animalData == null) return;

        if (!_fedToday)
        {
            Feed();
        }
        else if (!_pettedToday)
        {
            Pet();
        }
        else
        {
            Debug.Log($"{_animalData.animalName} has already been fed and petted today.");
        }
    }

    private void Feed()
    {
        _animalData.hunger = Mathf.Clamp(_animalData.hunger + _feedAmount, 0, 100);
        _fedToday = true;
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

    private void OnDayAdvanced()
    {
        _fedToday = false;
        _pettedToday = false;

        // hunger decreases each day if not fed
        _animalData.hunger = Mathf.Clamp(_animalData.hunger - 15, 0, 100);

        // happiness slowly drops
        _animalData.happiness = Mathf.Clamp(_animalData.happiness - 10, 0, 100);
    }

    public AnimalData GetAnimalData() => _animalData;
}
