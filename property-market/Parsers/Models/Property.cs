using KellermanSoftware.CompareNetObjects;
using System;

namespace Parsers.Models
{
    public class Property
    {
        public Address Address { get; set; } = new();
        public PropertyType Type { get; set; }

        public decimal? NumRooms { get; set; }
        public decimal? LivingArea { get; set; }
        public decimal? AdditionalArea { get; set; }
        public decimal? YardArea { get; set; }

        public string? EnergyClass { get; set; }

        public Uri? PrimarySource { get; set; }

        public DateTimeOffset? DateConstructed { get; set; }

        // TODO: Subtype Coop?
        public int? MonthlyPayment { get; set; }
        public decimal? OwnershipPartOfCoop { get; set; }
        public string? CoopId { get; set; }

        // TODO: Subtype apartment?
        public decimal? Floor { get; set; }
        public bool? Elevator { get; set; }

        public List<Document> Documents { get; set; } = new();

        public override string ToString() => $"{NumRooms}/{LivingArea}m² {Address}";

        public bool Compare(Property other)
        {
            var compareLogic = new CompareLogic();
            var result = compareLogic.Compare(this, other);
            return result.AreEqual;
        }
    }

    public class Document
    {
        public DocumentType Type { get; set; }
        public required Uri Source { get; set; }
        public required DateTimeOffset Fetched { get; set; }
        public DateTimeOffset? PertainingTo { get; set; }
        public DateTimeOffset? Published { get; set; }
        public string? Name { get; set; }

        public string? Content { get; set; }
        public string? ContentLocation { get; set; }
    }

    public enum DocumentType
    {
        CoopAnnualReport,
        EnergyDeclaration,
        InspectionReport,
        Other
    }

    public enum PropertyType
    {
        Unknown,
        Apartment,
        House,
        Terraced,
        Cabin,
        Farm,
        Land
    }
}