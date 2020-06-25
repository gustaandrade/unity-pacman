using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Maze Configuration", menuName = "ScriptableObjects/MazeConfiguration", order = 1)]
public class MazeConfiguration : ScriptableObject
{
    public Vector2[] Walls;
    public Vector2[] Pellets;
    public Vector2[] Energizers;
    public Vector2[] Intersections;
    public Vector2[] OutOfBounds;
    public Vector2[] Empties;
    public Vector2[] GhostHouse;
    public Vector2[] Tunnels;
    public Vector2[] Fruit;
}
