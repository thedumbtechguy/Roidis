using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
