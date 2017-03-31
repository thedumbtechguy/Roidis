using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Attribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RoidIndexAttribute : System.Attribute
    {
        public readonly string Name;

        /// <summary>
        /// Indicate that this field should be indexed
        /// </summary>
        /// <param name="Name">Name of the index. Defaults to field name.</param>
        public RoidIndexAttribute(string Name = null)
        {
            this.Name = string.IsNullOrWhiteSpace(Name) ? null : Name;
        }
    }
}
