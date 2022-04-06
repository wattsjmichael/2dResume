using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Condition : MonoBehaviour
{
   public string CondName {get; set;}
   public string Description {get; set;}
   public string StartMessage {get; set;}

   public Action<Pokemon> OnAfterTurn {get; set;}


}
