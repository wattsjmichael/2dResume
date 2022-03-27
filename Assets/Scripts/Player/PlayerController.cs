using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask grassLayer;

    public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal") ;
            input.y = Input.GetAxisRaw("Vertical");

            // remove diagonal movement
            if (input.x != 0)
                input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                
                if (IsWalkable(new Vector3(targetPos.x,targetPos.y-0.5f)))
                StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    //CoRoutine
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null) 
        {
           return false; 
        }
        return true;
    } 

    private void CheckForEncounters()
    {
         if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
         {
            if (UnityEngine.Random.Range(1, 101) <= 12)
            {   
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
         }
    }

}
