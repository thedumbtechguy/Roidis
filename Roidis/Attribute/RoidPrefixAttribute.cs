using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RoidPrefixAttribute : System.Attribute
    {
        public readonly string Prefix;

        public RoidPrefixAttribute(string Prefix)
        {
            if (string.IsNullOrWhiteSpace(Prefix))
                throw new ArgumentNullException(nameof(Prefix));

            this.Prefix = Prefix;
        }
    }
}
