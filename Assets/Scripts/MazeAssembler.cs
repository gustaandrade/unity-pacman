using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeAssembler : MonoBehaviour
{
    public static MazeAssembler Instance;

    private MazeConfiguration _mazeConfiguration;

    [Space(10), Header("Parents")]
    public GameObject WallsParent;
    public GameObject PelletsParent;
    public GameObject EnergizersParent;
    public GameObject IntersectionsParent;
    public GameObject OutOfBoundsParent;
    public GameObject EmptiesParent;
    public GameObject GhostHouseParent;
    public GameObject TunnelsParent;
    public GameObject FruitParent;

    [Space(10), Header("Prefabs")]
    public GameObject WallPrefab;
    public GameObject PelletPrefab;
    public GameObject EnergizerPrefab;
    public GameObject IntersectionPrefab;
    public GameObject OutOfBoundPrefab;
    public GameObject EmptyPrefab;
    public GameObject GhostHousePrefab;
    public GameObject TunnelPrefab;
    public GameObject FruitPrefab;

    public List<MazeTile> MazeTiles;
    public List<MazeTile> AllPelletTiles;

    private List<MazeTile> _pelletTiles;
    private List<MazeTile> _energizerTiles;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _mazeConfiguration = Resources.Load<MazeConfiguration>("MazeConfiguration");

        foreach (var wal in _mazeConfiguration.Walls) BuildMaze(wal, MazeTileType.Wall);
        foreach (var pel in _mazeConfiguration.Pellets) BuildMaze(pel, MazeTileType.Pellet);
        foreach (var en in _mazeConfiguration.Energizers) BuildMaze(en, MazeTileType.Energizer);
        //foreach (var inter in _mazeConfiguration.Intersections) BuildMaze(inter, MazeTileType.Intersection);
        foreach (var oob in _mazeConfiguration.OutOfBounds) BuildMaze(oob, MazeTileType.OutOfBound);
        foreach (var emp in _mazeConfiguration.Empties) BuildMaze(emp, MazeTileType.Empty);
        foreach (var gh in _mazeConfiguration.GhostHouse) BuildMaze(gh, MazeTileType.GhostHouse);
        foreach (var tun in _mazeConfiguration.Tunnels) BuildMaze(tun, MazeTileType.Tunnel);
        foreach (var fr in _mazeConfiguration.Fruit) BuildMaze(fr, MazeTileType.Fruit);

        _pelletTiles = PelletsParent.GetComponentsInChildren<MazeTile>().ToList();
        _energizerTiles = EnergizersParent.GetComponentsInChildren<MazeTile>().ToList();

        MazeTiles = transform.GetComponentsInChildren<MazeTile>().ToList();
        AllPelletTiles = _pelletTiles.Concat(_energizerTiles).ToList();

        foreach (var tile in MazeTiles)
        {
            var upNeighbor = MazeTiles.FirstOrDefault(mt => mt.TileX == tile.TileX && mt.TileY == tile.TileY + 1);
            var leftNeighbor = MazeTiles.FirstOrDefault(mt => mt.TileX == tile.TileX - 1 && mt.TileY == tile.TileY);
            var downNeighbor = MazeTiles.FirstOrDefault(mt => mt.TileX == tile.TileX && mt.TileY == tile.TileY - 1);
            var rightNeighbor = MazeTiles.FirstOrDefault(mt => mt.TileX == tile.TileX + 1 && mt.TileY == tile.TileY);

            tile.SetNeighbors(upNeighbor, leftNeighbor, downNeighbor, rightNeighbor);
        }
    }

    private void BuildMaze(Vector2 tilePosition, MazeTileType tileType)
    {
        GameObject tilePrefab;
        Transform tileParent;

        switch(tileType)
        {
            case MazeTileType.Wall:
                tilePrefab = WallPrefab;
                tileParent = WallsParent.transform;
                break;

            case MazeTileType.Pellet:
                tilePrefab = PelletPrefab;
                tileParent = PelletsParent.transform;
                break;

            case MazeTileType.Energizer:
                tilePrefab = EnergizerPrefab;
                tileParent = EnergizersParent.transform;
                break;

            case MazeTileType.Intersection:
                tilePrefab = IntersectionPrefab;
                tileParent = IntersectionsParent.transform;
                break;

            case MazeTileType.OutOfBound:
                tilePrefab = OutOfBoundPrefab;
                tileParent = OutOfBoundsParent.transform;
                break;

            case MazeTileType.Empty:
                tilePrefab = EmptyPrefab;
                tileParent = EmptiesParent.transform;
                break;

            case MazeTileType.GhostHouse:
                tilePrefab = GhostHousePrefab;
                tileParent = GhostHouseParent.transform;
                break;

            case MazeTileType.Tunnel:
                tilePrefab = TunnelPrefab;
                tileParent = TunnelsParent.transform;
                break;

            case MazeTileType.Fruit:
                tilePrefab = FruitPrefab;
                tileParent = FruitParent.transform;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
        }

        var newItem = Instantiate(tilePrefab, tileParent, false);
        newItem.transform.localPosition = new Vector3(tilePosition.x, tilePosition.y, 0);

        var newTile = newItem.GetComponent<MazeTile>();
        newTile.SetTilePosition(tilePosition.x, tilePosition.y);
        newTile.SetTilePosition(tilePosition.x, tilePosition.y);
    }
}
