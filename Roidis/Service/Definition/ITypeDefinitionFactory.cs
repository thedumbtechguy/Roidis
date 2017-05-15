using FastMember;

namespace Roidis.Service.Definition
{
    public interface ITypeDefinitionFactory
    {
        ITypeDefinition<T> Create<T>(TypeAccessor accessor);
    }
}