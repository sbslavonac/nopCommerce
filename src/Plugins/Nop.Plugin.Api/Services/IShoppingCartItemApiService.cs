using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Services
{
    public interface IShoppingCartItemApiService
    {
        Task<List<ShoppingCartItem>> GetShoppingCartItemsAsync(int? customerId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
                                                    DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, int limit = Configurations.DefaultLimit,
                                                    int page = Configurations.DefaultPageValue);

        Task<ShoppingCartItem> GetShoppingCartItemAsync(int id);
    }
}