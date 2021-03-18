using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Widgets.NivoSlider;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Api.DTOs.NivoSlider;
using System.Collections.Generic;
using System.Net;
using Nop.Plugin.Api.JSON.ActionResults;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Controllers
{
    public class NivoSliderController : BaseApiController
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        public NivoSliderController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IStoreContext storeContext,
            ISettingService settingService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _pictureService = pictureService;
        }


        /// <summary>
        /// Receive a list of all Sliders
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/sliders")]
        [ProducesResponseType(typeof(NivoSlidersRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetNivoSlidersAsync()
        {

            var nivoSliderSettings =await _settingService.LoadSettingAsync<NivoSliderSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            var slidesAsDtos =await GetNivoSliderAsync(nivoSliderSettings);

            var nivoSlidersRootObject = new NivoSlidersRootObject()
            {
                Slider = slidesAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(nivoSlidersRootObject, "");

            return new RawJsonActionResult(json);
        }


        private async Task<IList<NivoSliderDto>> GetNivoSliderAsync(NivoSliderSettings nivoSliderSettings)
        {
            var sliders = new List<NivoSliderDto>();

            var picture1Url =await _pictureService.GetPictureUrlAsync(nivoSliderSettings.Picture1Id, showDefaultPicture: false) ?? "";

            var picture2Url =await _pictureService.GetPictureUrlAsync(nivoSliderSettings.Picture2Id, showDefaultPicture: false) ?? "";

            var picture3Url =await _pictureService.GetPictureUrlAsync(nivoSliderSettings.Picture3Id, showDefaultPicture: false) ?? "";

            var picture4Url =await _pictureService.GetPictureUrlAsync(nivoSliderSettings.Picture4Id, showDefaultPicture: false) ?? "";

            var picture5Url =await _pictureService.GetPictureUrlAsync(nivoSliderSettings.Picture5Id, showDefaultPicture: false) ?? "";

            if (!string.IsNullOrEmpty(picture1Url))
            {
                sliders.Add(new NivoSliderDto()
                {
                    PictureUrl = picture1Url,
                    Text = nivoSliderSettings.Text1,
                    Link = nivoSliderSettings.Link1
                });
            }

            if (!string.IsNullOrEmpty(picture2Url))
            {
                sliders.Add(new NivoSliderDto()
                {
                    PictureUrl = picture2Url,
                    Text = nivoSliderSettings.Text2,
                    Link = nivoSliderSettings.Link2
                });
            }

            if (!string.IsNullOrEmpty(picture3Url))
            {
                sliders.Add(new NivoSliderDto()
                {
                    PictureUrl = picture3Url,
                    Text = nivoSliderSettings.Text3,
                    Link = nivoSliderSettings.Link3
                });
            }


            if (!string.IsNullOrEmpty(picture4Url))
            {
                sliders.Add(new NivoSliderDto()
                {
                    PictureUrl = picture4Url,
                    Text = nivoSliderSettings.Text4,
                    Link = nivoSliderSettings.Link4
                });
            }


            if (!string.IsNullOrEmpty(picture5Url))
            {
                sliders.Add(new NivoSliderDto()
                {
                    PictureUrl = picture5Url,
                    Text = nivoSliderSettings.Text5,
                    Link = nivoSliderSettings.Link5
                });
            }

            return sliders;
        }
    }
}
