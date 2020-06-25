using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeTile : MonoBehaviour
{
    [Space(10), Header("Maze Tile Information")]
    [SerializeField] private MazeTileType _mazeTileType;
    public int TileX;
    public int TileY;

    [Space(10), Header("Maze Tile Neighbors")]
    public MazeTile UpNeighbor;
    public MazeTile LeftNeighbor;
    public MazeTile DownNeighbor;
    public MazeTile RightNeighbor;

    private void Awake()
    {
        _mazeTileType = (MazeTileType)Enum.Parse(typeof(MazeTileType), gameObject.tag);
    }

    public void SetTilePosition(float posX, float posY)
    {
        TileX = (int)posX;
        TileY = (int)posY;
    }

    public void SetNeighbors(MazeTile up, MazeTile left, MazeTile down, MazeTile right)
    {
        UpNeighbor = up;
        LeftNeighbor = left;
        DownNeighbor = down;
        RightNeighbor = right;
    }
}

[Serializable]
public enum MazeTileType
{
    Wall,
    Pellet,
    Energizer,
    Intersection,
    OutOfBound,
    Empty,
    GhostHouse,
    Tunnel,
    Fruit
}
