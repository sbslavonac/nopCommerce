using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Maps
{
    public interface IJsonPropertyMapper
    {
        Dictionary<string, Tuple<string, Type>> GetMap(Type type);
        Task<Dictionary<string, Tuple<string, Type>>> GetMapAsync(Type type);
    }
}