using System;

namespace Astrum
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class AstralWorldTargetAttribute : Attribute
    {
        public Type Type;
        public string WorldID;
        public string SceneName;
    }
}
