using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB : MonoBehaviour
{
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
                        pokemon.StatusChanges.Enqueue(
                            $"{pokemon.Base.PokeName} is being burned"
                        );
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
    frz
}
