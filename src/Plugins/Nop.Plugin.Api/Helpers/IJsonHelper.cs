using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Helpers
{
    public interface IJsonHelper
    {
        Task<Dictionary<string, object>> GetRequestJsonDictionaryFromStreamAsync(Stream stream, bool rewindStream);
        string GetRootPropertyName<T>() where T : class, new();
    }
}