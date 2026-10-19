using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMananger : MonoBehaviour
{
   [SerializeField] private float currentTime;
   [SerializeField] private float endTime;
   private PLayType modes;
   private bool isReady;

   private void Update()
   {
      switch (modes)
      {
         case PLayType.Play:
            currentTime += Time.deltaTime;
            if (currentTime > endTime)
            {
               modes = PLayType.End;
            }

            break;
         case PLayType.End:
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
               modes = PLayType.Ready;
            break;
         case PLayType.Ready:
            StartCoroutine(WaitTime());
            break;
      }
   }

   private IEnumerator WaitTime()
   {
      yield return new WaitForSecondsRealtime(2f);
      currentTime = 0f;
      modes = PLayType.Play;
   }
}

public enum PLayType
{
   Ready,
   Play,
   End
}