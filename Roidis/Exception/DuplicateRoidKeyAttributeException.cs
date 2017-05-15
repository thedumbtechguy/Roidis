using Roidis.Attribute;
using System;

namespace Roidis.Exception
{
    public class DuplicateRoidKeyAttributeException : RoidException
    {
        public DuplicateRoidKeyAttributeException(Type type) : base($"Duplicate {nameof(RoidKeyAttribute)} declared in '{type.FullName}'")
        {
        }
    }
}