using Parsers.Models;
using Parsers;

namespace Crawling
{
    public interface ICrawlStateRepository
    {
        Task<IEnumerable<ProviderSeachCrawlState>> GetSearchCrawlStates();
        Task<ProviderSeachCrawlState?> GetSearchCrawlState(IPropertyDataProvider provider);
    }

    public class InMemoryCrawlStateRepository : ICrawlStateRepository
    {
        public List<ProviderSeachCrawlState> States { get; } = new();

        public Task<ProviderSeachCrawlState?> GetSearchCrawlState(IPropertyDataProvider provider) =>
            Task.FromResult(States.FirstOrDefault(o => o.Provider == provider));

        public Task<IEnumerable<ProviderSeachCrawlState>> GetSearchCrawlStates() =>
            Task.FromResult((IEnumerable<ProviderSeachCrawlState>)States);
    }

    public class ProviderSeachCrawlState
    {
        public required IPropertyDataProvider Provider { get; set; }
        public PropertyFilter? Filter { get; set; }
        public DateTimeOffset? LastCrawl { get; set; }
        public DateTimeOffset? NextCrawl { get; set; }

        // TODO: use history to calculate how often new listings appear
        // Combine with numItems on search page to calculate when next crawl should occur
        // Take hour of day + weekday into account?
    }

}
