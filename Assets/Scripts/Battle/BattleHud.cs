using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
   [SerializeField] Text nameText;
   [SerializeField] Text levelText;
   [SerializeField] HPBar hpBar;
   [SerializeField] Text statusText;
   [SerializeField] GameObject expBar;



   [SerializeField] Color psnColor;
   [SerializeField] Color brnColor;
   [SerializeField] Color slpColor;
   [SerializeField] Color frzColor;
   [SerializeField] Color parColor;

   Pokemon _pokemon;
   Dictionary<ConditionID, Color> statusColors;


   public void SetData(Pokemon pokemon)
   {
      _pokemon = pokemon;
      nameText.text = pokemon.Base.PokeName;
      levelText.text = "Lvl " + pokemon.Level;
      hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
      SetExp();

      statusColors = new Dictionary<ConditionID, Color>()
      {
         {ConditionID.psn, psnColor},
         {ConditionID.brn, brnColor},
         {ConditionID.slp, slpColor},
         {ConditionID.frz, frzColor},
         {ConditionID.par, parColor},
      };

      SetStatusText();
      _pokemon.OnStatusChanged += SetStatusText;

   }
   public void SetStatusText()
   {
      if (_pokemon.Status == null)
      {
         statusText.text = "";
      }
      else
      {
         statusText.text = _pokemon.Status.Id.ToString().ToUpper();
       
         statusText.color = statusColors[_pokemon.Status.Id];
      }
   }

   public IEnumerator UpdateHP()
   {
      if (_pokemon.HpChanged)
      {
         yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
         _pokemon.HpChanged = false;
      }
   }

public void SetExp()
{
   if (expBar == null) return;

   float normalizedExp = GetNormalizedExp();
   expBar.transform.localScale = new Vector3( normalizedExp, 1 , 1);

}

public IEnumerator SetExpSmooth()
{
   if (expBar == null)  yield break;

   float normalizedExp = GetNormalizedExp();
 yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();

}

float GetNormalizedExp()
{
   int currentLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
   int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

   float normalizedExp = (float)(_pokemon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);
   return Mathf.Clamp01(normalizedExp);

}

}
