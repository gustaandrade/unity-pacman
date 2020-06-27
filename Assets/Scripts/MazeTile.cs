using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent OnPlayerInteract = new UnityEvent();

    private void Awake()
    {
        _mazeTileType = (MazeTileType)Enum.Parse(typeof(MazeTileType), gameObject.tag);

        OnPlayerInteract.AddListener(ConsumePellet);
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

    private void ConsumePellet()
    {
        if (_mazeTileType == MazeTileType.Pellet || _mazeTileType == MazeTileType.Energizer)
        {
            SoundController.Instance.PlayPelletEatenSFX();
            gameObject.SetActive(false);

            switch (_mazeTileType)
            {
                case MazeTileType.Pellet:
                    ScoreController.Instance.SetPelletsConsumed();
                    ScoreController.Instance.SetLevelScore(10);
                    break;
                
                case MazeTileType.Energizer:
                    ScoreController.Instance.SetEnergizersConsumed();
                    ScoreController.Instance.SetLevelScore(50);
                    SoundController.Instance.PlayEnergizerMusic();
                    break;

                default: 
                    throw new ArgumentOutOfRangeException();
            }

            OnPlayerInteract.RemoveListener(ConsumePellet);
        }
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
