using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.ProductWarehousesMappings;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class ProductWarehouseMappingDtoMappings
    {
        public static ProductWarehouseInventoryDto ToDto(this ProductWarehouseInventory mapping)
        {
            return mapping.MapTo<ProductWarehouseInventory, ProductWarehouseInventoryDto>();
        }
    }
}
