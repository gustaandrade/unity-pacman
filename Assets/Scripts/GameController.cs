using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Space(10), Header("Maze Entities")]
    public PlayerController Player;
    public List<GhostController> Ghosts = new List<GhostController>();

    [Space(10), Header("Level Values")] 
    public int Lives;
    public int GhostsEaten;
    public int PelletsConsumed;
    public int EnergizersConsumed;
    public float TimeConsumed;

    [Space(10), Header("Level Variables")]
    public bool IsEnergized;
    public bool IsEnergizedTimeEnding;

    [Space(10), Header("Bonus Fruits")]
    public Sprite Cherry;
    public Sprite Strawberry;
    public Sprite Orange;
    public Sprite Apple;
    public Sprite Melon;
    public Sprite Ship;
    public Sprite Bell;
    public Sprite Key;

    [Space(10), Header("Lives Objects")] 
    public GameObject LivesPrefab;
    public GameObject LivesContainer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        Lives = 5;

        Player = MazeAssembler.Instance.GetComponentInChildren<PlayerController>();
        Ghosts = MazeAssembler.Instance.GetComponentsInChildren<GhostController>().ToList();

        for (var i = 0; i < Lives; i++)
            Instantiate(LivesPrefab, LivesContainer.transform);
    }

    private void Update()
    {
        TimeConsumed += Time.deltaTime;
    }

    public int GetPelletsConsumed()
    {
        return PelletsConsumed;
    }

    public int GetEnergizersConsumed()
    {
        return EnergizersConsumed;
    }

    public int GetGhostsEaten()
    {
        return GhostsEaten;
    }

    public void SetPelletsConsumed()
    {
        PelletsConsumed++;
    }

    public void SetEnergizersConsumed()
    {
        EnergizersConsumed++;
    }

    public void SetGhostsEaten()
    {
        GhostsEaten++;
    }

    public void ChangeFrightenedModeTo(bool changeTo)
    {
        IsEnergized = changeTo;
        Ghosts.ForEach(g => g.SetFrightenedModeTo(changeTo));

        if (!changeTo) GhostsEaten = 0;
    }

    public Sprite GetFruitSpriteFromData(LevelFruit fruit)
    {
        switch (fruit)
        {
            case LevelFruit.Cherry: return Cherry;
            case LevelFruit.Strawberry: return Strawberry;
            case LevelFruit.Orange: return Orange;
            case LevelFruit.Apple: return Apple;
            case LevelFruit.Melon: return Melon;
            case LevelFruit.Ship: return Ship;
            case LevelFruit.Bell: return Bell;
            case LevelFruit.Key: return Key;
            case LevelFruit.None: return null;
            default:
                throw new ArgumentOutOfRangeException(nameof(fruit), fruit, null);
        }
    }
}
