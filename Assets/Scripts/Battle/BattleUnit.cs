﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
  

    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public BattleHud Hud {
        get {return hud;}
    }
    
    
    
    public bool IsPlayerUnit
    {
        get { return isPlayerUnit;}
    }



    Image image;
    Vector3 originalPos;

    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public Pokemon Pokemon { get; set; }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
            image.sprite = Pokemon.Base.BackSprite;
        else
            image.sprite = Pokemon.Base.FrontSprite;
            image.SetNativeSize();
            hud.gameObject.SetActive(true);
            hud.SetData(pokemon);

            image.color = originalColor;

            PlayEnterAnimation();
    }

    public void Clear(){
        hud.gameObject.SetActive(false);

    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3 (-500f, originalPos.y);
        }
        else 
        {
             image.transform.localPosition = new Vector3 (500f, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, .25f));
        else
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, .25f));

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, .25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, .5f));
        sequence.Join(image.DOFade(0f, .5f));
    }


}
