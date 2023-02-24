using Parsers.Models;

namespace Parsers
{
    public interface IPropertyListingProvider
    {
        IPropertyDataProvider DataProvider { get; }

        Task<FetchResult> FetchListing(string objectId);

        PropertyListing ParseListing(Uri source, string fetched);
    }
}
