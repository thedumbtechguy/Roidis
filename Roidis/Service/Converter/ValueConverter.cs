using Roidis.Exception;
using Roidis.Service.Definition;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Roidis.Service.Converter
{
    // todo: keep a static list of types for reuse
    public class ValueConverter : IValueConverter
    {
        private static Tuple<RedisValue, bool> UNDEFINED = Tuple.Create(RedisValue.Null, false);

        private static Type[] SupportedPrimaryKeyTypes = new[] {
            typeof(int),
            typeof(string),
            typeof(long),
            typeof(byte[]),
            typeof(Guid)
        };

        private static Type[] SupportedMemberTypes = new[] {
            typeof(string),
            typeof(decimal),
            typeof(decimal?),
            typeof(byte[]),
            typeof(Guid),
            typeof(Guid?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
        };

        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);



        public RedisValue FromObject(object sourceValue, Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));
            else if(!IsMemberTypeSupported(sourceType))
                throw new UnsupportedMemberTypeException(sourceType);

            if (sourceValue == null)
                return RedisValue.EmptyString;
            
            #region special cases

            if (sourceType == typeof(Guid) || sourceType == typeof(Guid?))
                return sourceValue.ToString();


            if (sourceType == typeof(DateTime) || sourceType == typeof(DateTime?))
                return (((DateTime)sourceValue).ToUniversalTime() - Epoch).TotalMilliseconds;


            if (sourceType == typeof(DateTimeOffset) || sourceType == typeof(DateTimeOffset?))
                return ((DateTimeOffset)sourceValue).ToUnixTimeMilliseconds();


            if (sourceType.GetTypeInfo().IsEnum)
                return Convert.ToString(sourceValue);

            #endregion

            #region boxed and primitives

            if (sourceType == typeof(int) || sourceType == typeof(int?))
                return (int)sourceValue;

            if (sourceType == typeof(long) || sourceType == typeof(long?))
                return Convert.ToString((long)sourceValue);

            if (sourceType == typeof(double) || sourceType == typeof(double?))
                return Convert.ToString((double)sourceValue);

            if (sourceType == typeof(decimal) || sourceType == typeof(decimal?))
                return Convert.ToString((decimal)sourceValue);

            if (sourceType == typeof(bool) || sourceType == typeof(bool?))
                return (bool)sourceValue;

            if (sourceType == typeof(byte[]))
                return (byte[])sourceValue;

            if (sourceType == typeof(string))
                return (string)sourceValue;

            #endregion

            return (RedisValue) sourceValue;
        }

        public object ToObject(RedisValue source, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            #region special cases

            if (targetType == typeof(Guid))
                return !source.HasValue 
                    ? Guid.Empty 
                    : Guid.Parse(source);

            if (targetType == typeof(Guid?))
                return !source.HasValue 
                    ? (Guid?)null 
                    : Guid.Parse(source);


            if (targetType == typeof(DateTime))
                return !source.HasValue 
                    ? DateTime.Now 
                    : Epoch.AddMilliseconds((double)source);


            if (targetType == typeof(DateTime?))
                return !source.HasValue 
                    ? (DateTime?)null 
                    : Epoch.AddMilliseconds((double)source);


            if (targetType == typeof(DateTimeOffset))
                return !source.HasValue
                    ? DateTimeOffset.Now
                    : DateTimeOffset.FromUnixTimeMilliseconds((long)source);


            if (targetType == typeof(DateTimeOffset?))
                return !source.HasValue
                    ? (DateTimeOffset?)null
                    : DateTimeOffset.FromUnixTimeMilliseconds((long)source);


            if (targetType.GetTypeInfo().IsEnum)
                return !source.HasValue
                    ? Activator.CreateInstance(targetType)
                    : Enum.Parse(targetType, source);

            #endregion

            #region nullables

            if (targetType.GetTypeInfo().IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (!source.HasValue) return null;

                targetType = Nullable.GetUnderlyingType(targetType);
            }

            #endregion
            
            // this should handle all other stuff
            var newValue = Convert.ChangeType(source, targetType);

            // if it is still a RedisValue, the conversion failed
            if (newValue is RedisValue)
                throw new ConversionFailedException(source, targetType);            

            return newValue;
        }

        public Tuple<RedisValue, bool> ResolveId<T>(ITypeDefinition<T> definition, T instance)
        {
            var primaryKey = definition.PrimaryKey;

            var idType = definition.PrimaryKey.Type;

            if (!IsPrimaryKeyTypeSupported(primaryKey.Type))
            {
                throw new UnsupportedKeyTypeException(definition.PrimaryKey.Type);
            }
            else
            {
                var id = definition.Accessor[instance, primaryKey.Name];

                if (idType == typeof(int))
                    return Convert.ToInt32(id) == 0
                        ? UNDEFINED
                        : Tuple.Create((RedisValue)(int)id, true);

                else if (idType == typeof(long))
                    return Convert.ToInt64(id) == 0
                        ? UNDEFINED
                        : Tuple.Create((RedisValue)Convert.ToString(id), true);

                else if (idType == typeof(byte[]))
                    return id == null || ((byte[])id).Length == 0
                        ? UNDEFINED
                        : Tuple.Create((RedisValue)(byte[])id, true);

                else if (idType == typeof(Guid))
                    return (Guid)id == Guid.Empty
                        ? Tuple.Create((RedisValue)Guid.NewGuid().ToString(), false)
                        : Tuple.Create((RedisValue)(string)id, true);

                return string.IsNullOrWhiteSpace((string)id)
                    ? UNDEFINED
                    : Tuple.Create((RedisValue)(string)id, true);
            }
        }

        


        public bool IsPrimaryKeyTypeSupported(Type type)
        {
            return SupportedPrimaryKeyTypes.Contains(type);
        }

        public bool IsMemberTypeSupported(Type type)
        {
            return type.GetTypeInfo().IsPrimitive
                || type.GetTypeInfo().IsEnum
                || SupportedMemberTypes.Contains(type)
            ;
        }
    }
}
