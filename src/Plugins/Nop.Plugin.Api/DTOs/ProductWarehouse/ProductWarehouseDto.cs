using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTOs.ProductWarehousesMappings;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.DTO.ProductWarehouseMappings
{
    [JsonObject(Title = "warehouse_inventory")]
    public class ProductWarehouseDto : BaseDto
    {

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonProperty("stock_quantity")]
        public int StockQuantity { get; set; }

        [JsonProperty("reserved_quantity")]
        public int ReservedQuantity { get; set; }

   

    }

    public class ProductWarehouseBase 
    {
        [Key]
        public int WarehouseId { get; set; }
        
        public int StockQuantity { get; set; }

        public int ReservedQuantity { get; set; }

    }
    public class  ProductWarehouselist
    {
        [Key]
        public int Id { get; set; }
        
        public ICollection<ProductWarehouseBase> ProductWarehouseInventory { get; set; }
    }

}
