using UnityEngine;

/// <summary>
/// Controls animal animation. Uses GetComponent and SetFloat pattern from lecture.
/// Right now just idle — wander logic will be added in Milestone 2.
/// </summary>
public class AnimalController : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private float _wanderRadius = 3f;
    [SerializeField] private float _wanderSpeed  = 1f;

    void Start()
    {
        // get the animator component — same pattern from the lecture slides
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        // for now just idle, wander movement comes in milestone 2
        _animator.SetFloat("Speed", 0f);
    }
}
