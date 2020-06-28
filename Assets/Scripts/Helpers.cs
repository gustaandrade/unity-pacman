using System.Linq;
using UnityEngine;

public static class Helpers
{
    /// <summary>
    /// Returns the Vector3 correspondent direction from MoveDirection enum value
    /// </summary>
    /// <param name="direction">MoveDirection enum value</param>
    /// <returns>A Vector3 direction correspondent</returns>
    public static Vector3 GetVectorDirection(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up: return Vector3.up;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Down: return Vector3.down;
            case MoveDirection.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    /// <summary>
    /// Returns the opposite direction from a MoveDirection enum value
    /// </summary>
    /// <param name="direction">MoveDirection enum value</param>
    /// <returns>The opposite MoveDirection enum value</returns>
    public static MoveDirection GetOppositeDirection(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up: return MoveDirection.Down;
            case MoveDirection.Left: return MoveDirection.Right;
            case MoveDirection.Down: return MoveDirection.Up;
            case MoveDirection.Right: return MoveDirection.Left;
            default: return MoveDirection.Stale;
        }
    }

    /// <summary>
    /// Returns a MoveDirection enum value correspondent to a Vector3 direction
    /// </summary>
    /// <param name="direction">A Vector3 direction</param>
    /// <returns>A MoveDirection enum valu correspondent</returns>
    public static MoveDirection GetDirectionFromVector(Vector3 direction)
    {
        if (direction == Vector3.up)
            return MoveDirection.Up;
        if (direction == Vector3.left)
            return MoveDirection.Left;
        if (direction == Vector3.down)
            return MoveDirection.Down;
        if (direction == Vector3.right)
            return MoveDirection.Right;
        return MoveDirection.Stale;
    }

    /// <summary>
    /// Returns the distance in units between two Vector3
    /// </summary>
    /// <param name="first">First value</param>
    /// <param name="second">Second value</param>
    /// <returns>The distance between the vectors in units</returns>
    public static float GetDistanceBetweenVectors(Vector3 first, Vector3 second)
    {
        var dx = first.x - second.x;
        var dy = first.y - second.y;

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Returns an IntersectionValue on the maze based on the desired position
    /// </summary>
    /// <param name="location">The desired location</param>
    /// <returns>A IntersectionValue for that location</returns>
    public static IntersectionTile GetIntersectionTile(Vector3 location)
    {
        var mazeTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
            m.TileX == (int) location.x && m.TileY == (int) location.y && m.GetComponent<IntersectionTile>() != null);

        return mazeTile != null ? mazeTile.GetComponent<IntersectionTile>() : null;
    }
}