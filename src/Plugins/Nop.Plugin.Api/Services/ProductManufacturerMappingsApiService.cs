﻿using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Services
{
    public class ProductManufacturerMappingsApiService : IProductManufacturerMappingsApiService
    {
        private readonly IRepository<ProductManufacturer> _productManufacturerMappingsRepository;

        public ProductManufacturerMappingsApiService(IRepository<ProductManufacturer> productManufacturerMappingsRepository)
        {
            _productManufacturerMappingsRepository = productManufacturerMappingsRepository;
        }

        public IList<ProductManufacturer> GetMappings(int? productId = null, 
            int? manufacturerId = null, int limit = Configurations.DefaultLimit, 
            int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId)
        {
            var query = GetMappingsQuery(productId, manufacturerId, sinceId);

            return new ApiList<ProductManufacturer>(query, page - 1, limit);
        }

        public int GetMappingsCount(int? productId = null, int? manufacturerId = null)
        {
            return GetMappingsQuery(productId, manufacturerId).Count();
        }

        public async Task<ProductManufacturer> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _productManufacturerMappingsRepository.GetByIdAsync(id);
        }

        private IQueryable<ProductManufacturer> GetMappingsQuery(int? productId = null, 
            int? manufacturerId = null, int sinceId = Configurations.DefaultSinceId)
        {
            var query = _productManufacturerMappingsRepository.Table;

            if (productId != null)
            {
                query = query.Where(mapping => mapping.ProductId == productId);
            }

            if (manufacturerId != null)
            {
                query = query.Where(mapping => mapping.ManufacturerId == manufacturerId);
            }

            if (sinceId > 0)
            {
                query = query.Where(mapping => mapping.Id > sinceId);
            }

            query = query.OrderBy(mapping => mapping.Id);

            return query;
        }
    }
}