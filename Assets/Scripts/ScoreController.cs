﻿using System;
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
    private int _highScore;

    private MazeTile _fruitTile;
    private SpriteRenderer _fruitSprite;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _currentScore = PlayerPrefs.HasKey("CurrentScore") 
            ? PlayerPrefs.GetInt("CurrentScore")
            : 0;

        _highScore = PlayerPrefs.HasKey("HighScore")
            ? PlayerPrefs.GetInt("HighScore")
            : 0;

        LevelScore.text = _currentScore.ToString("000000");
        HighScore.text = _highScore.ToString("000000");

        _fruitTile = MazeAssembler.Instance.FruitParent.GetComponentInChildren<MazeTile>();
        _fruitSprite = _fruitTile.GetComponentInChildren<SpriteRenderer>();

        _fruitSprite.sprite = GameController.Instance.GetFruitSpriteFromData(LevelController.Instance.GetCurrentLevel().BonusFruit);
        _fruitTile.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameController.Instance.GetPelletsConsumed() == 70 ||
            GameController.Instance.GetPelletsConsumed() == 170)
            SpawnFruit();
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

        if (_currentScore > _highScore)
            HighScore.text = _currentScore.ToString("000000");
    }

    public void SaveLevelScore()
    {
        PlayerPrefs.SetInt("CurrentScore", _currentScore);
    }

    public void SetGhostScore()
    {
        switch (GameController.Instance.GhostsEaten)
        {
            case 1:
                SetLevelScore(200);
                break;
            case 2:
                SetLevelScore(400);
                break;
            case 3:
                SetLevelScore(800);
                break;
            case 4:
                SetLevelScore(1600);
                break;
        }
    }

    public void SetFruitScore()
    {
        SetLevelScore(LevelController.Instance.GetCurrentLevel().FruitPoints);

        StopAllCoroutines();
    }

    public void SetHighScore()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            if (PlayerPrefs.GetInt("HighScore") < PlayerPrefs.GetInt("CurrentScore"))
                PlayerPrefs.SetInt("HighScore", PlayerPrefs.GetInt("CurrentScore"));
        }
        else
            PlayerPrefs.SetInt("HighScore", PlayerPrefs.GetInt("CurrentScore"));

        PlayerPrefs.SetInt("CurrentScore", 0);
    }

    private void SpawnFruit()
    {
        StopAllCoroutines();

        StartCoroutine(nameof(SpawnFruitRoutine));
    }

    private IEnumerator SpawnFruitRoutine()
    {
        _fruitTile.gameObject.SetActive(true);

        yield return new WaitForSeconds(6);

        _fruitTile.gameObject.SetActive(false);
    }


}
