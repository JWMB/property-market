using Parsers.Models;

namespace Parsers.Providers
{
    public class Template : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        public Uri DefaultUri => new Uri("https://something");

        public IPropertyDataProvider DataProvider => this;

        public string Id => nameof(Template);

        public bool IsAggregator => false;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public /*async*/ Task<IPropertyListingProvider.FetchResult> FetchPropertyListingResult(string objectId)
        {
            throw new NotImplementedException();
        }

        public /*async*/ Task<IPropertyListingSearchProvider.FetchResult> FetchPropertySearchResults(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            throw new NotImplementedException();
        }

        public List<PropertyListing> ParseSearchResults(Uri source, string fetched)
        {
            throw new NotImplementedException();
        }
    }
}
