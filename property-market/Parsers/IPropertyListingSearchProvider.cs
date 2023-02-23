using Parsers.Models;

namespace Parsers
{
    public interface IPropertyListingSearchProvider
    {
        Uri DefaultUri { get; }
        IPropertyDataProvider DataProvider { get; }

        // TODO: FilteringCapabilities?
        Task<FetchResult> FetchPropertySearchResults(PropertyFilter? filter = null, int skip = 0, int take = 100);

        List<PropertyListing> ParseSearchResults(Uri source, string fetched);

        public class FetchResult
        {
            public required Uri Source { get; set; }
            public string Content { get; set; }
        }
    }
}
