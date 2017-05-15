using System;

namespace Roidis.Exception
{
    public class UnsupportedMemberTypeException : RoidException
    {
        public UnsupportedMemberTypeException(Type type) : base($"'{type.FullName}' is not a supported Key member type")
        {
        }
    }
}