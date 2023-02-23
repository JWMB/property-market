namespace Parsers.Models
{
    public class Address
    {
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? StreetAddress { get; set; }

        public override string ToString() => $"{StreetAddress}";
    }
}