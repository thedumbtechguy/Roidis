using System;

namespace Roidis.Exception
{
    public class MissingRoidKeyException : RoidException
    {
        public MissingRoidKeyException(Type type) : base($"'{type.FullName}' does not have a valid Key field")
        {
        }
    }
}