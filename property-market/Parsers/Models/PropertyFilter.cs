namespace Parsers.Models
{
    public class MinMax
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
    }

    public class PropertyFilter
    {
        public string? Location { get; set; }
        public MinMax? Price { get; set; }
        public MinMax? LivingArea { get; set; }
        public MinMax? OutsideM2 { get; set; }
        public MinMax? Rooms { get; set; }
        public MinMax? PricePerM2 { get; set; }
        public MinMax? BuiltYear { get; set; }

        public MinMax? CoopFee { get; set; }
        public MinMax? OperatingCosts { get; set; }

        public bool? Balcony { get; set; }
        public bool? Patio { get; set; }
        public bool? Fireplace { get; set; }

        public MinMax? Floor { get; set; }
    }
}
