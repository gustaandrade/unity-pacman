using System.Linq;
using UnityEngine;

public static class Helpers
{
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

    public static float GetDistanceBetweenVectors(Vector3 first, Vector3 second)
    {
        var dx = first.x - second.x;
        var dy = first.y - second.y;

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public static IntersectionTile GetIntersectionTile(Vector3 location)
    {
        var mazeTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
            m.TileX == (int) location.x && m.TileY == (int) location.y && m.GetComponent<IntersectionTile>() != null);

        return mazeTile != null ? mazeTile.GetComponent<IntersectionTile>() : null;
    }
}