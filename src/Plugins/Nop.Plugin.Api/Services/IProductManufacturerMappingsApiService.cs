using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Services
{
    public interface IProductManufacturerMappingsApiService
    {
        IList<ProductManufacturer> GetMappings(int? productId = null, int? manufacturerId = null, int limit = Configurations.DefaultLimit,
            int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId);

        int GetMappingsCount(int? productId = null, int? manufacturerId = null);

        Task<ProductManufacturer> GetByIdAsync(int id);
    }
}