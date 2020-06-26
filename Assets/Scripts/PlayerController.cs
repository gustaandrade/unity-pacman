using System;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Speed = 5f;
    
    private MoveDirection _moveDirection;
    private Animator _playerAnimator;

    private IntersectionTile _currentTile;

    private void Awake()
    {
        _moveDirection = MoveDirection.Stale;

        _playerAnimator = GetComponentInChildren<Animator>();

        _currentTile = GetIntersection(transform.localPosition);
        Debug.Log(_currentTile);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _moveDirection = MoveDirection.Up;
            MoveToNextTile();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _moveDirection = MoveDirection.Left;
            MoveToNextTile();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _moveDirection = MoveDirection.Down;
            MoveToNextTile();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _moveDirection = MoveDirection.Right;
            MoveToNextTile();
        }
    }

    private void Move()
    {
        _playerAnimator.speed = 1f;
        transform.localPosition += GetVectorDirection() * Speed * Time.deltaTime;
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