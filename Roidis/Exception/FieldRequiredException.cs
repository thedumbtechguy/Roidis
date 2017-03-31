using FastMember;
using Roidis.Attribute;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Exception
{
    public class MemberRequiredException : RoidException
    {
        public MemberRequiredException(Member member) : base($"{member.Name} is required") { }
    }
}
