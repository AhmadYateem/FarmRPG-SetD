using UnityEngine;

// one time-slot in an NPC's daily schedule
[System.Serializable]
public class ScheduleEntry
{
    public int hour;                // 0-23, matches OnTimeChanged value
    public Transform location;     // waypoint the NPC walks to at this hour
}
