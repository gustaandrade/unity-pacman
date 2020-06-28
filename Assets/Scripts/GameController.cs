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
    public int GhostsEaten;
    public int PelletsConsumed;
    public int EnergizersConsumed;
    public float TimeConsumed;

    [Space(10), Header("Level Variables")]
    public bool IsEnergized;
    public bool IsEnergizedTimeEnding;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        Player = MazeAssembler.Instance.GetComponentInChildren<PlayerController>();
        Ghosts = MazeAssembler.Instance.GetComponentsInChildren<GhostController>().ToList();
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

    public void SetPelletsConsumed()
    {
        PelletsConsumed++;
    }

    public void SetEnergizersConsumed()
    {
        EnergizersConsumed++;
    }

    public void ChangeFrightenedModeTo(bool changeTo)
    {
        IsEnergized = changeTo;
        Ghosts.ForEach(g => g.SetFrightenedModeTo(changeTo));
    }
}
