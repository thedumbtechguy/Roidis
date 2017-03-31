using Roidis.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Exception
{
    public class DuplicateRoidKeyAttributeException: RoidException
    {
        public DuplicateRoidKeyAttributeException(Type type) : base($"Duplicate {nameof(RoidKeyAttribute)} declared in '{type.FullName}'") { }
    }
}
