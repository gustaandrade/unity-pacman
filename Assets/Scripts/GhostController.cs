using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

public class GhostController : MonoBehaviour
{
    [Space(10), Header("Variables")] 
    public Ghost Ghost;
    public float Speed;
    public float FrightenedSpeed;
    public float EatenSpeed;
    public float ReleaseTime;
    public bool IsInGhostHouse;

    private MazeTile _scatterTile;

    private GameObject _player;
    private PlayerController _playerController;

    private MoveDirection _currentMoveDirection;
    private MoveDirection _nextMoveDirection;

    private IntersectionTile _currentIntersectionTile;
    private IntersectionTile _previousIntersectionTile;
    private IntersectionTile _targetIntersectionTile;

    private int _modeChangeIteration = 0;
    private float _modeChangeTimer = 0f;

    private float _currentReleaseTimer;

    private GhostMode _currentGhostMode;
    private GhostMode _previousGhostMode;

    private Level _currentLevel;

    private List<GhostController> _ghostControllers;

    private Animator _spriteAnimator;

    private float _previousMoveSpeed;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerController = _player.GetComponent<PlayerController>();

        _currentGhostMode = GhostMode.Scatter;
        _currentLevel = LevelController.Instance.GetCurrentLevel();

        GetScatterTile();

        var nextTile = GetIntersection(transform.localPosition);
        if (nextTile != null)
            _currentIntersectionTile = nextTile;

        if (IsInGhostHouse)
        {
            _currentMoveDirection = MoveDirection.Up;
            _targetIntersectionTile = _currentIntersectionTile.UpNeighbor;
        }
        else
        {
            _currentMoveDirection = MoveDirection.Left;
            _targetIntersectionTile = ChooseNextTile();
        }

        _previousIntersectionTile = _currentIntersectionTile;

        _ghostControllers = MazeAssembler.Instance.gameObject.GetComponentsInChildren<GhostController>().ToList();

        _spriteAnimator = GetComponentInChildren<Animator>();

        _previousMoveSpeed = Speed;
    }

    private void Update()
    {
        HandleGhostMode();
        WatchModeChange();
        CheckForReleaseGhost();
        Move();
        CheckCollision();
        UpdateAnimation();
    }

    private void Move()
    {
        if (_targetIntersectionTile != _currentIntersectionTile && _targetIntersectionTile != null && !IsInGhostHouse)
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
        if (_currentGhostMode != GhostMode.Frightened && _currentGhostMode != GhostMode.Eaten)
        {
            _modeChangeTimer += Time.deltaTime;

            Speed = _previousMoveSpeed;

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
            Speed = FrightenedSpeed;
        }
        else if (_currentGhostMode == GhostMode.Eaten)
        {
            Speed = EatenSpeed;
        }
    }

    private void WatchModeChange()
    {
        if (_currentGhostMode == GhostMode.Eaten) return;

        ChangeGhostMode(ScoreController.Instance.IsEnergized ? GhostMode.Frightened : _previousGhostMode);
    }

    private void ChangeGhostMode(GhostMode mode)
    {
        if (_currentGhostMode == mode) return;
     
        if (_currentGhostMode != GhostMode.Frightened && _currentGhostMode != GhostMode.Eaten)
            _previousGhostMode = _currentGhostMode;
        
        _currentGhostMode = mode;
    }

    private Vector3 GetGhostTargetTile()
    {
        Debug.Log(_currentGhostMode);

        switch (_currentGhostMode)
        {
            case GhostMode.Scatter:
                return _scatterTile.transform.localPosition;
            
            case GhostMode.Chase:
                switch (Ghost)
                {
                    case Ghost.Blinky:
                        return _player.transform.localPosition;

                    case Ghost.Pinky:
                        return _player.transform.localPosition +
                               4 * GetVectorDirection(_playerController.CurrentMoveOrientation);

                    case Ghost.Inky:
                        var blinky = _ghostControllers.FirstOrDefault(g => g.Ghost == Ghost.Blinky);
                        var blinkyPosition = blinky != null ? blinky.transform.localPosition : _player.transform.localPosition;
                        var targetTile = _player.transform.localPosition +
                                         2 * GetVectorDirection(_playerController.CurrentMoveOrientation);
                        var inkyDistance = GetDistanceBetweenVectors(blinkyPosition, targetTile);

                        return new Vector3(blinkyPosition.x + inkyDistance, blinkyPosition.y + inkyDistance, blinkyPosition.z);

                    case Ghost.Clyde:
                        var clydeDistance = GetDistanceBetweenVectors(transform.localPosition, _player.transform.localPosition);
                    
                        return clydeDistance <= 8 ? _scatterTile.transform.localPosition : _player.transform.localPosition;

                    default:
                        return Vector3.zero;
                }

            case GhostMode.Frightened:
                return new Vector3(UnityEngine.Random.Range(0, 30), UnityEngine.Random.Range(0, 26), 1f);

            case GhostMode.Eaten:
                Debug.Log(GetGhostHouse());

                return GetGhostHouse();

            default:
                return Vector3.zero;
        }
    }

    private void CheckForReleaseGhost()
    {
        if (!IsInGhostHouse) return;

        _currentReleaseTimer += Time.deltaTime;

        if (!(_currentReleaseTimer > ReleaseTime)) return;
        
        IsInGhostHouse = false;
    }

    private IntersectionTile ChooseNextTile()
    {
        IntersectionTile targetNode = null;
        var leastDistance = 999999f;

        var targetTile = GetGhostTargetTile();

        for (var i = 0; i < _currentIntersectionTile.Neighbors.Length; i++)
        {
            if (_currentIntersectionTile.NeighborDirections[i] != Vector3.zero &&
                GetDirectionFromVector(_currentIntersectionTile.NeighborDirections[i]) != GetOppositeDirection(_currentMoveDirection))
            {
                var distance = GetDistanceBetweenVectors
                    (_currentIntersectionTile.Neighbors[i].transform.localPosition, targetTile);
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

    private void CheckCollision()
    {
        if (GetDistanceBetweenVectors(transform.localPosition, _player.transform.localPosition) > 0.5f)
            return;

        if (_currentGhostMode == GhostMode.Frightened)
            ChangeGhostMode(GhostMode.Eaten);
        else
            Debug.Log("f in the chat");
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

    private Vector3 GetGhostHouse()
    {
        var mazeTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
            m.GetComponent<IntersectionTile>() != null && m.GetComponent<IntersectionTile>().IsGhostHouse);

        return mazeTile != null ? mazeTile.transform.localPosition : Vector3.zero;
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

    private void GetScatterTile()
    {
        switch(Ghost)
        {
            case Ghost.Blinky:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 25 && m.TileY == 31);
                break;
            
            case Ghost.Pinky:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 2 && m.TileY == 31);
                break;
            
            case Ghost.Inky:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 25 && m.TileY == -1);
                break;
            
            case Ghost.Clyde:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 2 && m.TileY == -1);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateAnimation()
    {
        _spriteAnimator.SetBool("Frightened", _currentGhostMode == GhostMode.Frightened);
        _spriteAnimator.SetBool("FrightenedEnding", _currentGhostMode == GhostMode.Frightened && ScoreController.Instance.IsEnergizedTimeEnding);
        _spriteAnimator.SetBool("Eaten", _currentGhostMode == GhostMode.Eaten);

        switch (_currentMoveDirection)
        {
            case MoveDirection.Up:
                _spriteAnimator.SetInteger("GhostDirection", 0);
                break;

            case MoveDirection.Left:
                _spriteAnimator.SetInteger("GhostDirection", 1);
                break;
            
            case MoveDirection.Down:
                _spriteAnimator.SetInteger("GhostDirection", 2);
                break;
            
            case MoveDirection.Right:
                _spriteAnimator.SetInteger("GhostDirection", 3);
                break;
            
            case MoveDirection.Stale:
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

[Serializable]
public enum GhostMode
{
    Scatter,
    Chase,
    Frightened,
    Eaten
}

public enum Ghost
{
    Blinky,
    Pinky,
    Inky,
    Clyde
}