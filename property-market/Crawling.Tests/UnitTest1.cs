using FakeItEasy;
using Parsers;
using Parsers.Models;
using Parsers.Providers;
using Shouldly;

namespace Crawling.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Crawler_Test()
        {
            var searchProvider = A.Fake<IPropertyListingSearchProvider>();
            var listing = CreatePropertyListing(searchProvider.DataProvider);
    
            A.CallTo(() => searchProvider.FetchSearchListings(A.Fake<PropertyFilter>(), 0, 100))
                .Returns(Task.FromResult(new FetchResult { Content = "aaa", Source = A.Fake<Uri>() }));
            A.CallTo(() => searchProvider.ParseSearchListings(A<Uri>._, A<string>._)).Returns(new List<PropertyListing> { listing });

            var provider = A.Fake<IPropertyDataProvider>();
            A.CallTo(() => provider.SearchProvider).Returns(searchProvider);

            var providers = new[] { provider };

            var stateRepo = new InMemoryCrawlStateRepository();
            var queue = new InMemoryCrawlQueue();
            stateRepo.States.Add(new ProviderSeachCrawlState { Provider = providers[0], LastCrawl = DateTimeOffset.MinValue, NextCrawl = DateTimeOffset.UtcNow.AddDays(-1) });

            var crawler = new Crawler(A.Fake<IRawDataRepository>(), queue, stateRepo, providers, new InMemoryListingsRepository());

            await crawler.PerformDueSearches();

            (await queue.ApproximateMessageCount).ShouldBe(1);

            var item = await queue.Receive();

            item.ShouldBe(listing);

            await queue.Delete(listing);
        }

        private PropertyListing CreatePropertyListing(IPropertyDataProvider provider)
        {
            var result = A.Fake<PropertyListing>();
            result.Provider = provider;

            result.ListingId = "asdasd"; // A.Fake<string>();
            return result;
        }
    }
}