using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonBase _base;
    int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((_base.Attack * level) / 100f) + 5; } //Same formula as pokemon
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((_base.Defense * level) / 100f) + 5; } //Same formula as pokemon
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((_base.SpAttack * level) / 100f) + 5; } //Same formula as pokemon
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((_base.SpDefense * level) / 100f) + 5; } //Same formula as pokemon
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((_base.Speed * level) / 100f) + 5; } //Same formula as pokemon
    }
    public int MaxHp
    {
        get { return Mathf.FloorToInt((_base.MaxHp * level) / 100f) + 10; } //Same formula as pokemon
    }
}
