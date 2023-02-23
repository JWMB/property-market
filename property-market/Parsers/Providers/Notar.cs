using Parsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsers.Providers
{
    internal class Notar : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        // https://www.notar.se/kopa-bostad?sortBy=date&sortOrder=desc
        public Uri DefaultUri => new Uri("https://www.notar.se");

        public IPropertyDataProvider DataProvider => this;

        public string Id => nameof(Notar);

        public bool IsAggregator => false;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public Task<IPropertyListingProvider.FetchResult> FetchPropertyListingResult(string objectId)
        {
            throw new NotImplementedException();
        }

        public Task<IPropertyListingSearchProvider.FetchResult> FetchPropertySearchResults(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            throw new NotImplementedException();
        }

        public List<PropertyListing> ParseSearchResults(Uri source, string fetched)
        {
            throw new NotImplementedException();
        }
    }

    public class HusmanHagberg
    {
        // https://www.husmanhagberg.se/kopa/
    }

    public class Bjurfors
    {
        // https://www.bjurfors.se/sv/tillsalu/
    }

    public class ErikOlsson
    {
        // https://www.erikolsson.se/bostader-till-salu/
    }

    public class Skandiamaklarna
    {
        // https://www.skandiamaklarna.se/till-salu/resultat/all/all/min%2Cmax/min%2Cmax/min%2Cmax
    }

}
