﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    ActionSelection,
    MoveSelection,
    PerformMove,
    Busy,
    PartyScreen,
    BattleOver
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField]
    BattleUnit playerUnit;

    [SerializeField]
    BattleUnit enemyUnit;

    [SerializeField]
    BattleDialogBox dialogBox;

    [SerializeField]
    PartyScreen partyScreen;
    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    public event Action<bool> OnBattleOver;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return (
            dialogBox.TypeDialog($"A Wild {enemyUnit.Pokemon.Base.PokeName} appeared!!!")
        );

        ChooseFirstTurn();
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = source.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy *= boostValues[evasion];
        else
            moveAccuracy /= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChange(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChange(sourceUnit.Pokemon);
        move.PP--; //Decrements PP

        yield return dialogBox.TypeDialog(
            $"{sourceUnit.Pokemon.Base.PokeName} used {move.Base.MoveName}"
        );

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondaryeffects in move.Base.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondaryeffects.Chance)
                    {
                        yield return RunMoveEffects(secondaryeffects, sourceUnit.Pokemon, targetUnit.Pokemon, secondaryeffects.Target);
                    }

                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog(
                    $"{targetUnit.Pokemon.Base.PokeName} has fainted"
                );
                targetUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2.0f);

                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.PokeName} attack missed");
        }

        //STATUSES LIKE BURN/PSN will hurt unit after the turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChange(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.PokeName} has fainted");
            sourceUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2.0f);

            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        
        //STAT BOOSTING
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }

            //STATUS CONDITIONS
            if (effects.Status != ConditionID.none)
            {
                target.SetStatus(effects.Status);
            }
            //Volatile Status
            if (effects.VolatileStatus != ConditionID.none)
            {
                target.SetVolatileStatus(effects.VolatileStatus);
            }

            yield return ShowStatusChange(source);
            yield return ShowStatusChange(target);
        }
    }

    IEnumerator ShowStatusChange(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
            ActionSelection();
        else
            StartCoroutine(EnemyMove());
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if (state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMove)
            ActionSelection();
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A Critical Hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's Super Effective!!!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's Not Effective!!!");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                Debug.Log("Bag");
            }
            else if (currentAction == 2)
            {
                Debug.Log("Pokemon");
                OpenPartyScreen();
            }
            else if (currentAction == 1)
            {
                Debug.Log("RUN!!!!");
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);

            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't send out the same pokemon");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        if (playerUnit.Pokemon.HP > 0)
        {
            currentPokemonFainted = false;
            yield return dialogBox.TypeDialog($"Come Back {playerUnit.Pokemon.Base.PokeName}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return (dialogBox.TypeDialog($"Go {newPokemon.Base.PokeName}!"));

        if (currentPokemonFainted)
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove());
    }
}
