namespace Parsers
{
    public interface IPropertyDataProvider
    {
        Uri DefaultUri { get; }
        string Id { get; }

        bool IsAggregator { get; }

        IPropertyListingSearchProvider? SearchProvider { get; }
        IPropertyListingProvider? ListingProvider { get; }

        public int GetHashCode() => Id.GetHashCode();
        public bool Equals(IPropertyDataProvider other) => Id == other.Id;
    }
}
