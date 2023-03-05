using Parsers.Models;

namespace Crawling
{
    public interface IListingsRepository
    {
        Task<PropertyListing?> Get(string providerId, string itemId);
        Task Upsert(PropertyListing listing);
        /// <summary>
        /// Listings that are still open (i.e. not yet sold)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PropertyListing>> GetOpenListings();
    }

    public class InMemoryListingsRepository : IListingsRepository
    {
        private Dictionary<string, Dictionary<string, PropertyListing>> retrieved = new();

        private Dictionary<string, PropertyListing> GetForProvider(string providerId)
        {
            if (!retrieved.ContainsKey(providerId))
                retrieved.Add(providerId, new Dictionary<string, PropertyListing>());
            return retrieved[providerId];
        }

        public Task<PropertyListing?> Get(string providerId, string itemId)
        {
            var dict = GetForProvider(providerId);
            return Task.FromResult(dict.TryGetValue(itemId, out var result) ? result : null);
        }

        public Task Upsert(PropertyListing listing)
        {
            var dict = GetForProvider(listing.Provider.Id);
            dict[listing.ListingId] = listing;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<PropertyListing>> GetOpenListings()
        {
            return Task.FromResult(retrieved.SelectMany(o => o.Value.Values).Where(o => o.Removed == null));
        }
    }
}
