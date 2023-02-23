namespace Parsers
{
    public interface IPropertyDataProvider
    {
        Uri DefaultUri { get; }
        string Id { get; }

        bool IsAggregator { get; }

        IPropertyListingSearchProvider? SearchProvider { get; }
        IPropertyListingProvider? ListingProvider { get; }
    }
}
