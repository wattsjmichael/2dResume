using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField]
    List<Sprite> walkDownSprites;

    [SerializeField]
    List<Sprite> walkUpSprites;

    [SerializeField]
    List<Sprite> walkLeftSprites;

    [SerializeField]
    List<Sprite> walkRightSprites;

    [SerializeField]
    FacingDirection defaultdirection = FacingDirection.Down;

    //Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }

    //States
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator walkRightAnim;

    SpriteAnimator currentAnim;
    bool wasPrevMoving;

    // references
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        SetFacingDirection(defaultdirection);

        currentAnim = walkDownAnim;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        var prevAnim = currentAnim;
        if (MoveX == 1)
            currentAnim = walkRightAnim;
        else if (MoveX == -1)
            currentAnim = walkLeftAnim;
        else if (MoveY == 1)
            currentAnim = walkUpAnim;
        else if (MoveY == -1)
            currentAnim = walkDownAnim;

        if (currentAnim != prevAnim || IsMoving != wasPrevMoving)
            currentAnim.Start();

        if (IsMoving)
            currentAnim.HandleUpdate();
        else
            spriteRenderer.sprite = currentAnim.Frames[0];
        wasPrevMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirection dir) 
    {
        if (dir == FacingDirection.Right)
        {
            MoveX = 1;
        }
       else if (dir == FacingDirection.Left)
        {
            MoveX = -1;
        }
        else if (dir == FacingDirection.Up)
        {
            MoveY = 1;
        }
        else if (dir == FacingDirection.Down)
        {
            MoveY = -1;
        }
    
     }
     public FacingDirection DefaultDirection
     {
         get => defaultdirection;
     }
}

public enum FacingDirection
{
    Up,
    Down,
    Left,
    Right
}
