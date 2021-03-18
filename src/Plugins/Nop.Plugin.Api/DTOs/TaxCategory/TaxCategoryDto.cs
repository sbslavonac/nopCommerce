using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;


namespace Nop.Plugin.Api.DTO.TaxCategory
{
    [JsonObject(Title = "taxcategory")]
    //[Validator(typeof(TaxCategoryDtoValidator))]
    public class TaxCategoryDto : BaseDto
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        /// 
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        /// 
        [JsonProperty("display_order")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the tax rate
        /// </summary>
        /// 
        [JsonProperty("rate")]
        public decimal Rate { get; set; }
    }
}
