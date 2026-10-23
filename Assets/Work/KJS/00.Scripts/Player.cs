using System;
using ModuleSystem;
using UnityEngine;

namespace Work.KJS._00.Scripts
{
    public class Player : ModuleOwner
    {
        private AgentMovement _agentMovement;
        [SerializeField] private InputSO _input;
        protected override void InitializeModules()
        {
            base.InitializeModules();
            _agentMovement = GetModule<AgentMovement>();
        }

        private void Update()
        {
            _agentMovement.SetDir(_input.MoveInput);
        }
    }
}