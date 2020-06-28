using System;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMazeEntity
{
    [Space(10), Header("Player Variables")]
    public float Speed;
    public MoveDirection CurrentMoveOrientation;
    
    private Animator _playerAnimator;
    
    private MoveDirection _currentMoveDirection;
    private MoveDirection _nextMoveDirection;

    private IntersectionTile _currentIntersectionTile;
    private IntersectionTile _previousIntersectionTile;
    private IntersectionTile _targetIntersectionTile;

    private MazeTile _currentMazeTile;

    private void Awake()
    {
        _playerAnimator = GetComponentInChildren<Animator>();

        var nextTile = Helpers.GetIntersectionTile(transform.localPosition);
        if (nextTile != null)
            _currentIntersectionTile = nextTile;

        CurrentMoveOrientation = MoveDirection.Left;
        ChangePosition(MoveDirection.Left);
    }

    private void Update()
    {
        // waits for a input for the player to move accordingly
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ChangePosition(MoveDirection.Up);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            ChangePosition(MoveDirection.Left);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            ChangePosition(MoveDirection.Down);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            ChangePosition(MoveDirection.Right);

        Move();
        UpdateAnimation();
        ConsumePellet();
    }

    /// <summary>
    /// Changes player position based on the next MoveDirection
    /// </summary>
    /// <param name="direction">The next MoveDirection to travel</param>
    private void ChangePosition(MoveDirection direction)
    {
        if (direction != _currentMoveDirection)
            _nextMoveDirection = direction;

        if (_currentIntersectionTile != null)
        {
            var nextTile = MoveToNeighbor(direction);
            if (nextTile != null)
            {
                _currentMoveDirection = direction;
                _targetIntersectionTile = nextTile;
                _previousIntersectionTile = _currentIntersectionTile;
                _currentIntersectionTile = null;
            }
        }
    }

    /// <summary>
    /// Moves the player accordingly to the target tiles
    /// </summary>
    public void Move()
    {
        if (_targetIntersectionTile != _currentIntersectionTile && _targetIntersectionTile != null)
        {
            if (_nextMoveDirection == Helpers.GetOppositeDirection(_currentMoveDirection))
            {
                _currentMoveDirection = Helpers.GetOppositeDirection(_currentMoveDirection);
                var tempTile = _targetIntersectionTile;
                _targetIntersectionTile = _previousIntersectionTile;
                _previousIntersectionTile = tempTile;
            }

            if (TargetWasOvershot())
            {
                _currentIntersectionTile = _targetIntersectionTile;
                transform.localPosition = _currentIntersectionTile.transform.localPosition;

                if (_currentIntersectionTile.IsPortal)
                {
                    transform.localPosition = _currentIntersectionTile.OppositePortal.transform.localPosition;
                    _currentIntersectionTile = _currentIntersectionTile.OppositePortal;
                }

                var nextTile = MoveToNeighbor(_nextMoveDirection);

                if (nextTile != null)
                    _currentMoveDirection = _nextMoveDirection;
                if (nextTile == null)
                    nextTile = MoveToNeighbor(_currentMoveDirection);
                if (nextTile != null)
                {
                    _targetIntersectionTile = nextTile;
                    _previousIntersectionTile = _currentIntersectionTile;
                    _currentIntersectionTile = null;
                }
                else
                {
                    _currentMoveDirection = MoveDirection.Stale;
                    _playerAnimator.speed = 0f;
                }
            }
            else
            {
                transform.localPosition += Helpers.GetVectorDirection(_currentMoveDirection) * Speed * Time.deltaTime;
                _playerAnimator.speed = 1f;
            }
        }
    }

    /// <summary>
    /// Updates player animation based on the trajectory
    /// </summary>
    public void UpdateAnimation()
    {
        CurrentMoveOrientation = _currentMoveDirection;

        switch (_currentMoveDirection)
        {
            case MoveDirection.Up:
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;

            case MoveDirection.Left:
                transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
                transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            case MoveDirection.Down:
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                break;

            case MoveDirection.Right:
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                break;
        }
    }

    /// <summary>
    /// Searches for a pellet object in the player's positions and consumes it
    /// </summary>
    private void ConsumePellet()
    {
        var actualTile = GetPelletAtLocation(transform.localPosition);
        if (_currentMazeTile != actualTile && actualTile != null)
        {
            _currentMazeTile = actualTile;
            _currentMazeTile.OnPlayerInteract.Invoke();
        }
    }

    /// <summary>
    /// Gets which pellet the player are in the same tile of
    /// </summary>
    /// <param name="location">Current location to check</param>
    /// <returns>The MazeTile correspondent</returns>
    public static MazeTile GetPelletAtLocation(Vector3 location)
    {
        var mazeTile = MazeAssembler.Instance.AllPelletTiles.FirstOrDefault
            (m => m.TileX == (int)location.x && m.TileY == (int)location.y);

        return mazeTile;
    }

    private IntersectionTile MoveToNeighbor(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up: return _currentIntersectionTile.UpNeighbor;
            case MoveDirection.Left: return _currentIntersectionTile.LeftNeighbor;
            case MoveDirection.Down: return _currentIntersectionTile.DownNeighbor;
            case MoveDirection.Right: return _currentIntersectionTile.RightNeighbor;
            default: return null;
        }
    }

    public bool TargetWasOvershot()
    {
        var tileToTarget = DistanceFromTile(_targetIntersectionTile.transform.localPosition);
        var tileToSelf = DistanceFromTile(transform.localPosition);

        return tileToSelf > tileToTarget;
    }

    public float DistanceFromTile(Vector3 target)
    {
        var distance = target - _previousIntersectionTile.transform.localPosition;
        return distance.sqrMagnitude;
    }
}

[Serializable]
public enum MoveDirection
{
    Up,
    Left,
    Down,
    Right,
    Stale
}