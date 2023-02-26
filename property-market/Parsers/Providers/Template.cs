using Parsers.Models;

namespace Parsers.Providers
{
    public class Template : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        private readonly IDataFetcher dataFetcher;

        //    // https://www.notar.se/kopa-bostad?sortBy=date&sortOrder=desc

        public Uri DefaultUri => new Uri("https://www.notar.se");
        
        public IPropertyDataProvider DataProvider => this;

        public string Id => nameof(Template);

        public bool IsAggregator => false;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public Template(IDataFetcher dataFetcher)
        {
            this.dataFetcher = dataFetcher;
        }

        public /*async*/ Task<FetchResult> FetchListing(string objectId)
        {
            throw new NotImplementedException();
        }

        public /*async*/ Task<FetchResult> FetchSearchListings(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            throw new NotImplementedException();
        }

        public PropertyListing ParseListing(Uri source, string fetched)
        {
            throw new NotImplementedException();
        }

        public List<PropertyListing> ParseSearchListings(Uri source, string fetched)
        {
            throw new NotImplementedException();
        }
    }
}
