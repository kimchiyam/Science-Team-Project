using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Work.KJS._00.Scripts
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class InputSO : ScriptableObject, InputActions.IPlayerActions
    {
        private InputActions _inputActions;
        public Vector2 MoveInput { get; private set; }

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new InputActions();
                _inputActions.Player.SetCallbacks(this);
            }
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            if (_inputActions != null)
            {
                _inputActions.Disable();
                _inputActions.Player.RemoveCallbacks(this);
            }
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }
        
        public void OnNewaction(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
        }

        
    }
    
}