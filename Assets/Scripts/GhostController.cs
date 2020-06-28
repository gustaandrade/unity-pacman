using System;
using System.Linq;
using UnityEngine;

public class GhostController : MonoBehaviour, IMazeEntity
{
    [Space(10), Header("Ghost Variables")] 
    public GhostType GhostType;
    public float NormalSpeed;
    public float FrightenedSpeed;
    public float EatenSpeed;
    public float TimeUntilRelease;
    public bool IsInGhostHouse;

    private GameObject _player;
    private PlayerController _playerController;

    private MoveDirection _currentMoveDirection;

    private IntersectionTile _currentIntersectionTile;
    private IntersectionTile _previousIntersectionTile;
    private IntersectionTile _targetIntersectionTile;

    private MazeTile _scatterTile;
    
    private int _modeChangeIteration;
    private float _modeChangeTimer;
    private float _previousMoveSpeed;
    private float _currentReleaseTimer;

    private GhostMode _currentGhostMode;
    private GhostMode _previousGhostMode;

    private Animator _spriteAnimator;

    private void Awake()
    {
        _spriteAnimator = GetComponentInChildren<Animator>();

        _player = GameObject.FindGameObjectWithTag("Player");
        _playerController = _player.GetComponent<PlayerController>();

        _currentGhostMode = GhostMode.Scatter;
        GetScatterTile();

        var nextTile = Helpers.GetIntersectionTile(transform.localPosition);
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
        _previousMoveSpeed = NormalSpeed;
    }

    private void Update()
    {
        HandleGhostMode();
        CheckForReleaseGhost();
        Move();
        CheckCollision();
        UpdateAnimation();
    }

    public void Move()
    {
        if (_targetIntersectionTile != _currentIntersectionTile && _targetIntersectionTile != null && !IsInGhostHouse)
        {
            if (TargetWasOvershot())
            {
                _currentIntersectionTile = _targetIntersectionTile;
                transform.localPosition = _currentIntersectionTile.transform.localPosition;

                if (_currentIntersectionTile.IsPortal)
                {
                    transform.localPosition = _currentIntersectionTile.OppositePortal.transform.localPosition;
                    _currentIntersectionTile = _currentIntersectionTile.OppositePortal;
                }

                if (_currentIntersectionTile.IsGhostHouseEntrance && _currentGhostMode == GhostMode.Eaten)
                    ChangeGhostMode(_previousGhostMode);

                _targetIntersectionTile = ChooseNextTile();
                _previousIntersectionTile = _currentIntersectionTile;
                _currentIntersectionTile = null;
            }
            else
                transform.localPosition += Helpers.GetVectorDirection(_currentMoveDirection) * NormalSpeed * Time.deltaTime;
        }
    }

    private void HandleGhostMode()
    {
        if (_currentGhostMode != GhostMode.Frightened && _currentGhostMode != GhostMode.Eaten && !IsInGhostHouse)
        {
            _modeChangeTimer += Time.deltaTime;
            NormalSpeed = _previousMoveSpeed;

            switch (_modeChangeIteration)
            {
                case 0:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[0])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (_currentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[1])
                    {
                        _modeChangeIteration = 1;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 1:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[2])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (_currentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[3])
                    {
                        _modeChangeIteration = 2;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 2:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[4])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (_currentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[5])
                    {
                        _modeChangeIteration = 3;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 3:
                    if (_currentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[6])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    break;
            }
        }
        else if (_currentGhostMode == GhostMode.Frightened)
            NormalSpeed = FrightenedSpeed;

        else if (_currentGhostMode == GhostMode.Eaten)
            NormalSpeed = EatenSpeed;
    }

    public void SetFrightenedModeTo(bool setTo)
    {
        if (_currentGhostMode == GhostMode.Eaten) return;

        ChangeGhostMode(setTo ? GhostMode.Frightened : _previousGhostMode);
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
        switch (_currentGhostMode)
        {
            case GhostMode.Scatter:
                return _scatterTile.transform.localPosition;
            
            case GhostMode.Chase:
                switch (GhostType)
                {
                    case GhostType.Blinky:
                        return _player.transform.localPosition;

                    case GhostType.Pinky:
                        return _player.transform.localPosition +
                               4 * Helpers.GetVectorDirection(_playerController.CurrentMoveOrientation);

                    case GhostType.Inky:
                        var blinky = GameController.Instance.Ghosts.FirstOrDefault(g => g.GhostType == GhostType.Blinky);
                        var blinkyPosition = blinky != null ? blinky.transform.localPosition : _player.transform.localPosition;
                        var targetTile = _player.transform.localPosition +
                                         2 * Helpers.GetVectorDirection(_playerController.CurrentMoveOrientation);
                        var inkyDistance = Helpers.GetDistanceBetweenVectors(blinkyPosition, targetTile);

                        return new Vector3(blinkyPosition.x + inkyDistance, blinkyPosition.y + inkyDistance, blinkyPosition.z);

                    case GhostType.Clyde:
                        var clydeDistance = Helpers.GetDistanceBetweenVectors(transform.localPosition, _player.transform.localPosition);
                    
                        return clydeDistance <= 8 ? _scatterTile.transform.localPosition : _player.transform.localPosition;

                    default:
                        return Vector3.zero;
                }

            case GhostMode.Frightened:
                return new Vector3(UnityEngine.Random.Range(0, 30), UnityEngine.Random.Range(0, 26), 0f);

            case GhostMode.Eaten:
                var ghostTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
                    m.GetComponent<IntersectionTile>() != null && m.GetComponent<IntersectionTile>().IsGhostHouseEntrance);

                return ghostTile != null ? ghostTile.transform.localPosition : Vector3.zero;

            default:
                return Vector3.zero;
        }
    }

    private void CheckForReleaseGhost()
    {
        if (!IsInGhostHouse) return;
        
        _currentReleaseTimer += Time.deltaTime;

        if (!(_currentReleaseTimer > TimeUntilRelease)) return;

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
                _currentIntersectionTile.NeighborDirections[i] != Helpers.GetVectorDirection(_currentMoveDirection) * -1)
            {
                var distance = Helpers.GetDistanceBetweenVectors
                    (_currentIntersectionTile.Neighbors[i].transform.localPosition, targetTile);

                if (distance < leastDistance)
                {
                    leastDistance = distance;
                    targetNode = _currentIntersectionTile.Neighbors[i];
                    _currentMoveDirection = Helpers.GetDirectionFromVector(_currentIntersectionTile.NeighborDirections[i]);
                }
            }
        }

        return targetNode;
    }

    private void CheckCollision()
    {
        if (Helpers.GetDistanceBetweenVectors(transform.localPosition, _player.transform.localPosition) > 0.5f)
            return;

        if (_currentGhostMode == GhostMode.Frightened)
            ChangeGhostMode(GhostMode.Eaten);
        else if (_currentGhostMode != GhostMode.Frightened && _currentGhostMode != GhostMode.Eaten)
            Debug.Log("f in the chat");
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

    private void GetScatterTile()
    {
        switch(GhostType)
        {
            case GhostType.Blinky:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 25 && m.TileY == 31);
                break;
            
            case GhostType.Pinky:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 2 && m.TileY == 31);
                break;
            
            case GhostType.Inky:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 25 && m.TileY == -1);
                break;
            
            case GhostType.Clyde:
                _scatterTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m => m.TileX == 2 && m.TileY == -1);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateAnimation()
    {
        _spriteAnimator.SetBool("Frightened", _currentGhostMode == GhostMode.Frightened);
        _spriteAnimator.SetBool("FrightenedEnding", _currentGhostMode == GhostMode.Frightened && 
                                                    GameController.Instance.IsEnergizedTimeEnding);
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

[Serializable]
public enum GhostType
{
    Blinky,
    Pinky,
    Inky,
    Clyde
}