using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
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
using Nop.Plugin.Api.DTO.ProductWarehouseMappings;
using Nop.Plugin.Api.DTO.Shipments;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.DTO.SpecificationAttributes;
using Nop.Plugin.Api.DTO.Stores;
using Nop.Plugin.Api.DTO.TaxCategory;
using Nop.Plugin.Api.DTOs.ProductReview;
using Nop.Plugin.Api.DTOs.ProductWarehousesMappings;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Helpers
{
    public class DTOHelper : IDTOHelper
    {
        private readonly CurrencySettings _currencySettings;
        private readonly IAclService _aclService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerApiService _customerApiService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ISettingService _settingService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IAddressService _addressService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly MediaSettings _mediaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IReviewTypeService _reviewTypeService;

        public DTOHelper(IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            ICustomerApiService customerApiService,
            ICustomerService customerService,
            IProductAttributeConverter productAttributeConverter,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IDiscountService discountService,
            IManufacturerService manufacturerService,
            CurrencySettings currencySettings,
            IStoreService storeService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IProductTagService productTagService,
            ITaxCategoryService taxCategoryService,
            ISettingService settingService,
            IShipmentService shipmentService,
            IOrderService orderService,
            IAddressService addressService,
            ISpecificationAttributeService specificationAttributeService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            CustomerSettings customerSettings,
            IReviewTypeService reviewTypeService)
        {
            _specificationAttributeService = specificationAttributeService;
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _customerApiService = customerApiService;
            _customerService = customerService;
            _productAttributeConverter = productAttributeConverter;
            _languageService = languageService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _storeService = storeService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _productTagService = productTagService;
            _taxCategoryService = taxCategoryService;
            _settingService = settingService;
            _discountService = discountService;
            _manufacturerService = manufacturerService;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _mediaSettings = mediaSettings;
            _customerSettings = customerSettings;
            _reviewTypeService = reviewTypeService;
        }

        public async Task<ProductDto> PrepareProductDTOAsync(Product product)
        {
            var productDto = product.ToDto();

            var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
            await PrepareProductImagesAsync(productPictures, productDto);

            var x =
            productDto.SeName = await _urlRecordService.GetSeNameAsync(product);
            productDto.DiscountIds = (await _discountService.GetAppliedDiscountsAsync(product)).Select(discount => discount.Id).ToList();


            productDto.ManufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).Select(pm => pm.Id).ToList();
            productDto.RoleIds = (await _aclService.GetAclRecordsAsync(product)).Select(acl => acl.CustomerRoleId).ToList();
            productDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(product)).Select(mapping => mapping.StoreId)
                .ToList();
            productDto.Tags = (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).Select(tag => tag.Name)
                .ToList();

            var productWarehouseInventory = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id)).ToList();


            productDto.AssociatedProductIds = await GetRelatedProductIdsByProductIdAsync(product.Id);

            // load product attributes
            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);

            await PrepareProductAttributesAsync(productAttributeMappings, productDto);


            // load product specification attributes
            var productSpecificationAttributeMappings = await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId: product.Id);

            PrepareProductSpecificationAttributes(productSpecificationAttributeMappings, productDto);



            PrepareWarehaousInventory(productWarehouseInventory, productDto);

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            productDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = await _localizationService.GetLocalizedAsync(product, x => x.Name, language.Id)
                };

                productDto.LocalizedNames.Add(localizedNameDto);
            }

            return productDto;
        }

        private async System.Threading.Tasks.Task<List<int>> GetRelatedProductIdsByProductIdAsync(int productId)
        {
            //load and cache report
            var productIds = (await _productService.GetRelatedProductsByProductId1Async(productId)).Select(x => x.ProductId2).ToArray();

            //load products
            var products = await _productService.GetProductsByIdsAsync(productIds);
            //ACL and store mapping

            products = await (await _productService.GetProductsByIdsAsync(productIds)).WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();

            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            //visible individually
            products = products.Where(p => p.VisibleIndividually).ToList();

            return products.Select(p => p.Id).ToList();
        }

        public async Task<ImageMappingDto> PrepareProductPictureDTOAsync(ProductPicture productPicture)
        {
            return await PrepareProductImageDtoAsync(productPicture);
        }
        protected async Task<ImageMappingDto> PrepareProductImageDtoAsync(ProductPicture productPicture)
        {
            ImageMappingDto imageMapping = null;

            var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId).ConfigureAwait(true);

            if (productPicture != null)
            {

                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                imageMapping = new ImageMappingDto
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Id = productPicture.Id,
                    ProductId = productPicture.ProductId,
                    PictureId = productPicture.PictureId,
                    Position = productPicture.DisplayOrder,
                    MimeType = picture.MimeType,
                    Src = await _pictureService.GetPictureUrlAsync(productPicture.PictureId)
                };
            }

            return imageMapping;
        }
        public async Task<CategoryDto> PrepareCategoryDTOAsync(Category category)
        {
            var categoryDto = category.ToDto();

            var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
            var imageDto = await PrepareImageDtoAsync(picture);

            if (imageDto != null)
            {
                categoryDto.Image = imageDto;
            }

            categoryDto.SeName = await _urlRecordService.GetSeNameAsync(category);
            categoryDto.DiscountIds = (await _discountService.GetAppliedDiscountsAsync(category)).Select(discount => discount.Id).ToList();
            categoryDto.RoleIds = (await _aclService.GetAclRecordsAsync(category)).Select(acl => acl.CustomerRoleId).ToList();
            categoryDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(category)).Select(mapping => mapping.StoreId).ToList();

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            categoryDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = await _localizationService.GetLocalizedAsync(category, x => x.Name, language.Id)
                };

                categoryDto.LocalizedNames.Add(localizedNameDto);
            }

            return categoryDto;
        }

        public async Task<OrderDto> PrepareOrderDTOAsync(Order order)
        {
            try
            {
                var orderDto = order.ToDto();

                // orderDto.OrderItems = (await _orderService.GetOrderItemsAsync(order.Id)).Select(await PrepareOrderItemDTOAsync).ToList();

                orderDto.Shipments = (await _shipmentService.GetShipmentsByOrderIdAsync(order.Id)).Select(PrepareShippingItemDTO).ToList();

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                orderDto.BillingAddress = billingAddress.ToDto();

                if (order.ShippingAddressId.HasValue)
                {
                    var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                    orderDto.ShippingAddress = shippingAddress.ToDto();
                }

                var customerDto = await _customerApiService.GetCustomerByIdAsync(order.CustomerId);

                if (customerDto != null)
                {
                    orderDto.Customer = customerDto.ToOrderCustomerDto();
                }

                return orderDto;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<ShoppingCartItemDto> PrepareShoppingCartItemDTOAsync(ShoppingCartItem shoppingCartItem)
        {
            var dto = shoppingCartItem.ToDto();
            dto.ProductDto = await PrepareProductDTOAsync(await _productService.GetProductByIdAsync(shoppingCartItem.ProductId));

            dto.CustomerDto = (await _customerService.GetCustomerByIdAsync(shoppingCartItem.CustomerId)).ToCustomerForShoppingCartItemDto();
            dto.Attributes = _productAttributeConverter.Parse(shoppingCartItem.AttributesXml);
            return dto;
        }

        public ShipmentDto PrepareShippingItemDTO(Shipment shipment)
        {
            return new ShipmentDto()
            {
                AdminComment = shipment.AdminComment,
                CreatedOnUtc = shipment.CreatedOnUtc,
                DeliveryDateUtc = shipment.DeliveryDateUtc,
                Id = shipment.Id,
                OrderId = shipment.OrderId,
                ShippedDateUtc = shipment.ShippedDateUtc,
                TotalWeight = shipment.TotalWeight,
                TrackingNumber = shipment.TrackingNumber
            };

        }
        public async Task<OrderItemDto> PrepareOrderItemDTOAsync(OrderItem orderItem)
        {
            var dto = orderItem.ToDto();
            var productoprepare = await _productService.GetProductByIdAsync(orderItem.ProductId);
            dto.Product = await PrepareProductDTOAsync(productoprepare);
            dto.Attributes = _productAttributeConverter.Parse(orderItem.AttributesXml);
            return dto;
        }

        public async Task<StoreDto> PrepareStoreDTOAsync(Store store)
        {
            var storeDto = store.ToDto();

            var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            if (!string.IsNullOrEmpty(primaryCurrency.DisplayLocale))
            {
                storeDto.PrimaryCurrencyDisplayLocale = primaryCurrency.DisplayLocale;
            }

            storeDto.LanguageIds = (await _languageService.GetAllLanguagesAsync(false, store.Id)).Select(x => x.Id).ToList();

            return storeDto;
        }

        public async Task<LanguageDto> PrepareLanguageDtoAsync(Language language)
        {
            var languageDto = language.ToDto();

            languageDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(language)).Select(mapping => mapping.StoreId)
                .ToList();

            if (languageDto.StoreIds.Count == 0)
            {
                languageDto.StoreIds = (await _storeService.GetAllStoresAsync()).Select(s => s.Id).ToList();
            }

            return languageDto;
        }

        public ProductAttributeDto PrepareProductAttributeDTO(ProductAttribute productAttribute)
        {
            return productAttribute.ToDto();
        }

        private async Task PrepareProductImagesAsync(IEnumerable<ProductPicture> productPictures, ProductDto productDto)
        {
            if (productDto.Images == null)
            {
                productDto.Images = new List<ImageMappingDto>();
            }

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                var imageDto = await PrepareImageDtoAsync(await _pictureService.GetPictureByIdAsync(productPicture.PictureId));

                if (imageDto != null)
                {
                    var productImageDto = new ImageMappingDto
                    {
                        Id = productPicture.Id,
                        PictureId = productPicture.PictureId,
                        Position = productPicture.DisplayOrder,
                        Src = imageDto.Src,
                        Attachment = imageDto.Attachment
                    };

                    productDto.Images.Add(productImageDto);
                }
            }
        }

        protected async Task<ImageDto> PrepareImageDtoAsync(Picture picture)
        {
            ImageDto image = null;

            if (picture != null)
            {
                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                image = new ImageDto
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Src = await _pictureService.GetPictureUrlAsync(picture.Id)
                };
            }

            return image;
        }
        private async Task PrepareProductAttributesAsync(IEnumerable<ProductAttributeMapping> productAttributeMappings,
            ProductDto productDto)
        {
            if (productDto.ProductAttributeMappings == null)
            {
                productDto.ProductAttributeMappings = new List<ProductAttributeMappingDto>();
            }

            foreach (var productAttributeMapping in productAttributeMappings)
            {
                var productAttributeMappingDto =
                  await PrepareProductAttributeMappingDtoAsync(productAttributeMapping);

                if (productAttributeMappingDto != null)
                {
                    productDto.ProductAttributeMappings.Add(productAttributeMappingDto);
                }
            }
        }


        public void PrepareWarehaousInventory(IEnumerable<ProductWarehouseInventory> productWarehouseInventories, ProductDto productDto)
        {
            if (productDto.ProductWarehouseInventory == null)
                productDto.ProductWarehouseInventory = new List<ProductWarehouseDto>();
            foreach (var productWarehouseInventorie in productWarehouseInventories)
            {
                var productWarehouseInventorieDto = new ProductWarehouseDto
                {
                    Id = productWarehouseInventorie.Id,
                    ProductId = productWarehouseInventorie.ProductId,
                    ReservedQuantity = productWarehouseInventorie.ReservedQuantity,
                    StockQuantity = productWarehouseInventorie.StockQuantity,
                    WarehouseId = productWarehouseInventorie.WarehouseId
                };
                productDto.ProductWarehouseInventory.Add(productWarehouseInventorieDto);
            }
        }

        private async Task<ProductAttributeMappingDto> PrepareProductAttributeMappingDtoAsync(
             ProductAttributeMapping productAttributeMapping)
        {
            ProductAttributeMappingDto productAttributeMappingDto = null;

            if (productAttributeMapping != null)
            {
                productAttributeMappingDto = new ProductAttributeMappingDto
                {
                    Id = productAttributeMapping.Id,
                    ProductAttributeId = productAttributeMapping.ProductAttributeId,
                    ProductAttributeName = (await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId)).Name,
                    TextPrompt = productAttributeMapping.TextPrompt,
                    DefaultValue = productAttributeMapping.DefaultValue,
                    AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = productAttributeMapping.DisplayOrder,
                    IsRequired = productAttributeMapping.IsRequired,
                    //TODO: Somnath
                    ProductAttributeValues = (await _productAttributeService
                                    .GetProductAttributeValuesAsync(productAttributeMapping.Id))
                                    .Select(x => x.ToDto())
                                    .ToList()
                };
            }

            return productAttributeMappingDto;
        }

        public async Task<CustomerDto> PrepareCustomerDTOAsync(Customer customer)
        {
            var result = customer.ToDto();

            /// customer roles
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
            foreach (var item in customerRoles)
            {
                result.RoleIds.Add(item.Id);
            }

            return result;
        }

        private void PrepareProductAttributeCombinations(IEnumerable<ProductAttributeCombination> productAttributeCombinations,
            ProductDto productDto)
        {
            productDto.ProductAttributeCombinations = productDto.ProductAttributeCombinations ?? new List<ProductAttributeCombinationDto>();

            foreach (var productAttributeCombination in productAttributeCombinations)
            {
                var productAttributeCombinationDto = PrepareProductAttributeCombinationDto(productAttributeCombination);
                if (productAttributeCombinationDto != null)
                {
                    productDto.ProductAttributeCombinations.Add(productAttributeCombinationDto);
                }
            }
        }

        private ProductAttributeCombinationDto PrepareProductAttributeCombinationDto(ProductAttributeCombination productAttributeCombination)
        {
            return productAttributeCombination.ToDto();
        }

        public void PrepareProductSpecificationAttributes(IEnumerable<ProductSpecificationAttribute> productSpecificationAttributes, ProductDto productDto)
        {
            if (productDto.ProductSpecificationAttributes == null)
                productDto.ProductSpecificationAttributes = new List<ProductSpecificationAttributeDto>();

            foreach (var productSpecificationAttribute in productSpecificationAttributes)
            {
                var productSpecificationAttributeDto = PrepareProductSpecificationAttributeDto(productSpecificationAttribute);

                if (productSpecificationAttributeDto != null)
                {
                    productDto.ProductSpecificationAttributes.Add(productSpecificationAttributeDto);
                }
            }
        }


        public ProductSpecificationAttributeDto PrepareProductSpecificationAttributeDto(ProductSpecificationAttribute productSpecificationAttribute)
        {
            return productSpecificationAttribute.ToDto();
        }

        public SpecificationAttributeDto PrepareSpecificationAttributeDto(SpecificationAttribute specificationAttribute)
        {
            return specificationAttribute.ToDto();
        }

        public async Task<ManufacturerDto> PrepareManufacturerDtoAsync(Manufacturer manufacturer)
        {
            var manufacturerDto = manufacturer.ToDto();

            var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
            var imageDto = await PrepareImageDtoAsync(picture);

            if (imageDto != null)
            {
                manufacturerDto.Image = imageDto;
            }

            manufacturerDto.SeName = await _urlRecordService.GetSeNameAsync(manufacturer);
            manufacturerDto.DiscountIds = (await _discountService.GetAppliedDiscountsAsync(manufacturer)).Select(discount => discount.Id).ToList();
            manufacturerDto.RoleIds = (await _aclService.GetAclRecordsAsync(manufacturer)).Select(acl => acl.CustomerRoleId).ToList();
            manufacturerDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(manufacturer)).Select(mapping => mapping.StoreId)
                .ToList();

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            manufacturerDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name, language.Id)
                };

                manufacturerDto.LocalizedNames.Add(localizedNameDto);
            }

            return manufacturerDto;
        }


        public async Task<TaxCategoryDto> PrepareTaxCategoryDTOAsync(TaxCategory taxCategory)
        {
            var taxRateModel = new TaxCategoryDto()
            {
                Id = taxCategory.Id,
                Name = taxCategory.Name,
                DisplayOrder = taxCategory.DisplayOrder,
                Rate = await _settingService.GetSettingByKeyAsync<decimal>(string.Format(Configurations.FixedRateSettingsKey, taxCategory.Id))
            };

            return taxRateModel;
        }

        public async Task<ProductReviewDto> PrepareProductReviewDTOAsync(ProductReview productReview)
        {

            var productReviewModel = productReview.ToDto();


            // load the customer who wrote a comment
            var customer = await _customerService.GetCustomerByIdAsync(productReview.CustomerId);

            productReviewModel.CustomerName = await _customerService.GetCustomerFullNameAsync(customer);

            productReviewModel.CustomerAvatarUrl = await _pictureService.GetPictureUrlAsync(
                       (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute)),
                        _mediaSettings.AvatarPictureSize, _customerSettings.DefaultAvatarEnabled, defaultPictureType: PictureType.Avatar);


            // load a review type mappings 

            productReviewModel.ReviewTypeMappingsDto = (await _reviewTypeService
                .GetProductReviewReviewTypeMappingsByProductReviewIdAsync(productReview.Id))
                .Select(rtm => rtm.ToDto())
                .ToList();

            return productReviewModel;
        }
    }
}



//protected ImageMappingDto PrepareProductImageDto(ProductPicture productPicture)
//{
//    ImageMappingDto imageMapping = null;

//    if (productPicture != null)
//    {
//        // We don't use the image from the passed dto directly 
//        // because the picture may be passed with src and the result should only include the base64 format.
//        imageMapping = new ImageMappingDto
//        {
//            //Attachment = Convert.ToBase64String(picture.PictureBinary),
//            Id = productPicture.Id,
//            ProductId = productPicture.ProductId,
//            PictureId = productPicture.PictureId,
//            Position = productPicture.DisplayOrder,
//            MimeType = productPicture.Picture.MimeType,
//            Src = _pictureService.GetPictureUrl(productPicture.Picture)
//        };
//    }

//    return imageMapping;
//}