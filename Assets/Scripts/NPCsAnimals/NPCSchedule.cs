using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves the NPC between waypoints based on a daily schedule.
/// Subscribes to GameEvents.OnTimeChanged and walks to the matching waypoint.
/// Clamps movement to camera bounds so NPCs never leave the screen.
/// Uses SpriteRenderer.flipX so the name canvas doesn't reverse.
/// </summary>
public class NPCSchedule : MonoBehaviour
{
    [SerializeField] private List<ScheduleEntry> _schedule = new List<ScheduleEntry>();
    [SerializeField] private float _moveSpeed = 2f;

    private Animator _animator;
    private SpriteRenderer _sr;
    private Coroutine _moveCoroutine;
    private bool _isMoving;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        GameEvents.OnTimeChanged.AddListener(OnTimeChanged);
    }

    void OnDestroy()
    {
        GameEvents.OnTimeChanged.RemoveListener(OnTimeChanged);
    }

    private void OnTimeChanged(int hour)
    {
        foreach (ScheduleEntry entry in _schedule)
        {
            if (entry.hour == hour && entry.location != null)
            {
                Vector3 target = ClampToScreen(entry.location.position);
                MoveTo(target);
                return;
            }
        }
    }

    [ContextMenu("Test Move to First Waypoint")]
    private void TestMove()
    {
        if (_schedule.Count > 0 && _schedule[0].location != null)
            MoveTo(ClampToScreen(_schedule[0].location.position));
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

            // clamp NPC within screen bounds each frame
            transform.position = ClampToScreen(transform.position);

            // flip sprite using flipX (does NOT reverse child NameCanvas)
            if (direction.x != 0 && _sr != null)
                _sr.flipX = direction.x < 0;

            if (_animator != null)
                _animator.SetFloat("Speed", _moveSpeed);

            yield return null;
        }

        transform.position = ClampToScreen(target);
        _isMoving = false;

        if (_animator != null)
            _animator.SetFloat("Speed", 0f);
    }

    private Vector3 ClampToScreen(Vector3 worldPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return worldPos;

        float margin = 0.5f;
        Vector3 minWorld = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 maxWorld = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        worldPos.x = Mathf.Clamp(worldPos.x, minWorld.x + margin, maxWorld.x - margin);
        worldPos.y = Mathf.Clamp(worldPos.y, minWorld.y + margin, maxWorld.y - margin);
        return worldPos;
    }

    public bool IsMoving() => _isMoving;
}
