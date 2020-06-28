using System;
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

    /// <summary>
    /// Consumes the pellet, energizer of fruit and does specific things for each one
    /// </summary>
    private void ConsumePellet()
    {
        if (_mazeTileType == MazeTileType.Pellet 
            || _mazeTileType == MazeTileType.Energizer 
            || _mazeTileType == MazeTileType.Fruit)
        {
            gameObject.SetActive(false);

            switch (_mazeTileType)
            {
                case MazeTileType.Pellet:
                    GameController.Instance.SetPelletsConsumed();
                    ScoreController.Instance.SetLevelScore(10);
                    SoundController.Instance.PlayPelletEatenSFX();
                    break;
                
                case MazeTileType.Energizer:
                    GameController.Instance.SetEnergizersConsumed();
                    ScoreController.Instance.SetLevelScore(50);
                    SoundController.Instance.PlayPelletEatenSFX();
                    SoundController.Instance.PlayEnergizerMusic();
                    break;

                case MazeTileType.Fruit:
                    ScoreController.Instance.SetFruitScore();
                    SoundController.Instance.PlayFruitEatenSFX();
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
