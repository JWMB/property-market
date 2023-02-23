using Parsers.Models;

namespace Parsers
{
    public interface IPropertyListingProvider
    {
        IPropertyDataProvider DataProvider { get; }

        Task<FetchResult> FetchPropertyListingResult(string objectId);

        public class FetchResult
        {
            public required PropertyListing Listing { get; set; }
            public required string RawResult { get; set; }
        }
    }
}
