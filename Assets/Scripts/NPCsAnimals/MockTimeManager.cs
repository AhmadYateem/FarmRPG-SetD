using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mock time system for testing NPC schedules and animal production.
/// Has buttons to advance time by 1 hour or skip to next day.
/// </summary>
public class MockTimeManager : MonoBehaviour
{
    [SerializeField] private Text _timeDisplay;
    [SerializeField] private Button _advanceTimeButton;
    [SerializeField] private Button _advanceDayButton;

    private int _currentHour = 6;  // start at 6 AM
    private int _currentDay  = 1;

    void Start()
    {
        if (_advanceTimeButton != null)
            _advanceTimeButton.onClick.AddListener(AdvanceTime);
        if (_advanceDayButton != null)
            _advanceDayButton.onClick.AddListener(AdvanceDay);

        UpdateDisplay();
    }

    [ContextMenu("Advance Time +1 Hour")]
    public void AdvanceTime()
    {
        _currentHour++;
        if (_currentHour >= 24)
        {
            _currentHour = 0;
            _currentDay++;
            GameEvents.OnDayAdvanced.Invoke();
        }
        GameEvents.OnTimeChanged.Invoke(_currentHour);
        UpdateDisplay();
    }

    [ContextMenu("Advance Day")]
    public void AdvanceDay()
    {
        _currentDay++;
        _currentHour = 6;
        GameEvents.OnDayAdvanced.Invoke();
        GameEvents.OnTimeChanged.Invoke(_currentHour);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_timeDisplay != null)
            _timeDisplay.text = $"Day {_currentDay}  {_currentHour}:00";
    }

    public int GetCurrentHour() => _currentHour;
    public int GetCurrentDay()  => _currentDay;
}
