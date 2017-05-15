using Roidis.Service.Definition;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Roidis.Service.Mapper
{
    public interface IHashMapper
    {
        List<HashEntry> HashFor<T>(ITypeDefinition<T> definition, T instance, bool isNew, HashEntry[] existingRecord);

        T InstanceFor<T>(ITypeDefinition<T> definition, HashEntry[] hash) where T : new();
    }
}