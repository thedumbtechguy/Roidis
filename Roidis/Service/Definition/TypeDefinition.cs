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
    public class TypeDefinition<T> : ITypeDefinition<T>
    {

        public string Name { get; private set; }
        public Member PrimaryKey { get; private set; }
        public TypeAccessor Accessor { get; private set; }

        private readonly List<Member> _allfields = new List<Member>();
        public List<Member> AllFields => _allfields;

        private readonly List<Member> _indexedfields = new List<Member>();
        public List<Member> IndexedFields => _indexedfields;
        
        private readonly List<Member> _requiredfields = new List<Member>();
        public List<Member> RequiredFields => _requiredfields;

        private readonly List<Member> _ignoreOnCreateFields = new List<Member>();
        public List<Member> IgnoreOnCreateFields => _ignoreOnCreateFields;

        private readonly List<Member> _ignoreOnUpdateFields = new List<Member>();
        public List<Member> IgnoreOnUpdateFields => _ignoreOnUpdateFields;

        internal readonly Dictionary<string, string> MemberToStorageNameMap = new Dictionary<string, string>();
        internal readonly Dictionary<string, string> MemberToIndexNameMap = new Dictionary<string, string>();
        private readonly IValueConverter _valueConverter;

        public TypeDefinition(string keyPrefix, TypeAccessor accessor, IValueConverter valueConverter)
        {
            Name = keyPrefix;
            Accessor = accessor;
            _valueConverter = valueConverter;
        }

        public string GetHashName(Member member)
        {
            return MemberToStorageNameMap[member.Name];
        }

        public string GetIndexName(Member member)
        {
            return MemberToIndexNameMap[member.Name];
        }

        public void SetPrimaryKey(Member primaryKey)
        {
            if (!_valueConverter.IsPrimaryKeyTypeSupported(primaryKey.Type))
                throw new UnsupportedKeyTypeException(primaryKey.Type);

            PrimaryKey = primaryKey;
        }
    }
}
