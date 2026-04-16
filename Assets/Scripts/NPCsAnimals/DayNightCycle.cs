using UnityEngine;

/// <summary>
/// Changes the camera background color based on time of day.
/// Listens to GameEvents.OnTimeChanged to smoothly transition sky colors.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    private Camera _cam;
    private Color _targetColor;
    private float _lerpSpeed = 2f;

    // Time-of-day color palette
    private static readonly Color NightColor   = new Color(0.05f, 0.05f, 0.15f);   // dark blue
    private static readonly Color DawnColor    = new Color(0.55f, 0.35f, 0.45f);    // pinkish
    private static readonly Color MorningColor = new Color(0.5f, 0.75f, 0.95f);     // light blue
    private static readonly Color NoonColor    = new Color(0.45f, 0.7f, 0.95f);     // bright blue
    private static readonly Color AfternoonColor = new Color(0.6f, 0.7f, 0.85f);    // warm blue
    private static readonly Color SunsetColor  = new Color(0.85f, 0.45f, 0.25f);    // orange
    private static readonly Color DuskColor    = new Color(0.3f, 0.2f, 0.35f);      // purple

    void Start()
    {
        _cam = Camera.main;
        if (_cam == null) return;

        _targetColor = _cam.backgroundColor;
        GameEvents.OnTimeChanged.AddListener(OnTimeChanged);
    }

    void OnDestroy()
    {
        GameEvents.OnTimeChanged.RemoveListener(OnTimeChanged);
    }

    void Update()
    {
        if (_cam != null)
            _cam.backgroundColor = Color.Lerp(_cam.backgroundColor, _targetColor, Time.deltaTime * _lerpSpeed);
    }

    private void OnTimeChanged(int hour)
    {
        _targetColor = GetSkyColor(hour);
    }

    private Color GetSkyColor(int hour)
    {
        if (hour >= 0 && hour < 5)       return NightColor;
        if (hour == 5)                    return DawnColor;
        if (hour >= 6 && hour < 8)        return MorningColor;
        if (hour >= 8 && hour < 12)       return NoonColor;
        if (hour >= 12 && hour < 16)      return AfternoonColor;
        if (hour >= 16 && hour < 18)      return SunsetColor;
        if (hour >= 18 && hour < 20)      return DuskColor;
        return NightColor; // 20-23
    }
}
