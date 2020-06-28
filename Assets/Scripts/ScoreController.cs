using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    public static ScoreController Instance;

    [Space(10), Header("Score Texts")]
    public Text LevelScore;
    public Text HighScore;

    [Space(10), Header("Game Variables")]
    public bool IsEnergized;
    public bool IsEnergizedTimeEnding;

    private int _currentScore;
    private int _pelletsConsumed;
    private int _energizersConsumed;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public int GetCurrentScore()
    {
        return _currentScore;
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore");
    }

    public int GetPelletsConsumed()
    {
        return _pelletsConsumed;
    }

    public int GetEnergizersConsumed()
    {
        return _energizersConsumed;
    }

    public void SetLevelScore(int score)
    {
        _currentScore += score;
        LevelScore.text = _currentScore.ToString("000000");
    }

    public void SetHighScore(int score)
    {
        HighScore.text = score.ToString("000000");
        PlayerPrefs.SetInt("HighScore", score);
    }

    public void SetPelletsConsumed()
    {
        _pelletsConsumed++;
    }

    public void SetEnergizersConsumed()
    {
        _energizersConsumed++;
    }
}
