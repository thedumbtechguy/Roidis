using FastMember;
using Roidis.Attribute;
using Roidis.Exception;
using Roidis.Service.Converter;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Service.Definition
{
    public class TypeDefinitionFactory : ITypeDefinitionFactory
    {
        private readonly IValueConverter valueConverter;

        public TypeDefinitionFactory(IValueConverter valueConverter)
        {
            this.valueConverter = valueConverter;
        }

        public ITypeDefinition<T> Create<T>(TypeAccessor accessor)
        {
            var idFromName = false;
            var type = typeof(T);        
            var definition = new TypeDefinition<T>(GetPrefix(type), accessor, valueConverter);

            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance|BindingFlags.Public);
            foreach (PropertyInfo prop in props)
            {
                // identify collections
                var isCollection = prop.PropertyType != typeof(byte[]) && prop.PropertyType != typeof(string) && prop.PropertyType.GetInterfaces().Contains(typeof(IEnumerable));
                
                var member = accessor.GetMembers().First(m => m.Name == prop.Name);
                var memberName = member.Name;

                if (isCollection || !valueConverter.IsMemberTypeSupported(member.Type)) continue;

                if (definition.PrimaryKey == null && prop.Name.ToLower() == "id")
                {
                    idFromName = true;
                    definition.SetPrimaryKey(member);
                }

                var attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    //if (!isCollection)
                    {
                        // find primary key
                        if (attr is RoidKeyAttribute)
                        {
                            if (definition.PrimaryKey == null || idFromName)
                            {
                                definition.SetPrimaryKey(member);
                                idFromName = false;
                            }
                            else if (definition.PrimaryKey != null)
                            {
                                throw new DuplicateRoidKeyAttributeException(type);
                            }
                        }

                        //find indexes
                        else if (attr is RoidIndexAttribute indexAttribute)
                        {
                            var indexName = GetIndexName(indexAttribute, member);

                            definition.IndexedFields.Add(member);
                            definition.MemberToIndexNameMap.Add(member.Name, indexName);
                        }

                        //find required fields
                        else if (attr is RoidRequiredAttribute requiredAttribute)
                        {
                            definition.RequiredFields.Add(member);
                        }

                        //find custom field definitions
                        else if (attr is RoidFieldNameAttribute nameAttribute)
                        {
                            memberName = nameAttribute.Name;
                        }

                        //find ignored field definitions
                        else if (attr is RoidIgnoreAttribute ignoreAttribute)
                        {
                            if (ignoreAttribute.OnCreate)
                                definition.IgnoreOnCreateFields.Add(member);

                            if (ignoreAttribute.OnUpdate)
                                definition.IgnoreOnUpdateFields.Add(member);
                        }
                    }
                }

                //if (!isCollection)
                definition.AllFields.Add(member);
                definition.MemberToStorageNameMap.Add(member.Name, memberName);
            }

            if (definition.PrimaryKey == null)
                throw new MissingRoidKeyException(type);

            return definition;
        }


        private static string GetIndexName(RoidIndexAttribute indexAttribute, Member member)
        {
            return indexAttribute.Name ?? member.Name;
        }

        private static string GetPrefix(Type type)
        {
            var prefix = type.GetTypeInfo().GetCustomAttribute<RoidPrefixAttribute>(false);
            return prefix?.Prefix ?? type.Name;
        }
    }
}
