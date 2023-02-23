namespace Parsers.Models
{
    public class Coop
    {
        public List<Address> Addresses { get; set; } = new();
        public Uri? Homepage { get; set; }
        public List<AnnualReport> AnnualReports { get; set; } = new();
    }

    public class AnnualReport
    {
    }
}
