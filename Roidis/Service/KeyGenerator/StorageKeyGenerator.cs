﻿using FastMember;
using Roidis.Service.Definition;
using StackExchange.Redis;

namespace Roidis.Service.KeyGenerator
{
    public class StorageKeyGenerator : IStorageKeyGenerator
    {
        public RedisKey GetKey<T>(ITypeDefinition<T> definition, RedisValue id)
        {
            return $"{definition.Name}:{id}";
        }

        public RedisKey GetClusteredIndexKey<T>(ITypeDefinition<T> definition)
        {
            return $"#:#roidis:#system:#index:#clustered:{definition.Name}";
        }

        public RedisKey GetFieldIndexKey<T>(ITypeDefinition<T> definition, Member member, RedisValue value)
        {
            var indexName = definition.GetIndexNameFromMember(member);
            return $"#:#roidis:#user:#index:#field:{definition.Name}:{indexName}:{value}";
        }

        public RedisKey GetSerialKey<T>(ITypeDefinition<T> definition)
        {
            return $"#:#roidis:#system:#serial:{definition.Name}";
        }
    }
}