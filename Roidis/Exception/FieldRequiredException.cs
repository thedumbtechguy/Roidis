using FastMember;

namespace Roidis.Exception
{
    public class FieldRequiredException : RoidException
    {
        public FieldRequiredException(Member member) : base($"{member.Name} is required")
        {
        }
    }
}