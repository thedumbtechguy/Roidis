using FastMember;
using Roidis.Attribute;
using Roidis.Exception;
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
    public interface ITypeDefinition<T>
    {
        /// <summary>
        /// Used as a key prefix for the object in redis
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The property identified as the primary key
        /// </summary>
        Member PrimaryKey { get; }

        /// <summary>
        /// All supported members in the class
        /// </summary>
        List<Member> AllFields { get; }

        /// <summary>
        /// All members to be indexed
        /// </summary>
        List<Member> IndexedFields { get; }

        /// <summary>
        /// All required fields
        /// </summary>
        List<Member> RequiredFields { get; }

        /// <summary>
        /// Fields to ignore when creating a new record
        /// </summary>
        List<Member> IgnoreOnCreateFields { get; }

        /// <summary>
        /// Fields to ignore when updating an existing record
        /// </summary>
        List<Member> IgnoreOnUpdateFields { get; }

        /// <summary>
        /// An object that can be used to set and retrieve values of a given instance
        /// </summary>
        TypeAccessor Accessor { get; }

        /// <summary>
        /// Get the name used to store the given member in a hash
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        string GetHashName(Member member);

        /// <summary>
        /// Get the name used to create the index key for this member 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        string GetIndexName(Member member);
    }
}
