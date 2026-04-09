using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves the NPC between waypoints based on a daily schedule.
/// Subscribes to GameEvents.OnTimeChanged and walks to the matching waypoint.
/// </summary>
public class NPCSchedule : MonoBehaviour
{
    [SerializeField] private List<ScheduleEntry> _schedule = new List<ScheduleEntry>();
    [SerializeField] private float _moveSpeed = 2f;

    private Animator _animator;
    private Coroutine _moveCoroutine;
    private bool _isMoving;

    void Start()
    {
        _animator = GetComponent<Animator>();
        GameEvents.OnTimeChanged.AddListener(OnTimeChanged);
    }

    void OnDestroy()
    {
        GameEvents.OnTimeChanged.RemoveListener(OnTimeChanged);
    }

    private void OnTimeChanged(int hour)
    {
        // find the schedule entry for this hour
        foreach (ScheduleEntry entry in _schedule)
        {
            if (entry.hour == hour && entry.location != null)
            {
                MoveTo(entry.location.position);
                return;
            }
        }
    }

    [ContextMenu("Test Move to First Waypoint")]
    private void TestMove()
    {
        if (_schedule.Count > 0 && _schedule[0].location != null)
            MoveTo(_schedule[0].location.position);
    }

    private void MoveTo(Vector3 target)
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveCoroutine(target));
    }

    private IEnumerator MoveCoroutine(Vector3 target)
    {
        _isMoving = true;

        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 direction = (target - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;

            // flip sprite based on movement direction
            if (direction.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            // animate if animator exists
            if (_animator != null)
                _animator.SetFloat("Speed", _moveSpeed);

            yield return null;
        }

        transform.position = target;
        _isMoving = false;

        if (_animator != null)
            _animator.SetFloat("Speed", 0f);
    }

    public bool IsMoving() => _isMoving;
}
