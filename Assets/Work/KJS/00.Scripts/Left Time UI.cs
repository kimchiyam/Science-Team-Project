using System;
using TMPro;
using UnityEngine;

public class LeftTimeUI : MonoBehaviour
{
   private TextMeshProUGUI _text;

   private void Awake()
   {
      _text = GetComponent<TextMeshProUGUI>();
      
   }

   private void Update()
   {
     // _text.text = $"Left Time : {MapCreateManager.instance.gamestate}";
   }
}
