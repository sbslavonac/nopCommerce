using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.ProductWarehouseMappings
{
    public class ProductWarehouseMappingsRootObject : ISerializableObject
    {
        public ProductWarehouseMappingsRootObject()
        {
            ProductWarehouseMappingDtos = new List<ProductWarehouseDto>();
        }

        [JsonProperty("warehouse_inventory")]
        public IList<ProductWarehouseDto> ProductWarehouseMappingDtos { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "warehouse_inventory";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ProductWarehouseDto);
        }
    }
}
