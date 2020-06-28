using System;
using System.Linq;
using UnityEngine;

public class GhostController : MonoBehaviour, IMazeEntity
{
    [Space(10), Header("Ghost Types")] 
    public GhostType GhostType;
    public GhostMode CurrentGhostMode;

    [Space(10), Header("Ghost Variables")]
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

    private GhostMode _previousGhostMode;

    private Animator _spriteAnimator;

    private void Awake()
    {
        _spriteAnimator = GetComponentInChildren<Animator>();

        _player = GameObject.FindGameObjectWithTag("Player");
        _playerController = _player.GetComponent<PlayerController>();

        // sets the first movement of each ghost to scatter, using the out of bounds tiles
        CurrentGhostMode = GhostMode.Scatter;
        GetScatterTile();

        var nextTile = Helpers.GetIntersectionTile(transform.localPosition);
        if (nextTile != null)
            _currentIntersectionTile = nextTile;

        // only blinky starts outside the ghost house, and moves left first
        // the other 3 ghosts need to leave the ghost house first
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

    /// <summary>
    /// Moves each ghost based on the chosen IntersectionTiles
    /// </summary>
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

                if (_currentIntersectionTile.IsGhostHouseEntrance && CurrentGhostMode == GhostMode.Eaten)
                    ChangeGhostMode(_previousGhostMode);

                _targetIntersectionTile = ChooseNextTile();
                _previousIntersectionTile = _currentIntersectionTile;
                _currentIntersectionTile = null;
            }
            else
                transform.localPosition += Helpers.GetVectorDirection(_currentMoveDirection) * NormalSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Changes the behavior of each ghost depending of their Typing and Mode
    /// </summary>
    private void HandleGhostMode()
    {
        if (CurrentGhostMode != GhostMode.Frightened && CurrentGhostMode != GhostMode.Eaten && !IsInGhostHouse)
        {
            _modeChangeTimer += Time.deltaTime;
            NormalSpeed = _previousMoveSpeed;

            // each iteration is based on the level difficulties, and how the ghosts change from scatter to chase
            // all data are saved on a ScriptableObject with the difficult settings
            switch (_modeChangeIteration)
            {
                case 0:
                    if (CurrentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[0])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (CurrentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[1])
                    {
                        _modeChangeIteration = 1;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 1:
                    if (CurrentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[2])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (CurrentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[3])
                    {
                        _modeChangeIteration = 2;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 2:
                    if (CurrentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[4])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    if (CurrentGhostMode == GhostMode.Chase &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[5])
                    {
                        _modeChangeIteration = 3;
                        ChangeGhostMode(GhostMode.Scatter);
                        _modeChangeTimer = 0f;
                    }
                    break;

                case 3:
                    if (CurrentGhostMode == GhostMode.Scatter &&
                        _modeChangeTimer > LevelController.Instance.GetCurrentLevel().GhostTimers[6])
                    {
                        ChangeGhostMode(GhostMode.Chase);
                        _modeChangeTimer = 0f;
                    }
                    break;
            }
        }
        else if (CurrentGhostMode == GhostMode.Frightened)
            NormalSpeed = FrightenedSpeed;

        else if (CurrentGhostMode == GhostMode.Eaten)
            NormalSpeed = EatenSpeed;
    }

    public void SetFrightenedModeTo(bool setTo)
    {
        if (CurrentGhostMode == GhostMode.Eaten) return;

        ChangeGhostMode(setTo ? GhostMode.Frightened : _previousGhostMode);
    }

    /// <summary>
    /// Changes the Mode of a Ghost and stores the previous Mode to easy change
    /// </summary>
    /// <param name="mode">The desired mode to change</param>
    private void ChangeGhostMode(GhostMode mode)
    {
        if (CurrentGhostMode == mode) return;
     
        if (CurrentGhostMode != GhostMode.Frightened && CurrentGhostMode != GhostMode.Eaten)
            _previousGhostMode = CurrentGhostMode;
        
        CurrentGhostMode = mode;
    }

    /// <summary>
    /// Gets the next target tile that the Ghost needs to travel
    /// </summary>
    /// <returns>The target tile that the Ghost will travel to</returns>
    private Vector3 GetGhostTargetTile()
    {
        // first check is to determine which Ghost Mode the Ghost are currently
        switch (CurrentGhostMode)
        {
            // gets the scatter out of bounds tile to pursue
            case GhostMode.Scatter:
                return _scatterTile.transform.localPosition;
            
            // when in chase mode, all four ghosts behave different one from each other, as follows:
            case GhostMode.Chase:
                switch (GhostType)
                {
                    // Blinky always chases PacMan using the tile with Pacman's current position as reference
                    case GhostType.Blinky:
                        return _player.transform.localPosition;

                    // Pinky chases PacMan using the tile with Pacman's current position + 4 tiles, regarding the orientation
                    case GhostType.Pinky:
                        return _player.transform.localPosition +
                               4 * Helpers.GetVectorDirection(_playerController.CurrentMoveOrientation);

                    // Inky uses Blinky's position to based their tile to flank Pacman. They calculate the distance between Blinky and Pacman and,
                    // based on that value, traces a vector in the opposite direction and chooses that tile to pursue
                    case GhostType.Inky:
                        var blinky = GameController.Instance.Ghosts.FirstOrDefault(g => g.GhostType == GhostType.Blinky);
                        var blinkyPosition = blinky != null ? blinky.transform.localPosition : _player.transform.localPosition;
                        var targetTile = _player.transform.localPosition +
                                         2 * Helpers.GetVectorDirection(_playerController.CurrentMoveOrientation);
                        var inkyDistance = Helpers.GetDistanceBetweenVectors(blinkyPosition, targetTile);

                        return new Vector3(blinkyPosition.x + inkyDistance, blinkyPosition.y + inkyDistance, blinkyPosition.z);

                    // Clyde pursues the tile with Pacman's position, until they get in a tile that is less than 8 units distant.
                    // When this happens, Clyde changes their tile to the scatter mode tile, until the distance is more than 8 units again
                    case GhostType.Clyde:
                        var clydeDistance = Helpers.GetDistanceBetweenVectors(transform.localPosition, _player.transform.localPosition);
                    
                        return clydeDistance <= 8 ? _scatterTile.transform.localPosition : _player.transform.localPosition;

                    default:
                        return Vector3.zero;
                }

            // gets a random tile on the maze
            case GhostMode.Frightened:
                return new Vector3(UnityEngine.Random.Range(0, 30), UnityEngine.Random.Range(0, 26), 0f);

            // gets the first tile of the Ghost House Entrance
            // KNOWN ISSUE: The Ghost does not go inside the House yet, only to the entrance. This behavior needs to be fixed
            case GhostMode.Eaten:
                var ghostTile = MazeAssembler.Instance.MazeTiles.FirstOrDefault(m =>
                    m.GetComponent<IntersectionTile>() != null && m.GetComponent<IntersectionTile>().IsGhostHouseEntrance);

                return ghostTile != null ? ghostTile.transform.localPosition : Vector3.zero;

            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Releases each ghost using the predefined times
    /// </summary>
    private void CheckForReleaseGhost()
    {
        if (!IsInGhostHouse) return;
        
        _currentReleaseTimer += Time.deltaTime;

        if (!(_currentReleaseTimer > TimeUntilRelease)) return;

        IsInGhostHouse = false;
    }

    /// <summary>
    /// Chooses which tile the Ghost needs to travel based on less distance overall
    /// </summary>
    /// <returns>The next IntersectionTile for the Ghost to travel</returns>
    private IntersectionTile ChooseNextTile()
    {
        IntersectionTile targetNode = null;
        var leastDistance = 999999f;

        // gets the tile based on the TargetTile function
        var targetTile = GetGhostTargetTile();

        // searches for all possible tiles to travel and chooses the one that makes the overall travel shorter
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

    /// <summary>
    /// Checks if the Ghost collided with something, and does something for each relevant encounter
    /// </summary>
    private void CheckCollision()
    {
        if (Helpers.GetDistanceBetweenVectors(transform.localPosition, _player.transform.localPosition) > 0.5f)
            return;

        if (CurrentGhostMode == GhostMode.Frightened)
        {
            ChangeGhostMode(GhostMode.Eaten);
            GameController.Instance.SetGhostsEaten();
            ScoreController.Instance.SetGhostScore();
            SoundController.Instance.PlayGhostEatenSFX();
        }
        else if (CurrentGhostMode != GhostMode.Frightened && CurrentGhostMode != GhostMode.Eaten)
        {
            GameController.Instance.Defeat();
            SoundController.Instance.PlayDyingMusic();
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

    /// <summary>
    /// Gets each ghost scatter tile from the maze
    /// </summary>
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

    /// <summary>
    /// Updates Ghost animations depending of their Mode and Type
    /// </summary>
    public void UpdateAnimation()
    {
        _spriteAnimator.SetBool("Frightened", CurrentGhostMode == GhostMode.Frightened);
        _spriteAnimator.SetBool("FrightenedEnding", CurrentGhostMode == GhostMode.Frightened && 
                                                    GameController.Instance.IsEnergizedTimeEnding);
        _spriteAnimator.SetBool("Eaten", CurrentGhostMode == GhostMode.Eaten);

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