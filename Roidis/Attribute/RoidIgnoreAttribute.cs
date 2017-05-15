using System;

namespace Roidis.Attribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RoidIgnoreAttribute : System.Attribute
    {
        public readonly bool OnCreate;
        public readonly bool OnUpdate;

        public RoidIgnoreAttribute(bool OnCreate = true, bool OnUpdate = true)
        {
            this.OnCreate = OnCreate;
            this.OnUpdate = OnUpdate;
        }
    }
}