using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
   [SerializeField] private float currentTime;
   [SerializeField] private float endTime;
   private PLayType modes;
   private bool isReady = true;

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
            {
               SceneManager.LoadScene(1);
               modes = PLayType.Ready;
            }
            break;
         case PLayType.Ready:
            if(isReady)
            StartCoroutine(WaitTime());
            break;
      }
   }

   private IEnumerator WaitTime()
   {
      isReady = false;
      yield return new WaitForSecondsRealtime(2f);
      currentTime = 0f;
      modes = PLayType.Play;
      isReady = true;
   }
}

public enum PLayType
{
   Ready,
   Play,
   End
}