using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Attribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RoidFieldAttribute : System.Attribute
    {
        public readonly string Name;

        public RoidFieldAttribute(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException(nameof(Name));

            this.Name = Name;
        }
    }
}
