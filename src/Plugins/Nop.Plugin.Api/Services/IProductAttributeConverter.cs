using Nop.Plugin.Api.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Services
{
    public interface IProductAttributeConverter
    {
        List<ProductItemAttributeDto> Parse(string attributesXml);
        Task<string> ConvertToXmlAsync(List<ProductItemAttributeDto> attributeDtos, int productId);
    }
}
