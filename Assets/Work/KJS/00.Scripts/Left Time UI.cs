using System;
using TMPro;
using UnityEngine;

public class LeftTimeUI : MonoBehaviour
{
   private TextMeshProUGUI _text;
   [SerializeField]private GameState _gameState;

   private void Awake()
   {
      _text = GetComponent<TextMeshProUGUI>();
      
   }

   private void Update()
   {
      _text.text = $"Left Time : {_gameState.EndTime - _gameState.CurrentTime:F0}";
   }
}
