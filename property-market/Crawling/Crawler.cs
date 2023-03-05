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
                if (existing.Equals(parsed) == false)
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

        public async Task PerformSearch(IPropertyListingSearchProvider searchProvider, PropertyFilter? filter = null) //ProviderSeachCrawlState state)
        {
            //if (state.Provider?.SearchProvider == null)
            //    return;

            var result = await searchProvider.FetchSearchListings(filter);

            await rawDataRepository.Add(searchProvider.DataProvider, result.Content);

            var listings = searchProvider.ParseSearchListings(result.Source, result.Content);
            var addedOrUpdated = await GetAddedOrUpdated(listings);

            var state = await crawlStateRepository.GetOrCreateSearchCrawlState(searchProvider.DataProvider);
            state.LastCrawl = DateTimeOffset.UtcNow;
            // TODO: next crawl should be determined by heuristics (how often page seems to contain new info - and depending on time of day, day of week etc)
            state.NextCrawl = DateTimeOffset.UtcNow.Add(addedOrUpdated.Any() ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(5));

            await crawlStateRepository.SetSearchCrawlState(searchProvider.DataProvider, state);

            if (addedOrUpdated.Any())
            {
                foreach (var item in addedOrUpdated)
                {
                    await queueSender.Send(item);
                }
            }
        }

        public async Task PerformDueSearches()
        {
            var states = await crawlStateRepository.GetSearchCrawlStates();

            var toCrawl = states.Where(o => o.NextCrawl <= DateTimeOffset.UtcNow)
                .Select(o => new { o.Provider.SearchProvider, o.Filter })
                .Concat(providers.Except(states.Select(o => o.Provider))
                    .Select(o => new { o.SearchProvider, Filter = (PropertyFilter?)null }));

            foreach (var item in toCrawl.Where(o => o.SearchProvider != null))
                await PerformSearch(item.SearchProvider!, item.Filter);
        }

        public async Task QueueOpenListings()
        {
            var openListings = await listingsRepository.GetOpenListings();

            // TODO: with a simple queue, we could get unnecessary duplicates
            // Use a table instead? Or should this logic/re-adding be done just after the listing has been scanned?
            // Or have a marker on the item - DateTimeOffset AddedToQueue - that we can check?

            // Scan those that have bids quite often (every hour?)
            foreach (var item in openListings.Where(o => o.Bids.Any()))
                ;
            // Scan those that don't have bids once a day
            //foreach (var item in openListings.Where(o => o.Bids.Any()))
        }

        private async Task<IEnumerable<PropertyListing>> GetAddedOrUpdated(IEnumerable<PropertyListing> listings)
        {
            var result = new List<PropertyListing>();
            foreach (var item in listings)
            {
                var existing = await listingsRepository.Get(item.Provider.Id, item.ListingId);
                if (existing != null)
                {
                    if (!existing.Equals(item))
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
