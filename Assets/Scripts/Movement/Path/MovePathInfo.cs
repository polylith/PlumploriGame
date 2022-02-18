using UnityEngine;

/// <summary>
/// This struct holds the path data for a character to get
/// to a target.
/// </summary>
public struct MovePathInfo
{
    public bool HasPoints { get => null != points && points.Length > 0; }
    public Vector3[] points;
    public Vector3 target;
    public Vector3 LastPoint { get => points.Length > 0 ? points[points.Length - 1] : target; }

    /// <summary>
    /// Calculates the distance from a given position to the end of
    /// the path. if there are no path data, the distance to the
    /// target is caluclated.
    /// </summary>
    /// <param name="position">current position</param>
    /// <returns>distance to the last path point</returns>
    public float Distance(Vector3 position)
    {
        Vector3 point = LastPoint;
        point.y = position.y;
        Debug.DrawLine(position, target, Color.yellow, 15f);
        return Vector3.Distance(position, point);
    }
}
