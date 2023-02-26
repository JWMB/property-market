using Parsers;
using Parsers.Models;

namespace Crawling
{

    public class Crawler
    {
        private List<IPropertyDataProvider> providers;
        private readonly ICrawlQueueSender queueSender;
        private readonly ICrawlStateRepository crawlStateRepository;
        private IListingsRepository listingsRepository;
        private IRawDataRepository rawDataRepository;

        public Crawler(IRawDataRepository rawDataRepository, ICrawlQueueSender queueSender, ICrawlStateRepository crawlStateRepository, IEnumerable<IPropertyDataProvider> dataProviders, IListingsRepository listingsRepository)
        {
            providers = dataProviders.ToList();
            this.rawDataRepository = rawDataRepository;
            this.queueSender = queueSender;
            this.crawlStateRepository = crawlStateRepository;
            this.listingsRepository = listingsRepository;
        }

        public async Task CrawlItem(PropertyListing listing)
        {
            var provider = providers.SingleOrDefault(o => o.Id == listing.Provider.Id);
            if (provider == null)
                throw new Exception($"No provider found: {listing.Provider.Id}");
            if (provider.ListingProvider == null)
                throw new Exception($"No itemProvider in {listing.Provider.Id}");

            await CrawlItem(provider.ListingProvider, listing.ListingId);
        }

        public async Task CrawlItem(IPropertyListingProvider provider, string listingId)
        {
            var result = await provider.FetchListing(listingId);
            var parsed = provider.ParseListing(result.Source, result.Content);
            var existing = await listingsRepository.Get(provider.DataProvider.Id, listingId);

            if (existing != null)
            {
                if (existing.Compare(parsed) == false)
                {
                    await listingsRepository.Upsert(parsed);
                    if (provider.DataProvider.IsAggregator)
                    {
                        await queueSender.Send(parsed);
                    }
                }
            }
            else
            {
                await listingsRepository.Upsert(parsed);
            }
        }

        public async Task CrawlLists()
        {
            var states = crawlStateRepository.SearchCrawlStates;

            var toCrawl = states.Where(o => o.NextCrawl <= DateTimeOffset.UtcNow);

            foreach (var item in toCrawl)
            {
                if (item.Provider?.SearchProvider == null)
                    continue;
                var listProvider = item.Provider.SearchProvider;

                var result = await listProvider.FetchSearchListings(item.Filter);

                item.LastCrawl = DateTimeOffset.UtcNow;

                await rawDataRepository.Add(item.Provider, result.Content);

                var listings = listProvider.ParseSearchListings(result.Source, result.Content);
                var addedOrUpdated = await GetAddedOrUpdated(listings);

                // TODO: next crawl should be determined by heuristics (how often page seems to contain new info - and depending on time of day, day of week etc)
                item.NextCrawl = DateTimeOffset.UtcNow.Add(addedOrUpdated.Any() ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(5));
                if (addedOrUpdated.Any())
                {
                    foreach (var xx in addedOrUpdated)
                    {
                        await queueSender.Send(xx);
                    }
                }
            }
        }

        private async Task<IEnumerable<PropertyListing>> GetAddedOrUpdated(IEnumerable<PropertyListing> listings)
        {
            var result = new List<PropertyListing>();
            foreach (var item in listings)
            {
                var existing = await listingsRepository.Get(item.Provider.Id, item.ListingId);
                if (existing != null)
                {
                    if (!existing.Compare(item))
                    {
                        await listingsRepository.Upsert(item);
                        result.Add(item);
                    }
                }
                else
                {
                    result.Add(item);
                }
            }
            return result;
        }
    }
}
