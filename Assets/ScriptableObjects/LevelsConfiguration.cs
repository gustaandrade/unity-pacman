﻿using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsConfiguration", menuName = "ScriptableObjects/LevelsConfiguration", order = 1)]
public class LevelsConfiguration : ScriptableObject
{
    public Level[] Levels = new Level[19];
}

[Serializable]
public class Level
{
    public int LevelNumber;
    public LevelFruit[] LevelFruits = new LevelFruit[7];
    public LevelFruit BonusFruit;
    public int FruitPoints;
    public int NormalEnergizerTime;
    public int HardEnergizerTime;
    public float[] GhostTimers = new float[7];
}

[Serializable]
public enum LevelFruit
{
    Cherry,
    Strawberry,
    Orange,
    Apple,
    Melon,
    Ship,
    Bell,
    Key,
    None
}
