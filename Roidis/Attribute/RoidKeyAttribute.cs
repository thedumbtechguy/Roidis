using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Attribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RoidKeyAttribute : System.Attribute
    {
    }
}
