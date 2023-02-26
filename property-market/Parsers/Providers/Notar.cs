using Newtonsoft.Json;
using Parsers.Models;
using static Parsers.Providers.Notar;

namespace Parsers.Providers
{
    public class Notar : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        private readonly IDataFetcher dataFetcher;

        public Uri DefaultUri => new Uri("https://something");

        public IPropertyDataProvider DataProvider => this;

        public string Id => nameof(Template);

        public bool IsAggregator => false;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public Notar(IDataFetcher dataFetcher)
        {
            this.dataFetcher = dataFetcher;
        }

        public /*async*/ Task<FetchResult> FetchListing(string objectId)
        {
            throw new NotImplementedException();

        }

        public async Task<FetchResult> FetchSearchListings(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            var curl = new CurlRequest("""curl 'https://data.notar.se/objects?limit=24&sortBy=date&sortOrder=desc&assignmentStatus=Kommande&assignmentStatus=Till%20salu&assignmentStatus=P%C3%A5g%C3%A5ende' -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/110.0' -H 'Accept: application/json, text/plain, */*' -H 'Accept-Language: en-US,en;q=0.5' -H 'Accept-Encoding: gzip, deflate, br' -H 'Referer: https://www.notar.se/' -H 'authorization: Basic bm90YXI6OWd6ekRmRGZCVXpjMjJleVhqUG53bU5m' -H 'Origin: https://www.notar.se' -H 'DNT: 1' -H 'Connection: keep-alive' -H 'Sec-Fetch-Dest: empty' -H 'Sec-Fetch-Mode: cors' -H 'Sec-Fetch-Site: same-site' -H 'Pragma: no-cache' -H 'Cache-Control: no-cache' -H 'TE: trailers'""");
            var result = await dataFetcher.Fetch(curl);
            if (result == null) throw new Exception($"Failed {curl.Uri}");
            return new FetchResult { Content = result, Source = curl.Uri };
        }

        public PropertyListing ParseListing(Uri source, string fetched)
        {
            throw new NotImplementedException();
        }

        public List<PropertyListing> ParseSearchListings(Uri source, string fetched)
        {
            var items = JsonConvert.DeserializeObject<List<Root>>(fetched);
            if (items == null)
                throw new Exception($"Couldn't deserialize {source}");

            return items.Select(item => {
                return new PropertyListing
                {
                    CrawledFrom = source,
                    ListingId = item.entityId,
                    Provider = this,
                    RealtorListingPage = new Uri($"https://www.notar.se/kopa-bostad/objekt/{item.entityId}"),

                    Price = item.price.startingPrice,
                    PriceType = item.price.type == "Utgångspris" ? PriceType.Starting : PriceType.Starting, //TODO
                    // TODO:
                    // "type":"HousingCooperative"
                    // documents":[{"file":"CFIL4STJPTVGQFJTH1M1.pdf","name":"Stadgar.pdf"},{"file":"CFIL5A4QR887P44TP2EL.pdf","name":"Årsredovisning - 2020/2021.pdf"}
                    // "bids":[]
                    // "energyDeclaration":{}

                    Property = new Property
                    {
                        NumRooms = item.numberOfRooms,
                        LivingArea = (decimal)item.livingSpaceArea,

                        Address = new Address
                        {
                            StreetAddress = item.address,
                            ZipCode = item.zipCode,
                            City = item.city,
                            Region = item.county,
                        }
                    }
                };
            }).ToList();
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
        public class AreaData
        {
            public List<PolygonWGS84> polygonWGS84 { get; set; }
            public string entityType { get; set; }
            public Surrounding surrounding { get; set; }
            public string filterValue { get; set; }
            public List<Polygon> polygon { get; set; }
            public string entityId { get; set; }
            public List<Image> images { get; set; }
            public string regionCode { get; set; }
            public string text { get; set; }
            public string type { get; set; }
            public string description { get; set; }
        }

        public class Association
        {
            public string name { get; set; }
            public int numberOfApartments { get; set; }
            public int numberOfPremises { get; set; }
            public string corporateNumber { get; set; }
            public string organizationalForm { get; set; }
            public bool genuineAssociation { get; set; }
            public string theAssociationOwnTheGround { get; set; }
            public string allowTwinOwnership { get; set; }
            public string allowLegalPersonAsBuyer { get; set; }
            public string about { get; set; }
            public string courtyard { get; set; }
            public string renovations { get; set; }
            public string finances { get; set; }
            public string sharedSpaces { get; set; }
            public string other { get; set; }
            public double participationInAssociation { get; set; }
            public double participationOffAnnualFee { get; set; }
            public int transferFee { get; set; }
            public string transferFeePaidBy { get; set; }
            public int pledgeFee { get; set; }
            public Building building { get; set; }
            public string tvAndBroadband { get; set; }
            public string parking { get; set; }
            public EnergyDeclaration energyDeclaration { get; set; }
            public string insurance { get; set; }
            public int? indirectNetDebt { get; set; }
            public string indirectNetDebtComment { get; set; }
            public int? numberOfRentalUnits { get; set; }
            public string participationComment { get; set; }
        }

        public class BalconyPatio
        {
            public bool balcony { get; set; }
            public bool? parkingLot { get; set; }
            public string description { get; set; }
            public bool? patio { get; set; }
        }

        public class Building
        {
            public string buildingType { get; set; }
            public string elevator { get; set; }
            public string buildingYear { get; set; }
            public string heating { get; set; }
            public string ventilation { get; set; }
            public string windows { get; set; }
        }

        public class Construction
        {
            public string facade { get; set; }
            public string windows { get; set; }
            public string roof { get; set; }
            public string other { get; set; }
        }

        public class Descriptions
        {
            public string @long { get; set; }
            public string title { get; set; }
            public string @short { get; set; }
        }

        public class Document
        {
            public string file { get; set; }
            public string name { get; set; }
        }

        public class Electricity
        {
            public string companie { get; set; }
            public string distributor { get; set; }
            public int powerConsumptionKWH { get; set; }
        }

        public class EnergyDeclaration
        {
            public string completedDescription { get; set; }
            public string primaryEnergyNumber { get; set; }
            public string @class { get; set; }
            public string declarationDate { get; set; }
            public string consumption { get; set; }
        }

        public class Enrollment
        {
            public string planRegulations { get; set; }
            public string preferentialAndCommunity { get; set; }
            public string enrolledEasement { get; set; }
            public string easementsLoad { get; set; }
        }

        public class Floors
        {
            public int floor { get; set; }
            public int totalNumberOfFloors { get; set; }
            public string floorDescription { get; set; }
        }

        public class Image
        {
            public string file { get; set; }
            public int order { get; set; }
            public string category { get; set; }
            public string title { get; set; }
        }

        public class Insurance
        {
            public int amount { get; set; }
            public string company { get; set; }
            public string fullValue { get; set; }
        }

        public class Interior
        {
            public int numberOfRooms { get; set; }
            public string kitchenType { get; set; }
            public string description { get; set; }
            public List<Room> rooms { get; set; }
        }

        public class Lease
        {
        }

        public class Leasehold
        {
        }

        public class LivingSpaceArea
        {
            public double livingSpaceArea { get; set; }
            public string areaSource { get; set; }
            public string areaSourceComment { get; set; }
        }

        public class MonthlyFee
        {
            public int monthlyFee { get; set; }
            public string monthlyFeeDescription { get; set; }
            public bool monthlyFeeIsZero { get; set; }
        }

        public class Mortgage
        {
            public int? numberOfMortgages { get; set; }
            public int? totalMortgageAmount { get; set; }
        }

        public class ObjectSettings
        {
            public bool showPrice { get; set; }
            public bool showAddress { get; set; }
            public bool showAsComing { get; set; }
            public bool showAsPreview { get; set; }
            public bool showAsReferenceHousing { get; set; }
            public bool showAsSoonForSale { get; set; }
            public bool showAsTodaysHousing { get; set; }
        }

        public class OperationCosts
        {
            public int? electricity { get; set; }
            public int? insurance { get; set; }
            public int? personsInTheHousehold { get; set; }
            public int? sum { get; set; }
            public string otherDescription { get; set; }
            public int? chimneySweeping { get; set; }
            public int? electricityConsumptionKWH { get; set; }
            public int? heating { get; set; }
            public int? sanitation { get; set; }
            public int? waterAndDrain { get; set; }
            public int? taxFee { get; set; }
            public int? totalSum { get; set; }
            public int? other { get; set; }
        }

        public class Other
        {
            public string areaSource { get; set; }
            public string localIncomeTax { get; set; }
            public Surrounding surrounding { get; set; }
            public object otherBuildings { get; set; }
            public string possibleAccessDate { get; set; }
            public string otherDescription { get; set; }
            public string areaSourceComment { get; set; }
            public string tv { get; set; }
            public string broadband { get; set; }
            public string parking { get; set; }
            public int? otherSpaceArea { get; set; }
        }

        public class Plot
        {
            public int area { get; set; }
            public object buildingPermission { get; set; }
            public object other { get; set; }
            public string parking { get; set; }
            public string patio { get; set; }
            public string plotType { get; set; }
        }

        public class Polygon
        {
            public int latitude { get; set; }
            public int longitude { get; set; }
        }

        public class PolygonWGS84
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Price
        {
            public string currency { get; set; }
            public string type { get; set; }
            public int startingPrice { get; set; }
            public int startingLivingSpacePricePerArea { get; set; }
            public int? finalPrice { get; set; }
            public int? finalLivingSpacePricePerArea { get; set; }
        }

        public class Residence
        {
            public string areaId { get; set; }
            public string area { get; set; }
            public string municipality { get; set; }
            public LivingSpaceArea livingSpaceArea { get; set; }
            public int numberOfRooms { get; set; }
            public string kitchenType { get; set; }
            public TvAndBroadband tvAndBroadband { get; set; }
            public Tax tax { get; set; }
            public Mortgage mortgage { get; set; }
            public Enrollment enrollment { get; set; }
            public EnergyDeclaration energyDeclaration { get; set; }
            public string parking { get; set; }
            public string propertyType { get; set; }
            public MonthlyFee monthlyFee { get; set; }
            public double participationInAssociation { get; set; }
            public double participationOffAnnualFee { get; set; }
            public string tenure { get; set; }
            public bool pawned { get; set; }
            public string apartmentNumber { get; set; }
            public Floors floors { get; set; }
            public BalconyPatio balconyPatio { get; set; }
            public string possibleAccessDate { get; set; }
            public string otherDescription { get; set; }
            public int? indirectNetDebt { get; set; }
            public string indirectNetDebtComment { get; set; }
            public int? otherSpaceArea { get; set; }
            public string waterAndDrain { get; set; }
            public string disposalForm { get; set; }
            public string buildingType { get; set; }
            public string propertyUnitDesignation { get; set; }
            public int? plotArea { get; set; }
            public string patio { get; set; }
            public string buildingYear { get; set; }
            public Construction construction { get; set; }
            public string renovations { get; set; }
            public string heating { get; set; }
            public string plotParking { get; set; }
            public Leasehold leasehold { get; set; }
            public Lease lease { get; set; }
            public string participationComment { get; set; }
        }

        public class Room
        {
            public string title { get; set; }
            public string description { get; set; }
        }

        public class Root
        {
            public string entityType { get; set; }
            public string entityId { get; set; }
            public string id { get; set; }
            public string originalEstateId { get; set; }
            public string type { get; set; }
            public string indexType { get; set; }
            public string agentId { get; set; }
            public string additionalAgentId { get; set; }
            public string customerId { get; set; }
            public string officeId { get; set; }
            public ObjectSettings objectSettings { get; set; }
            public Descriptions descriptions { get; set; }
            public string updatedAt { get; set; }
            public string updatedRemotelyAt { get; set; }
            public Price price { get; set; }
            public string address { get; set; }
            public string county { get; set; }
            public string area { get; set; }
            public string municipality { get; set; }
            public string zipCode { get; set; }
            public string city { get; set; }
            public double longitude { get; set; }
            public double latitude { get; set; }
            public string countryCode { get; set; }
            public double livingSpaceArea { get; set; }
            public int numberOfRooms { get; set; }
            public string assignmentStatus { get; set; }
            public List<object> marketingMethods { get; set; }
            public bool active { get; set; }
            public bool inspected { get; set; }
            public bool inspected_alt { get; set; }
            public Electricity electricity { get; set; }
            public List<Image> images { get; set; }
            public List<Document> documents { get; set; }
            public List<Viewing> viewings { get; set; }
            public List<object> links { get; set; }
            public List<object> bids { get; set; }
            public string bidSetting { get; set; }
            public Interior interior { get; set; }
            public Other other { get; set; }
            public Residence residence { get; set; }
            public OperationCosts operationCosts { get; set; }
            public bool notarPlus { get; set; }
            public Association association { get; set; }
            public AreaData areaData { get; set; }
            public bool? buyersInspection { get; set; }
            public Insurance insurance { get; set; }
            public Plot plot { get; set; }
        }

        public class Surrounding
        {
            public string generalAboutArea { get; set; }
            public string nearService { get; set; }
            public string communication { get; set; }
            public string parking { get; set; }
            public string other { get; set; }
        }

        public class Tax
        {
            public string typeCode { get; set; }
            public int? taxAssessmentYear { get; set; }
            public int? taxFee { get; set; }
            public int? landValue { get; set; }
            public int? buildingValue { get; set; }
            public int? totalAssessedValue { get; set; }
            public bool? preliminaryAssessedValue { get; set; }
            public int? valueYear { get; set; }
        }

        public class TvAndBroadband
        {
            public string tv { get; set; }
            public string broadband { get; set; }
        }

        public class Viewing
        {
            public string start { get; set; }
            public string end { get; set; }
            public string bookingUrl { get; set; }
            public string commentary { get; set; }
        }


    }
}
