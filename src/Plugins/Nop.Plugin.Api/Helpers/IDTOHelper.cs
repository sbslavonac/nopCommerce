using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.DTO.Images;
using Nop.Plugin.Api.DTO.Languages;
using Nop.Plugin.Api.DTO.Manufacturers;
using Nop.Plugin.Api.DTO.OrderItems;
using Nop.Plugin.Api.DTO.Orders;
using Nop.Plugin.Api.DTO.ProductAttributes;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.DTO.SpecificationAttributes;
using Nop.Plugin.Api.DTO.Stores;
using Nop.Plugin.Api.DTO.TaxCategory;
using Nop.Plugin.Api.DTOs.ProductReview;
using Nop.Plugin.Api.DTOs.ProductWarehousesMappings;

namespace Nop.Plugin.Api.Helpers
{
    public interface IDTOHelper
    {
        Task<CustomerDto> PrepareCustomerDTOAsync(Customer customer);
        Task<ProductDto> PrepareProductDTOAsync(Product product);
        Task<CategoryDto> PrepareCategoryDTOAsync(Category category);
        Task<OrderDto> PrepareOrderDTOAsync(Order order);
        
        Task<ShoppingCartItemDto> PrepareShoppingCartItemDTOAsync(ShoppingCartItem shoppingCartItem);
        Task<OrderItemDto> PrepareOrderItemDTOAsync(OrderItem orderItem);
        Task<StoreDto> PrepareStoreDTOAsync(Store store);
        Task<LanguageDto> PrepareLanguageDtoAsync(Language language);
        ProductAttributeDto PrepareProductAttributeDTO(ProductAttribute productAttribute);
        ProductSpecificationAttributeDto PrepareProductSpecificationAttributeDto(ProductSpecificationAttribute productSpecificationAttribute);
        SpecificationAttributeDto PrepareSpecificationAttributeDto(SpecificationAttribute specificationAttribute);
        Task<ManufacturerDto> PrepareManufacturerDtoAsync(Manufacturer manufacturer);
        Task<TaxCategoryDto> PrepareTaxCategoryDTOAsync(TaxCategory taxCategory);
        Task<ImageMappingDto> PrepareProductPictureDTOAsync(ProductPicture productPicture);
        Task<ProductReviewDto> PrepareProductReviewDTOAsync(ProductReview productReview);
    }
}

