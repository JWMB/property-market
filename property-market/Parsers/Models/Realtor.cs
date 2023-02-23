namespace Parsers.Models
{

    public class Realtor
    {
        public string Name { get; set; } = string.Empty;
        public RealtorCompany? Company { get; set; }
    }

    public class RealtorCompany
    {
        public string Name { get; set; } = string.Empty;
        public Uri? Homepage { get; set; }
        public Address? Address { get; set; }
    }
}