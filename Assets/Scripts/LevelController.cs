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

        foreach (var fruit in GetCurrentLevel().LevelFruits.Where(f => f != LevelFruit.None))
        {
            var levelFruit = Instantiate(LevelPrefab, LevelContainer.transform);
            levelFruit.GetComponent<Image>().sprite = GameController.Instance.GetFruitSpriteFromData(fruit);
        }
    }

    public Level GetCurrentLevel()
    {
        return _levelsConfiguration.Levels.FirstOrDefault(l => l.LevelNumber == _currentLevel);
    }

    public void AdvanceLevel()
    {
        _currentLevel += 1;
        PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
    }
}
