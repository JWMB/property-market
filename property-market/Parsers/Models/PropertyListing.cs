using KellermanSoftware.CompareNetObjects;

namespace Parsers.Models
{
    public class PropertyListing
    {
        public required string ListingId { get; set; }
        public Property Property { get; set; } = new();

        public DateTimeOffset? PreviewAdded { get; set; }
        public DateTimeOffset ListingAdded { get; set; }
        public DateTimeOffset? Removed { get; set; }
        public DateTimeOffset? SoldAt { get; set; }

        public int? Price { get; set; }
        public PriceType PriceType { get; set; } = PriceType.Starting;

        public List<Bid> Bids { get; set; } = new();

        public required IPropertyDataProvider Provider { get; set; }
        
        public required Uri CrawledFrom { get; set; }
        public Uri? UriWithProvider { get; set; }
        public Uri? RealtorListingPage { get; set; }

        public Realtor? Realtor { get; set; }

        public Dictionary<string, string> Untyped { get; set; } = new();

        public override string ToString()
        {
            return $"{Property} {Price}";
        }

        public bool Compare(PropertyListing other)
        {
            var compareLogic = new CompareLogic();
            var result = compareLogic.Compare(this, other);
            return result.AreEqual;
        }
    }

    public enum PriceType
    {
        Starting,
        Accept,
    }

    public class Bid
    {
        public DateTimeOffset Time { get; set; }
        public string Bidder { get; set; } = string.Empty;
        public int Price { get; set; }
    }
}
