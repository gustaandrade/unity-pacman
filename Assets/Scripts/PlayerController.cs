using System;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Speed;
    
    private Animator _playerAnimator;
    
    private MoveDirection _moveDirection;
    private MoveDirection _nextDirection;

    private IntersectionTile _currentTile;
    private IntersectionTile _previousTile;
    private IntersectionTile _targetTile;

    private void Awake()
    {
        _playerAnimator = GetComponentInChildren<Animator>();

        var nextTile = GetIntersection(transform.localPosition);
        if (nextTile != null)
            _currentTile = nextTile;

        _moveDirection = MoveDirection.Left;
        ChangePosition(_moveDirection);
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
    }

    private void ChangePosition(MoveDirection direction)
    {
        if (direction != _moveDirection)
            _nextDirection = direction;

        if (_currentTile != null)
        {
            var nextTile = MoveToNeighbor(direction);
            if (nextTile != null)
            {
                _moveDirection = direction;
                _targetTile = nextTile;
                _previousTile = _currentTile;
                _currentTile = null;
            }
        }
    }

    private void Move()
    {
        if (_targetTile != _currentTile && _targetTile != null)
        {
            if (OvershotTarget())
            {
                _currentTile = _targetTile;

                transform.localPosition = _currentTile.transform.localPosition;

                var nextTile = MoveToNeighbor(_nextDirection);
                if (nextTile != null)
                    _moveDirection = _nextDirection;
                if (nextTile == null)
                    nextTile = MoveToNeighbor(_moveDirection);
                if (nextTile != null)
                {
                    _targetTile = nextTile;
                    _previousTile = _currentTile;
                    _currentTile = null;
                }
                else
                {
                    _moveDirection = MoveDirection.Stale;
                    _playerAnimator.speed = 0f;
                }
            }
            else
            {
                transform.localPosition += GetVectorDirection() * Speed * Time.deltaTime;
                _playerAnimator.speed = 1f;
            }
        }

        UpdateSprite();
    }

    private void MoveToNextTile()
    {
        var nextTile = MoveToNeighbor(_moveDirection);
        if (nextTile != null)
        {
            transform.localPosition = nextTile.transform.localPosition;
            UpdateSprite();
            _currentTile = nextTile;
        }
    }

    private void UpdateSprite()
    {
        switch (_moveDirection)
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

            default:
                break;
        }
    }

    private Vector3 GetVectorDirection()
    {
        switch (_moveDirection)
        {
            case MoveDirection.Up: return Vector3.up;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Down: return Vector3.down;
            case MoveDirection.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    private IntersectionTile GetIntersection(Vector3 location)
    {
        var mazeTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
            m.TileX == (int) location.x && m.TileY == (int) location.y && m.GetComponent<IntersectionTile>() != null);

        return mazeTile != null ? mazeTile.GetComponent<IntersectionTile>() : null;
    }

    private IntersectionTile MoveToNeighbor(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up: return _currentTile.UpNeighbor;
            case MoveDirection.Left: return _currentTile.LeftNeighbor;
            case MoveDirection.Down: return _currentTile.DownNeighbor;
            case MoveDirection.Right: return _currentTile.RightNeighbor;
            default: return null;
        }
    }

    private bool OvershotTarget()
    {
        var tileToTarget = DistanceFromTile(_targetTile.transform.localPosition);
        var tileToSelf = DistanceFromTile(transform.localPosition);

        return tileToSelf > tileToTarget;
    }

    private float DistanceFromTile(Vector3 target)
    {
        var distance = target - _previousTile.transform.localPosition;
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