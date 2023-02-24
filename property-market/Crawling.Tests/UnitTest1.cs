using Parsers;
using Parsers.Providers;

namespace Crawling.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Crawler_Test()
        {
            var crawler = new Crawler(new InMemoryListingsRepository());

            var providers = new[]
            {
                new Booli()
            };

            await crawler.CrawlLists();

            //await crawler.CrawlItem();
        }
    }
}