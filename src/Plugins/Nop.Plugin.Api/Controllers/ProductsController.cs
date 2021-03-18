using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.Images;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTO.ProductWarehouseMappings;
using Nop.Plugin.Api.DTOs.ProductWarehousesMappings;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.ProductsParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductApiService _productApiService;
        private readonly IProductService _productService;

        private readonly IUrlRecordService _urlRecordService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IFactory<Product> _factory;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IDTOHelper _dtoHelper;
        private readonly ILogger _logger;
        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreContext _storeContext;
        private readonly IOrderReportService _orderReportService;

        public ProductsController(IProductApiService productApiService,
                                  CatalogSettings catalogSettings,
                                  IOrderReportService orderReportService,
                                  IStoreContext storeContext,
                                  IJsonFieldsSerializer jsonFieldsSerializer,
                                  IProductService productService,
                                  IUrlRecordService urlRecordService,
                                  ICustomerActivityService customerActivityService,
                                  ILocalizationService localizationService,
                                  IFactory<Product> factory,
                                  IAclService aclService,
                                  IStoreMappingService storeMappingService,
                                  IStoreService storeService,
                                  ICustomerService customerService,
                                  IDiscountService discountService,
                                  IPictureService pictureService,
                                  IManufacturerService manufacturerService,
                                  IProductTagService productTagService,
                                  IProductAttributeService productAttributeService,
                                  ILogger logger,
                                  IDTOHelper dtoHelper) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _productApiService = productApiService;
            _factory = factory;
            _catalogSettings = catalogSettings;
            _storeContext = storeContext;
            _orderReportService = orderReportService;
            _manufacturerService = manufacturerService;
            _productTagService = productTagService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _logger = logger;
            _dtoHelper = dtoHelper;
        }

        /// <summary>
        /// Receive a list of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetProductsAsync(ProductsParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            var allProducts = _productApiService.GetProducts(parameters.Ids, parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                        parameters.UpdatedAtMax, parameters.Limit, parameters.Page, parameters.SinceId, parameters.CategoryId,
                                                                        parameters.VendorName, parameters.PublishedStatus, parameters.ShowOnHomePage)
                                                .WhereAwait(async p => await StoreMappingService.AuthorizeAsync(p));

            var productsAsDtos = new List<ProductDto>();
            await foreach (var product in allProducts)
            {
                var productDto = new ProductDto();
                productDto = await _dtoHelper.PrepareProductDTOAsync(product);
                productsAsDtos.Add(productDto);
            }


            var productsRootObject = new ProductsRootObjectDto()
            {
                Products = productsAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(productsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Receive a list of best selling products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/bestselling")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetBestSellingProductsAsync(ProductsParametersModel parameters)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                return new RawJsonActionResult("{\"products\":[]}");
            //var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            //load report
            var report = (await _orderReportService.BestSellersReportAsync(
                      storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                      pageSize: _catalogSettings.NumberOfBestsellersOnHomepage))
                  .ToList();

            parameters.Ids = report.Select(x => x.ProductId).ToList();
            return await GetProductsAsync(parameters);
        }
        /// checkhere *****

        /// <summary>
        /// Receive a list of new products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>

        [HttpGet]
        [Route("/api/products/new")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetNewProductsAsync(ProductsParametersModel parameters)
        {
            var products = (await _productService.GetProductsMarkedAsNewAsync());

            var productsAsDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                var productDto = await _dtoHelper.PrepareProductDTOAsync(product);
                productsAsDtos.Add(productDto);
            }


            var productsRootObject = new ProductsRootObjectDto()
            {
                Products = productsAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(productsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);

        }


        /// <summary>
        /// Receive a list of new products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>

        [HttpGet]
        [Route("/api/products/search")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> SearchProductsAsync(SearchProductsParametersModel parameters)
        {
            //var @enum = (ProductSortingEnum)parameters.OrderBy;

            //var allProducts =(await  _productService.SearchProductsAsync(
            //    keywords: parameters.Term,
            //    orderBy: @enum)).ToList();

            //IList<ProductDto> productsAsDtos = allProducts.Select(product => _dtoHelper.PrepareProductDTOAsync(product)).ToList();

            //var productsRootObject = new ProductsRootObjectDto()
            //{
            //    Products = productsAsDtos
            //};

            //var json = JsonFieldsSerializer.Serialize(productsRootObject, parameters.Fields);

            //return new RawJsonActionResult(json);
            return new RawJsonActionResult("{}");
        }


        /// <summary>
        /// Receive a count of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/count")]
        [ProducesResponseType(typeof(ProductsCountRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetProductsCount(ProductsCountParametersModel parameters)
        {
            var allProductsCount = _productApiService.GetProductsCountAsync(parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                       parameters.UpdatedAtMax, parameters.PublishedStatus, parameters.VendorName,
                                                                       parameters.CategoryId);

            var productsCountRootObject = new ProductsCountRootObject()
            {
                Count = allProductsCount
            };

            return Ok(productsCountRootObject);
        }

        /// <summary>
        /// Retrieve product by spcified id
        /// </summary>
        /// <param name="id">Id of the product</param>
        /// <param name="fields">Fields from the product you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetProductByIdAsync(int id, string fields = "")
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var product = _productApiService.GetProductById(id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, fields);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/products/sku/{sku}")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetProductBySkuAsync(string sku, string fields = "")
        {
            if (sku == "")
            {
                return Error(HttpStatusCode.BadRequest, "sku", "invalid sku");
            }

            var product = _productApiService.GetProductBySku(sku);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, fields);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/products")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<IActionResult> CreateProductAsync([ModelBinder(typeof(JsonModelBinder<ProductDto>))] Delta<ProductDto> productDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            await CustomerActivityService.InsertActivityAsync("APIService", "Starting Product Create", null);

            // Inserting the new product
            var product = await _factory.InitializeAsync();
            productDelta.Merge(product);

            await _productService.InsertProductAsync(product);

            await UpdateProductPicturesAsync(product, productDelta.Dto.Images);

            await UpdateProductTagsAsync(product, productDelta.Dto.Tags);

            await UpdateProductManufacturers(product, productDelta.Dto.ManufacturerIds);

            await UpdateAssociatedProductsAsync(product, productDelta.Dto.AssociatedProductIds);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(product, productDelta.Dto.SeName, product.Name, true);

            await _urlRecordService.SaveSlugAsync(product, seName, 0);

            await UpdateAclRolesAsync(product, productDelta.Dto.RoleIds);

            await UpdateDiscountMappingsAsync(product, productDelta.Dto.DiscountIds);

            await UpdateStoreMappingsAsync(product, productDelta.Dto.StoreIds);

            await _productService.UpdateProductAsync(product);
            await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product);

            // Preparing the result dto of the new product
            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateProductAsync([ModelBinder(typeof(JsonModelBinder<ProductDto>))] Delta<ProductDto> productDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }
            await CustomerActivityService.InsertActivityAsync("APIService", "Starting Product Update", null);

            var product = _productApiService.GetProductById(productDelta.Dto.Id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            productDelta.Merge(product);

            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.UpdateProductAsync(product);

            await UpdateProductAttributesAsync(product, productDelta);

            await UpdateProductPicturesAsync(product, productDelta.Dto.Images);

            await UpdateProductTagsAsync(product, productDelta.Dto.Tags);

            await UpdateProductManufacturers(product, productDelta.Dto.ManufacturerIds);

            await UpdateAssociatedProductsAsync(product, productDelta.Dto.AssociatedProductIds);

            // Update the SeName if specified
            if (productDelta.Dto.SeName != null)
            {
                var seName = await _urlRecordService.ValidateSeNameAsync(product, productDelta.Dto.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, seName, 0);
            }

            await UpdateDiscountMappingsAsync(product, productDelta.Dto.DiscountIds);

            await UpdateStoreMappingsAsync(product, productDelta.Dto.StoreIds);

            await UpdateAclRolesAsync(product, productDelta.Dto.RoleIds);



            await _productService.UpdateProductAsync(product);

            await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.UpdateProduct"), product);

            // Preparing the result dto of the new product
            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);


        }

        [HttpDelete]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var product = _productApiService.GetProductById(id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            await _productService.DeleteProductAsync(product);

            //activity log
            await CustomerActivityService.InsertActivityAsync("APIService", string.Format(await LocalizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

            return new RawJsonActionResult("{}");
        }
        [HttpPost]
        [Route("/api/products/warehouse/{id}")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> InsertProductWarehousesAsync(ProductWarehouselist productWarehouseInventories)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }
            await CustomerActivityService.InsertActivityAsync("APIService", "Starting Product Update", null);

            var product = _productApiService.GetProductById(productWarehouseInventories.Id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            if (product.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return Error(HttpStatusCode.BadRequest, "product warehouse", "ManageInventoryMethod");

            if (!product.UseMultipleWarehouses)
            {
                //    var message = $"{LocalizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} { await LocalizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit")}";
                return Error(HttpStatusCode.BadRequest, "product warehouse", "message");
            }


            foreach (var productWarehouseInventory in productWarehouseInventories.ProductWarehouseInventory)
            {
                await _productService.InsertProductWarehouseInventoryAsync(
                         new ProductWarehouseInventory
                         {
                             ProductId = productWarehouseInventories.Id,
                             WarehouseId = productWarehouseInventory.WarehouseId,
                             StockQuantity = productWarehouseInventory.StockQuantity,
                             ReservedQuantity = productWarehouseInventory.ReservedQuantity
                         });
            }
            //_productService.AddStockQuantityHistoryEntry(entityToUpdate, pwi.StockQuantity, pwi.StockQuantity,
            //     pwi.WarehouseId, message);
            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);


        }
        [HttpPut]
        [Route("/api/products/warehouse/{id}/{disablebuybutton}/{unpublish}")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> UpdateProductWarehousesAsync(ProductWarehouselist productWarehouseInventories, int id, bool disablebuybutton, bool unpublish)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }
            await CustomerActivityService.InsertActivityAsync("APIService", "Starting Product Update", null);

            var product = _productApiService.GetProductById(productWarehouseInventories.Id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            if (product.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return Error(HttpStatusCode.BadRequest, "product warehouse", "ManageInventoryMethod");

            if (!product.UseMultipleWarehouses)
            {
                var message = $"{await LocalizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} {await LocalizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit")}";
                return Error(HttpStatusCode.BadRequest, "product warehouse", "message");
            }


            foreach (var productWarehouseInventory in productWarehouseInventories.ProductWarehouseInventory)
            {

                //quantity change history message
                var message = $"{await LocalizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} {await LocalizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit")}";

                var existingPwI = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id)).FirstOrDefault(x => x.WarehouseId == productWarehouseInventory.WarehouseId);
                if (existingPwI != null)
                {
                    var previousStockQuantity = existingPwI.StockQuantity;

                    //update existing record
                    existingPwI.StockQuantity = productWarehouseInventory.StockQuantity;
                    existingPwI.ReservedQuantity = productWarehouseInventory.ReservedQuantity;
                    await _productService.UpdateProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity - previousStockQuantity, existingPwI.StockQuantity,
                         existingPwI.WarehouseId, message);

                }
                else
                {

                    //no need to insert a record for qty 0
                    existingPwI = new ProductWarehouseInventory
                    {
                        WarehouseId = productWarehouseInventory.WarehouseId,
                        ProductId = product.Id,
                        StockQuantity = productWarehouseInventory.StockQuantity,
                        ReservedQuantity = productWarehouseInventory.ReservedQuantity
                    };

                    await _productService.InsertProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity, existingPwI.StockQuantity,
                         existingPwI.WarehouseId, message);
                }
            }

            if (disablebuybutton != product.DisableBuyButton)
            {
                product.UpdatedOnUtc = DateTime.UtcNow;
                product.DisableBuyButton = disablebuybutton;
                if (disablebuybutton && unpublish)
                    product.Published = false;
                else if (!disablebuybutton && !product.Published )
                    product.Published = true;
                await _productService.UpdateProductAsync(product);
            }

            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);

        }
        private async Task UpdateProductPicturesAsync(Product entityToUpdate, List<ImageMappingDto> setPictures)
        {
            // If no pictures are specified means we don't have to update anything
            if (setPictures == null)
            {
                return;
            }

            // delete unused product pictures
            var productPictures = await _productService.GetProductPicturesByProductIdAsync(entityToUpdate.Id);
            var unusedProductPictures = productPictures.Where(x => setPictures.All(y => y.Id != x.Id)).ToList();
            foreach (var unusedProductPicture in unusedProductPictures)
            {
                var picture = await PictureService.GetPictureByIdAsync(unusedProductPicture.PictureId);
                if (picture == null)
                {
                    throw new ArgumentException("No picture found with the specified id");
                }
                await PictureService.DeletePictureAsync(picture);
            }

            foreach (var imageDto in setPictures)
            {
                if (imageDto.Id > 0)
                {
                    // update existing product picture
                    var productPictureToUpdate = productPictures.FirstOrDefault(x => x.Id == imageDto.Id);
                    if (productPictureToUpdate != null && imageDto.Position > 0)
                    {
                        productPictureToUpdate.DisplayOrder = imageDto.Position;
                        await _productService.UpdateProductPictureAsync(productPictureToUpdate);
                    }
                }
                else
                {
                    // add new product picture
                    var newPicture = await PictureService.InsertPictureAsync(imageDto.Binary, imageDto.MimeType, string.Empty);
                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        PictureId = newPicture.Id,
                        ProductId = entityToUpdate.Id,
                        DisplayOrder = imageDto.Position
                    });
                }
            }
        }

        private async Task UpdateProductAttributesAsync(Product entityToUpdate, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute mappings are specified means we don't have to update anything
            if (productDtoDelta.Dto.ProductAttributeMappings == null)
            {
                return;
            }

            // delete unused product attribute mappings
            var toBeUpdatedIds = productDtoDelta.Dto.ProductAttributeMappings.Where(y => y.Id != 0).Select(x => x.Id);
            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(entityToUpdate.Id);
            var unusedProductAttributeMappings = productAttributeMappings.Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeMapping in unusedProductAttributeMappings)
            {
                await _productAttributeService.DeleteProductAttributeMappingAsync(unusedProductAttributeMapping);
            }

            foreach (var productAttributeMappingDto in productDtoDelta.Dto.ProductAttributeMappings)
            {
                if (productAttributeMappingDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeMappingToUpdate = productAttributeMappings.FirstOrDefault(x => x.Id == productAttributeMappingDto.Id);
                    if (productAttributeMappingToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeMappingDto, productAttributeMappingToUpdate, false);

                        await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMappingToUpdate);

                        await UpdateProductAttributeValuesAsync(productAttributeMappingDto, productDtoDelta);
                    }
                }
                else
                {
                    var newProductAttributeMapping = new ProductAttributeMapping
                    {
                        ProductId = entityToUpdate.Id
                    };

                    productDtoDelta.Merge(productAttributeMappingDto, newProductAttributeMapping);

                    // add new product attribute
                    await _productAttributeService.InsertProductAttributeMappingAsync(newProductAttributeMapping);
                }
            }
        }

        private async Task UpdateProductAttributeValuesAsync(ProductAttributeMappingDto productAttributeMappingDto, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute values are specified means we don't have to update anything
            if (productAttributeMappingDto.ProductAttributeValues == null)
                return;

            // delete unused product attribute values
            var toBeUpdatedIds = productAttributeMappingDto.ProductAttributeValues.Where(y => y.Id != 0).Select(x => x.Id);

            var unusedProductAttributeValues =
               (await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMappingDto.Id)).Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeValue in unusedProductAttributeValues)
            {
                await _productAttributeService.DeleteProductAttributeValueAsync(unusedProductAttributeValue);
            }

            foreach (var productAttributeValueDto in productAttributeMappingDto.ProductAttributeValues)
            {
                if (productAttributeValueDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeValueToUpdate =
                       await _productAttributeService.GetProductAttributeValueByIdAsync(productAttributeValueDto.Id);
                    if (productAttributeValueToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeValueDto, productAttributeValueToUpdate, false);

                        await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValueToUpdate);
                    }
                }
                else
                {
                    var newProductAttributeValue = new ProductAttributeValue();
                    productDtoDelta.Merge(productAttributeValueDto, newProductAttributeValue);

                    newProductAttributeValue.ProductAttributeMappingId = productAttributeMappingDto.Id;
                    // add new product attribute value
                    await _productAttributeService.InsertProductAttributeValueAsync(newProductAttributeValue);
                }
            }
        }

        private async Task UpdateProductTagsAsync(Product product, IReadOnlyCollection<string> productTags)
        {
            if (productTags == null)
            {
                return;
            }

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var existingProductTags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);
            var productTagsToRemove = new List<ProductTag>();
            foreach (var existingProductTag in existingProductTags)
            {
                var found = false;
                foreach (var newProductTag in productTags)
                {
                    if (!existingProductTag.Name.Equals(newProductTag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    found = true;
                    break;
                }

                if (!found)
                {
                    productTagsToRemove.Add(existingProductTag);
                }
            }

            try
            {
                // checkhere ******
                await _productTagService.UpdateProductTagsAsync(product, productTagsToRemove.Select(o => o.Name).ToArray());

                foreach (var productTagName in productTags)
                {
                    ProductTag productTag;

                    var productTag2 = await _productTagService.GetProductTagByIdAsync(1); // await _productTagService.GetAllProductTagsAsync(productTagName);
                    if (productTag2 == null)
                    {
                        //add new product tag
                        productTag = new ProductTag
                        {
                            Name = productTagName
                        };
                        // _productTagService.InsertProductProductTagMappingAsync(productTag);
                    }
                    else
                    {
                        productTag = productTag2;
                    }

                    var seName = await _urlRecordService.ValidateSeNameAsync(productTag, string.Empty, productTag.Name, true);
                    await _urlRecordService.SaveSlugAsync(productTag, seName, 0);

                    //Perform a final check to deal with duplicates etc.
                    var currentProductTags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);
                    if (!currentProductTags.Any(o => o.Id == productTag.Id))
                    {
                        await _productTagService.InsertProductProductTagMappingAsync(new ProductProductTagMapping()
                        {
                            ProductId = product.Id,
                            ProductTagId = productTag.Id
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task UpdateDiscountMappingsAsync(Product product, List<int> passedDiscountIds)
        {
            if (passedDiscountIds == null)
            {
                return;
            }

            var allDiscounts = await DiscountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true);
            var appliedProductDiscount = await DiscountService.GetAppliedDiscountsAsync(product);
            foreach (var discount in allDiscounts)
            {
                if (passedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (!appliedProductDiscount.Any(d => d.Id == discount.Id))
                    {
                        appliedProductDiscount.Add(discount);
                    }
                }
                else
                {
                    //remove discount
                    if (appliedProductDiscount.Any(d => d.Id == discount.Id))
                    {
                        appliedProductDiscount.Remove(discount);
                    }
                }
            }

            await _productService.UpdateProductAsync(product);
            await _productService.UpdateHasDiscountsAppliedAsync(product);
        }

        private async Task UpdateProductManufacturers(Product product, List<int> passedManufacturerIds)
        {
            // If no manufacturers specified then there is nothing to map 
            if (passedManufacturerIds == null)
            {
                return;
            }
            var productmanufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
            var unusedProductManufacturers = productmanufacturers.Where(x => !passedManufacturerIds.Contains(x.Id)).ToList();

            // remove all manufacturers that are not passed
            foreach (var unusedProductManufacturer in unusedProductManufacturers)
            {
                //_manufacturerService.DeleteProductManufacturer(unusedProductManufacturer);
            }

            foreach (var passedManufacturerId in passedManufacturerIds)
            {
                // not part of existing manufacturers so we will create a new one
                if (productmanufacturers.All(x => x.Id != passedManufacturerId))
                {
                    // if manufacturer does not exist we simply ignore it, otherwise add it to the product
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(passedManufacturerId);
                    if (manufacturer != null)
                    {
                        await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                        {
                            ProductId = product.Id,
                            ManufacturerId = manufacturer.Id
                        });
                    }
                }
            }
        }
        private async Task UpdateAssociatedProductsAsync(Product product, List<int> passedAssociatedProductIds)
        {
            // If no associated products specified then there is nothing to map 
            if (passedAssociatedProductIds == null)
                return;

            var noLongerAssociatedProducts = (await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true))
                    .Where(p => !passedAssociatedProductIds.Contains(p.Id));

            // update all products that are no longer associated with our product
            foreach (var noLongerAssocuatedProduct in noLongerAssociatedProducts)
            {
                noLongerAssocuatedProduct.ParentGroupedProductId = 0;
                await _productService.UpdateProductAsync(noLongerAssocuatedProduct);
            }

            var newAssociatedProducts = await _productService.GetProductsByIdsAsync(passedAssociatedProductIds.ToArray());
            foreach (var newAssociatedProduct in newAssociatedProducts)
            {
                newAssociatedProduct.ParentGroupedProductId = product.Id;
                await _productService.UpdateProductAsync(newAssociatedProduct);
            }
        }
    }
}