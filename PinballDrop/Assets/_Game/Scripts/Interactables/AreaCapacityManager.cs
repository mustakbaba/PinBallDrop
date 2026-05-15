using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AreaCapacityManager : MonoSingleton<AreaCapacityManager>
{
   public int CapacityAmount;
   [SerializeField] private TextMeshPro _capacityText;
   private int _currentAmount;
   public int CurrentAmount => _currentAmount;
   protected override void Awake()
   {
      base.Awake();
      SetCapacityText();
   }
   
   private void SetCapacityText()
   {
      _capacityText.text = $"{_currentAmount}/{CapacityAmount}";
   }

   public void SetAmount(int comeAmount)
   {
      _currentAmount += comeAmount;
      SetCapacityText();
   }
}
