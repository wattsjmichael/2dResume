using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField]
    Dialog dialog;

    [SerializeField]
    List<Vector2> movementPattern;

    [SerializeField]
    float timeBetweenPattern;
    Character character;
    NPCState state;

    float idleTimer = 0f;
    int currentPattern = 0;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initator)
    {
        if (state == NPCState.Idle)
        {
        state = NPCState.Dialog;
        character.LookTowards(initator.position);
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
            idleTimer = 0f;
            state = NPCState.Idle;
        }

        ));


        }
        //    StartCoroutine(character.Move(new Vector2(-2, 0)));
    }

    public void Update()
    {
        
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;
        yield return character.Move(movementPattern[currentPattern]);
        if (transform.position !=oldPos)
        currentPattern = (currentPattern +1) % movementPattern.Count;
        state = NPCState.Idle;
    }
}

public enum NPCState
{
    Idle,
    Walking,
    Dialog
}
