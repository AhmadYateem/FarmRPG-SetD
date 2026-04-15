using UnityEngine;
using System.Collections;

/// <summary>
/// Visual feedback when an animal is fed or petted — flashes the sprite colour.
/// Attach to every animal that has an AnimalController.
/// </summary>
public class InteractionFeedback : MonoBehaviour
{
    [SerializeField] private Color _feedColor = Color.green;
    [SerializeField] private Color _petColor  = new Color(1f, 0.5f, 0.8f); // pink
    [SerializeField] private float _flashDuration = 0.35f;

    private SpriteRenderer _sr;
    private Color _originalColor;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _originalColor = _sr.color;

        GameEvents.OnAnimalFed.AddListener(OnFed);
        GameEvents.OnAnimalPetted.AddListener(OnPetted);
    }

    void OnDestroy()
    {
        GameEvents.OnAnimalFed.RemoveListener(OnFed);
        GameEvents.OnAnimalPetted.RemoveListener(OnPetted);
    }

    private void OnFed(string animalName)
    {
        if (IsThisAnimal(animalName))
            StartCoroutine(FlashColor(_feedColor));
    }

    private void OnPetted(string animalName)
    {
        if (IsThisAnimal(animalName))
            StartCoroutine(FlashColor(_petColor));
    }

    private bool IsThisAnimal(string animalName)
    {
        AnimalController ac = GetComponent<AnimalController>();
        return ac != null && ac.GetAnimalData() != null && ac.GetAnimalData().animalName == animalName;
    }

    private IEnumerator FlashColor(Color color)
    {
        if (_sr == null) yield break;
        _sr.color = color;
        yield return new WaitForSeconds(_flashDuration);
        _sr.color = _originalColor;
    }
}
