using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using Roidis.Service.Definition;
using FastMember;

namespace Roidis.Service.KeyGenerator
{
    public interface IStorageKeyGenerator
    {
        RedisKey GetKey<T>(ITypeDefinition<T> definition, RedisValue id);

        RedisKey GetClusteredIndexKey<T>(ITypeDefinition<T> definition);

        RedisKey GetFieldIndexKey<T>(ITypeDefinition<T> definition, Member member, RedisValue value);

        RedisKey GetSerialKey<T>(ITypeDefinition<T> definition);
    }
}
