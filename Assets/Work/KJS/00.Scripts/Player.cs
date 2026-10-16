using ModuleSystem;
using UnityEngine;

namespace Work.KJS._00.Scripts
{
    public class Player : ModuleOwner
    {
        private AgentMovement _agentMovement;
        protected override void InitializeModules()
        {
            base.InitializeModules();
            _agentMovement = GetModule<AgentMovement>();
        }
    }
}