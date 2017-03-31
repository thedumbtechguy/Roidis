using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Attribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RoidFieldNameAttribute : System.Attribute
    {
        public readonly string Name;

        public RoidFieldNameAttribute(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException(nameof(Name));

            Name = Name.Trim();

            if (Name.StartsWith(Constants.InternalPrefix))
                throw new ArgumentException($"Name cannot start with {Constants.InternalPrefix}");

            this.Name = Name;
        }
    }
}
