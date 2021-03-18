using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models
{
    public record ClientApiModel : BaseNopModel
    {

        public int Id { get; set; }
        public string ClientName { get; set; }

        public string ClientId
        {
            get;
            set;
        }

        public string ClientSecret
        {
            get;
            set;
        }

        public string RedirectUrl { get; set; }

        public int AccessTokenLifetime
        {
            get; set;
        }

        public int RefreshTokenLifetime
        {
            get; set;
        }

        public bool Enabled { get; set; }
    }

    public record ClientApiListModel : BasePagedListModel<ClientApiModel>
    {

    }

}