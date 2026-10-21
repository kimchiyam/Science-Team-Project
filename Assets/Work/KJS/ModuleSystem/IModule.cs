using ModuleSystem;

namespace Work.KJS.ModuleSystem
{
    public interface IModule
    {
        public void Initialize(ModuleOwner owner);
    }
}