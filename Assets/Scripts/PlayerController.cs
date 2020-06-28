using System;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Space(10), Header("Variables")]
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

        var nextTile = GetIntersection(transform.localPosition);
        if (nextTile != null)
            _currentIntersectionTile = nextTile;

        _currentMoveDirection = MoveDirection.Left;
        CurrentMoveOrientation = MoveDirection.Left;

        ChangePosition(_currentMoveDirection);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ChangePosition(MoveDirection.Up);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            ChangePosition(MoveDirection.Left);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            ChangePosition(MoveDirection.Down);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            ChangePosition(MoveDirection.Right);

        Move();
        UpdateSprite();
        ConsumePellet();
    }

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

    private void Move()
    {
        if (_targetIntersectionTile != _currentIntersectionTile && _targetIntersectionTile != null)
        {
            if (_nextMoveDirection == GetOppositeDirection(_currentMoveDirection))
            {
                _currentMoveDirection = GetOppositeDirection(_currentMoveDirection);
                var tempTile = _targetIntersectionTile;
                _targetIntersectionTile = _previousIntersectionTile;
                _previousIntersectionTile = tempTile;
            }

            if (OvershotTarget())
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
                transform.localPosition += GetVectorDirection() * Speed * Time.deltaTime;
                _playerAnimator.speed = 1f;
            }
        }
    }

    //private void MoveToNextTile()
    //{
    //    var nextTile = MoveToNeighbor(_currentMoveDirection);
    //    if (nextTile != null)
    //    {
    //        transform.localPosition = nextTile.transform.localPosition;
    //        UpdateSprite();
    //        _currentIntersectionTile = nextTile;
    //    }
    //}

    private void UpdateSprite()
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

    private Vector3 GetVectorDirection()
    {
        switch (_currentMoveDirection)
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

    private IntersectionTile GetIntersection(Vector3 location)
    {
        var mazeTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
            m.TileX == (int) location.x && m.TileY == (int) location.y && m.GetComponent<IntersectionTile>() != null);

        return mazeTile != null ? mazeTile.GetComponent<IntersectionTile>() : null;
    }

    private MazeTile GetPelletAtLocation(Vector3 location)
    {
        var mazeTile = MazeAssembler.Instance.AllPelletTiles.FirstOrDefault
            (m => m.TileX == (int)location.x && m.TileY == (int)location.y);

        return mazeTile;
    }

    private void ConsumePellet()
    {
        var actualTile = GetPelletAtLocation(transform.localPosition);
        if (_currentMazeTile != actualTile && actualTile != null)
        {
            _currentMazeTile = actualTile;
            _currentMazeTile.OnPlayerInteract.Invoke();
        }
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