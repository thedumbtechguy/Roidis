using Roidis.Attribute;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Exception
{
    public class UnsupportedMemberTypeException : RoidException
    {
        public UnsupportedMemberTypeException(Type type) : base($"'{type.FullName}' is not a supported Key member type") { }
    }
}
