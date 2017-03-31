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
    public interface ITypeDefinitionFactory
    {
        ITypeDefinition<T> Create<T>(TypeAccessor accessor);
    }
}
