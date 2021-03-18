using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Services
{
    public class NewsLetterSubscriptionApiService : INewsLetterSubscriptionApiService
    {
        private readonly IRepository<NewsLetterSubscription> _newsLetterSubscriptionRepository;
        private readonly IStoreContext _storeContext;

        public NewsLetterSubscriptionApiService(IRepository<NewsLetterSubscription> newsLetterSubscriptionRepository, IStoreContext storeContext)
        {
            _newsLetterSubscriptionRepository = newsLetterSubscriptionRepository;
            _storeContext = storeContext;
        }

        public async Task<List<NewsLetterSubscription>> GetNewsLetterSubscriptionsAsync(DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId,
            bool? onlyActive = true)
        {
            var query = await GetNewsLetterSubscriptionsQueryAsync(createdAtMin, createdAtMax, onlyActive);

            if (sinceId > 0)
            {
                query = query.Where(c => c.Id > sinceId);
            }

            return new ApiList<NewsLetterSubscription>(query, page - 1, limit);
        }

        private async Task<IQueryable<NewsLetterSubscription>> GetNewsLetterSubscriptionsQueryAsync(DateTime? createdAtMin = null, DateTime? createdAtMax = null, bool? onlyActive = true)
        {
            var id = (await _storeContext.GetCurrentStoreAsync()).Id;
            var query = _newsLetterSubscriptionRepository.Table.Where(nls => nls.StoreId == (id));

            if (onlyActive != null && onlyActive == true)
            {
                query = query.Where(nls => nls.Active == onlyActive);
            }

            if (createdAtMin != null)
            {
                query = query.Where(c => c.CreatedOnUtc > createdAtMin.Value);
            }

            if (createdAtMax != null)
            {

                query = query.Where(c => c.CreatedOnUtc < createdAtMax.Value);
            }

            query = query.OrderBy(nls => nls.Id);

            return query;
        }
    }
}
