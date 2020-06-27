using System.Linq;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    private LevelsConfiguration _levelsConfiguration;

    private int _currentLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _levelsConfiguration = Resources.Load<LevelsConfiguration>("LevelsConfiguration");

        SetCurrentLevel(1);
    }

    public Level GetCurrentLevel()
    {
        return _levelsConfiguration.Levels.FirstOrDefault(l => l.LevelNumber == _currentLevel);
    }

    public void SetCurrentLevel(int level)
    {
        _currentLevel = level;
        PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
    }
}
