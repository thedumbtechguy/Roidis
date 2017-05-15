using System;

namespace Roidis.Exception
{
    public class UnsupportedKeyTypeException : RoidException
    {
        public UnsupportedKeyTypeException(Type type) : base($"'{type.FullName}' is not a supported Key field type")
        {
        }
    }
}