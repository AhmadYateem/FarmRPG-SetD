using UnityEngine;

/// <summary>
/// Holds named waypoints for NPC schedule movement.
/// Place empty GameObjects in the scene and drag them into the array.
/// </summary>
public class MockGridManager : MonoBehaviour
{
    [SerializeField] private Transform[] _waypoints;

    /// <summary>
    /// Find a waypoint by name (e.g. "Home", "Shop", "Farm", "Park").
    /// </summary>
    public Transform GetWaypoint(string waypointName)
    {
        foreach (Transform wp in _waypoints)
        {
            if (wp != null && wp.name == waypointName)
                return wp;
        }
        Debug.LogWarning($"Waypoint '{waypointName}' not found!");
        return null;
    }

    public Transform[] GetAllWaypoints() => _waypoints;
}
