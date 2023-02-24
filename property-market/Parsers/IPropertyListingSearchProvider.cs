using Parsers.Models;

namespace Parsers
{
    public interface IPropertyListingSearchProvider
    {
        Uri DefaultUri { get; }
        IPropertyDataProvider DataProvider { get; }

        // TODO: FilteringCapabilities?
        Task<FetchResult> FetchSearchListings(PropertyFilter? filter = null, int skip = 0, int take = 100);

        List<PropertyListing> ParseSearchListings(Uri source, string fetched);
    }
}
