using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField]
    string trainerName;

    [SerializeField]
    Sprite sprite;

    [SerializeField]
    GameObject exclaimation;

    [SerializeField]
    GameObject fov;

    [SerializeField]
    Dialog dialog;

    [SerializeField]
    Dialog dialogAfterBattle;

    bool battleLost = false;

    Character character; //Grabs reference to charracter script

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        exclaimation.SetActive(true);
        yield return new WaitForSeconds(.5f);
        exclaimation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));
        yield return character.Move(moveVec);

        StartCoroutine(
            DialogManager.Instance.ShowDialog(
                dialog,
                () =>
                {
                    GameController.Instance.StartTrainerBattle(this);
                }
            )
        );
    }

    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        if (!battleLost)
        {
            StartCoroutine(
                DialogManager.Instance.ShowDialog(
                    dialog,
                    () =>
                    {
                        GameController.Instance.StartTrainerBattle(this);
                    }
                )
            );
        }
        else 
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
        }
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public string TrainerName
    {
        get => trainerName;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
