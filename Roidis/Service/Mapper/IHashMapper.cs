using Roidis.Service.Converter;
using Roidis.Service.Definition;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roidis.Service.Mapper
{
    public interface IHashMapper
    {
        List<HashEntry> HashFor<T>(ITypeDefinition<T> definition, T instance, bool isNew, HashEntry[] existingRecord);
        T InstanceFor<T>(ITypeDefinition<T> definition, HashEntry[] hash) where T : new();
    }
}
