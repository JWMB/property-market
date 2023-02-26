using Parsers.Models;
using Parsers;

namespace Crawling
{
    public interface ICrawlStateRepository
    {
        IEnumerable<ProviderSeachCrawlState> SearchCrawlStates { get; }
    }

    public class InMemoryCrawlStateRepository : ICrawlStateRepository
    {
        public List<ProviderSeachCrawlState> States { get; } = new();
        public IEnumerable<ProviderSeachCrawlState> SearchCrawlStates => States;
    }

    public class ProviderSeachCrawlState
    {
        public IPropertyDataProvider? Provider { get; set; }
        public PropertyFilter? Filter { get; set; }
        public DateTimeOffset? LastCrawl { get; set; }
        public DateTimeOffset? NextCrawl { get; set; }

        // TODO: use history to calculate how often new listings appear
        // Combine with numItems on search page to calculate when next crawl should occur
        // Take hour of day + weekday into account?
    }

}
