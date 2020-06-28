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

    private int _currentScore;

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
}
