using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB : MonoBehaviour
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } =
        new Dictionary<ConditionID, Condition>()
        {
            {
                ConditionID.psn,
                new Condition()
                {
                    CondName = "Poison",
                    StartMessage = "has been poisoned",
                    OnAfterTurn = (Pokemon pokemon) =>
                    {
                        pokemon.UpdateHP(pokemon.MaxHp / 8);
                        pokemon.StatusChanges.Enqueue(
                            $"{pokemon.Base.PokeName} is being hurt by poison"
                        );
                    }
                }
            },
            {
                ConditionID.brn,
                new Condition()
                {
                    CondName = "Burn",
                    StartMessage = "has been burnt",
                    OnAfterTurn = (Pokemon pokemon) =>
                    {
                        pokemon.UpdateHP(pokemon.MaxHp / 2);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.PokeName} is being burned");
                    }
                }
            },
            {
                ConditionID.par,
                new Condition()
                {
                    CondName = "Paralyzed",
                    StartMessage = "has been paralyzed",
                    OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (Random.Range(1, 5) == 1)
                        {
                            pokemon.StatusChanges.Enqueue(
                                $"{pokemon.Base.PokeName} has been paralyzed and cant move"
                            );
                            return false;
                        }
                        return true;
                    }
                }
            },
            {
                ConditionID.frz,
                new Condition()
                {
                    CondName = "Freeze",
                    StartMessage = "has been frozen",
                    OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (Random.Range(1, 5) == 1)
                        {
                            pokemon.CureStatus();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.PokeName} is not frozen");
                            return true;
                        }
                        return false;
                    }
                }
            },
            {
                ConditionID.slp,
                new Condition()
                {
                    CondName = "Sleep",
                    StartMessage = "has fallen asleep",
                    OnStart = (Pokemon pokemon) =>
                    {
                        //sleep for 1 -3 turns
                        pokemon.StatusTime = Random.Range(1, 4);
                        Debug.Log(pokemon.StatusTime);
                    },
                    OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (pokemon.StatusTime <= 0)
                        {
                            pokemon.CureStatus();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.PokeName} woke up");
                            return true;
                        }

                        pokemon.StatusTime--;
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.PokeName} is sleeping");
                        return false;
                    }
                }
            },
            {
                ConditionID.confusion,
                new Condition()
                {
                    CondName = "Confusion",
                    StartMessage = "is confused",
                    OnStart = (Pokemon pokemon) =>
                    {
                        //confused for 1 -4 turns
                        pokemon.VolatileStatusTime = Random.Range(1, 5);
                        Debug.Log(pokemon.VolatileStatusTime);
                    },
                    OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (pokemon.VolatileStatusTime <= 0)
                        {
                            pokemon.CureVolatileStatus();
                            pokemon.StatusChanges.Enqueue(
                                $"{pokemon.Base.PokeName} kicked out of confusion"
                            );
                            return true;
                        }

                        pokemon.VolatileStatusTime--;

                        //50% chance to perform the move
                        if (Random.Range(1, 3) == 1)
                            return true;

                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.PokeName} is confused");
                        pokemon.UpdateHP(pokemon.MaxHp/8);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.PokeName} hit itself");
                        return false;
                    }
                }
            }
        };
}

public enum ConditionID
{
    none,
    psn,
    brn,
    slp,
    par,
    frz,
    confusion
}
