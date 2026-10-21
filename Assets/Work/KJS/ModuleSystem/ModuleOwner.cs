using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Work.KJS.ModuleSystem;

namespace ModuleSystem
{
    public class ModuleOwner : MonoBehaviour
    {
        Dictionary<Type,IModule> _modules = new();

        private void Awake()
        {
            _modules = GetComponentsInChildren<IModule>().ToDictionary(m => m.GetType());
            InitializeModules();
        }

        protected virtual void InitializeModules()
        {
            foreach(IModule module in _modules.Values)
            {
                module.Initialize(this);
            }
        }

        public T GetModule<T>() where T : IModule
        {
            if (_modules.TryGetValue(typeof(T), out IModule module))
            {
                return (T)module;
            }
            
            IModule findModule = _modules.Values.FirstOrDefault(x => x is T);
            
            if(findModule != null)
                return (T)findModule;
            
            return default;
        }
    }
}