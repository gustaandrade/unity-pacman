using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    [Space(10), Header("Score Indicators")]
    public GameObject LevelContainer;
    public GameObject LevelPrefab;

    private LevelsConfiguration _levelsConfiguration;

    private int _currentLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _levelsConfiguration = Resources.Load<LevelsConfiguration>("LevelsConfiguration");

        _currentLevel = PlayerPrefs.HasKey("CurrentLevel")
            ? PlayerPrefs.GetInt("CurrentLevel")
            : 1;

        // updates the UI with the corresponding level fruit notations
        foreach (var fruit in GetCurrentLevel().LevelFruits.Where(f => f != LevelFruit.None))
        {
            var levelFruit = Instantiate(LevelPrefab, LevelContainer.transform);
            levelFruit.GetComponent<Image>().sprite = GameController.Instance.GetFruitSpriteFromData(fruit);
        }
    }

    /// <summary>
    /// Returns the current level
    /// </summary>
    /// <returns></returns>
    public Level GetCurrentLevel()
    {
        return _levelsConfiguration.Levels.FirstOrDefault(l => l.LevelNumber == _currentLevel);
    }

    /// <summary>
    /// Advances one level after a win
    /// </summary>
    public void AdvanceLevel()
    {
        _currentLevel += 1;
        PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
    }
}
