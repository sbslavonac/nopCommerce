using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Api.DTOs.ProductWarehousesMappings
{
    [JsonObject(Title = "warehouse_inventory")]
    public class ProductWarehouseInventoryDto : BaseDto
    {
        private List<ProductWarehouseInventoryDto> _ProductWarehouseInventoryValues;

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonProperty("stock_quantity")]
        public int StockQuantity { get; set; }

        [JsonProperty("reserved_quantity")]
        public int ReservedQuantity { get; set; }

        [JsonProperty("warehouse_inventory_values")]
        public List<ProductWarehouseInventoryDto> ProductWarehouseInventoryValues
        {
            get { return _ProductWarehouseInventoryValues; }
            set { _ProductWarehouseInventoryValues = value; }
        }
    }
}
