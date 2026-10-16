using UnityEngine;
using Work.KJS.ModuleSystem;

namespace ModuleSystem
{
    public class MonoModule : MonoBehaviour , IModule
    {
        protected ModuleOwner _owner;
        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
        }
    }
}