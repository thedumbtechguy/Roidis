using Roidis.Attribute;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Exception
{
    public class MissingRoidKeyException : RoidException
    {
        public MissingRoidKeyException(Type type) : base($"'{type.FullName}' does not have a valid Key field") { }
    }
}
