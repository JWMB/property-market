using Parsers;
using Parsers.Models;

namespace Crawling
{
    public class Crawler
    {
        private List<IPropertyDataProvider> providers = new List<IPropertyDataProvider>();
        private ListingsRepository repository = new();

        public async Task CrawlItem(PropertyListing listing)
        {
            var provider = providers.SingleOrDefault(o => o.Id == listing.Provider.DataProvider.Id);
            if (provider == null) 
                throw new Exception($"No provider found: {listing.Provider.DataProvider.Id}");
            if (provider.ListingProvider == null)
                throw new Exception($"No itemProvider in {listing.Provider.DataProvider.Id}");

            await CrawlItem(provider.ListingProvider, listing.ListingId);
        }

        public async Task CrawlItem(IPropertyListingProvider provider, string listingId)
        {
            var listing = await provider.FetchPropertyListingResult(listingId);
            var existing = await repository.Get(provider.DataProvider.Id, listingId);

            if (existing != null)
            {
                if (existing.Compare(listing.Listing) == false)
                {
                    await repository.Upsert(listing.Listing);
                    if (provider.DataProvider.IsAggregator)
                    {
                        await QueueItemCrawl(listing.Listing);
                    }
                }
            }
            else
            {
                await repository.Upsert(listing.Listing);
            }
        }

        public async Task CrawlLists()
        {
            var listProviders = new List<ListCrawlInfo>();

            while (true)
            {
                var toCrawl = listProviders.Where(o => o.NextCrawl >= DateTimeOffset.UtcNow);

                foreach (var item in toCrawl)
                {
                    if (item.Provider?.SearchProvider == null)
                        continue;
                    var listProvider = item.Provider.SearchProvider;

                    var result = await listProvider.FetchPropertySearchResults(item.Filter);

                    item.LastCrawl = DateTimeOffset.UtcNow;

                    await SaveRawResult(item, result.Content);

                    var listings = listProvider.ParseSearchResults(result.Source, result.Content);
                    var addedOrUpdated = await GetAddedOrUpdated(listings);

                    // TODO: next crawl should be determined by heuristics (how often page seems to contain new info - and depending on time of day, day of week etc)
                    item.NextCrawl = DateTimeOffset.UtcNow.Add(addedOrUpdated.Any() ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(5));
                    if (addedOrUpdated.Any())
                    {
                        foreach (var xx in addedOrUpdated)
                        {
                            await QueueItemCrawl(xx);
                        }
                    }
                }

                await Task.Delay(1000);
            }
        }

        private Task QueueItemCrawl(PropertyListing listing)
        {
            // Always crawl from realtor's page if it exists
            var uri = listing.RealtorListingPage ?? listing.UriWithProvider; //Provider.DataProvider.IsAggregator
            // TODO: also check if we have a working parser for that realtor!

            // TODO:
            return Task.CompletedTask;
        }

        private Task<IEnumerable<PropertyListing>> GetAddedOrUpdated(IEnumerable<PropertyListing> listings)
        {
            // TODO: !
            return Task.FromResult((IEnumerable<PropertyListing>)new List<PropertyListing>());
        }

        private Task SaveRawResult(ListCrawlInfo crawlInfo, string rawResult)
        {
            // TODO: !
            return Task.CompletedTask;
        }

        class ListCrawlInfo
        {
            public IPropertyDataProvider? Provider { get; set; }
            public PropertyFilter? Filter { get; set; }
            public DateTimeOffset? LastCrawl { get; set; }
            public DateTimeOffset? NextCrawl { get; set; }
        }
    }
}
