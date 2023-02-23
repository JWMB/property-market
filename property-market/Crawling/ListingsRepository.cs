using Parsers.Models;

namespace Crawling
{
    public class ListingsRepository
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
            var dict = GetForProvider(listing.Provider.DataProvider.Id);
            dict[listing.ListingId] = listing;
            return Task.CompletedTask;
        }
    }
}
