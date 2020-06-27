using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GhostController : MonoBehaviour
{
    [Space(10), Header("Variables")]
    public float Speed;

    private GameObject _player;

    private MoveDirection _currentMoveDirection;
    private MoveDirection _nextMoveDirection;

    private IntersectionTile _currentIntersectionTile;
    private IntersectionTile _previousIntersectionTile;
    private IntersectionTile _targetIntersectionTile;

    private int _modeChangeIteration = 0;
    private float _modeChangeTimer = 0f;

    private GhostMode _currentGhostMode;
    private GhostMode _previousGhostMode;

    private Level _currentLevel;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        _currentGhostMode = GhostMode.Scatter;
        _currentLevel = LevelController.Instance.GetCurrentLevel();

        var nextTile = GetIntersection(transform.localPosition);
        if (nextTile != null)
            _currentIntersectionTile = nextTile;

        _previousIntersectionTile = _currentIntersectionTile;

        _currentMoveDirection = MoveDirection.Right;

        _targetIntersectionTile = GetIntersection(_player.transform.localPosition);
    }

    private void Update()
    {
        HandleGhostMode();
        Move();
    }

    private void Move()
    {
        if (_targetIntersectionTile != _currentIntersectionTile && _targetIntersectionTile != null)
        {
            if (OvershotTarget())
            {
                _currentIntersectionTile = _targetIntersectionTile;
                transform.localPosition = _currentIntersectionTile.transform.localPosition;

                if (_currentIntersectionTile.IsPortal)
                {
                    transform.localPosition = _currentIntersectionTile.OppositePortal.transform.localPosition;
                    _currentIntersectionTile = _currentIntersectionTile.OppositePortal;
                }

                _targetIntersectionTile = ChooseNextTile();
                _previousIntersectionTile = _currentIntersectionTile;
                _currentIntersectionTile = null;
            }
            else
            {
                transform.localPosition += GetVectorDirection(_currentMoveDirection) * Speed * Time.deltaTime;
            }
        }
    }

    private void HandleGhostMode()
    {
        if (_currentGhostMode != GhostMode.Frightened)
        {
            _modeChangeTimer += Time.deltaTime;

            switch (_modeChangeIteration)
            {
                case 0:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > _currentLevel.GhostTimers[0])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (_currentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > _currentLevel.GhostTimers[1])
                    {
                        _modeChangeIteration = 1;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 1:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > _currentLevel.GhostTimers[2])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (_currentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > _currentLevel.GhostTimers[3])
                    {
                        _modeChangeIteration = 2;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 2:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > _currentLevel.GhostTimers[4])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (_currentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > _currentLevel.GhostTimers[5])
                    {
                        _modeChangeIteration = 3;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 3:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > _currentLevel.GhostTimers[6])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    break;
            }
        }
        else if (_currentGhostMode == GhostMode.Frightened)
        {

        }
    }

    private void ChangeGhostMode(GhostMode mode)
    {
        _previousGhostMode = _currentGhostMode;
        _currentGhostMode = mode;
    }

    private IntersectionTile ChooseNextTile()
    {
        IntersectionTile targetNode = null;
        var leastDistance = 999999f;

        var targetVector = _player.transform.localPosition;

        for (var i = 0; i < _currentIntersectionTile.Neighbors.Length; i++)
        {
            if (_currentIntersectionTile.NeighborDirections[i] != Vector3.zero &&
                GetOppositeDirection(GetDirectionFromVector(_currentIntersectionTile.NeighborDirections[i])) != _currentMoveDirection)
            {
                var distance = GetDistanceBetweenVectors
                    (_currentIntersectionTile.Neighbors[i].transform.localPosition, targetVector);
                if (distance < leastDistance)
                {
                    leastDistance = distance;
                    targetNode = _currentIntersectionTile.Neighbors[i];
                    _currentMoveDirection = GetDirectionFromVector(_currentIntersectionTile.NeighborDirections[i]);
                }
            }
        }

        return targetNode;
    }

    private Vector3 GetVectorDirection(MoveDirection direction)
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

    private MoveDirection GetOppositeDirection(MoveDirection direction)
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

    private MoveDirection GetDirectionFromVector(Vector3 direction)
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

    private IntersectionTile GetIntersection(Vector3 location)
    {
        var mazeTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
            m.TileX == (int)location.x && m.TileY == (int)location.y && m.GetComponent<IntersectionTile>() != null);

        return mazeTile != null ? mazeTile.GetComponent<IntersectionTile>() : null;
    }

    private bool OvershotTarget()
    {
        var tileToTarget = DistanceFromTile(_targetIntersectionTile.transform.localPosition);
        var tileToSelf = DistanceFromTile(transform.localPosition);

        return tileToSelf > tileToTarget;
    }

    private float DistanceFromTile(Vector3 target)
    {
        var distance = target - _previousIntersectionTile.transform.localPosition;
        return distance.sqrMagnitude;
    }

    private float GetDistanceBetweenVectors(Vector3 first, Vector3 second)
    {
        var dx = first.x - second.x;
        var dy = first.y - second.y;

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private List<IntersectionTile> CheckValidTiles()
    {
        return _currentIntersectionTile.Neighbors.Where
            ((t, i) => 
            _currentIntersectionTile.NeighborDirections[i] != Vector3.zero).ToList();
    }
}

[Serializable]
public enum GhostMode
{
    Scatter,
    Chase,
    Frightened
}