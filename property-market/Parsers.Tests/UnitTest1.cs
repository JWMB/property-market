using Parsers.Providers;
using Shouldly;

namespace Parsers.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var providerTestData = new[]
            {
                new { Provider = (IPropertyDataProvider)new Hemnet(new FileDataFetcher( new Dictionary<string, string>{
                    { "bostader?location_ids", "Hemnet_list_230221_0854.dat" }, { "singlelisting", "Hemnet_item_230221_1227.dat" } }))},
                new { Provider = (IPropertyDataProvider)new Fastighetsbyran(new FileDataFetcher( new Dictionary<string, string>{
                    { "api/v1/soek", "Fastighetsbyran_list_230223_2037.dat" }, { "singlelisting", "fastighetsbyran-object.dat" } }))},
                new { Provider = (IPropertyDataProvider)new Lansfast(new FileDataFetcher( new Dictionary<string, string>{
                    { "api/findestateapi", "Lansfast.dat" }, { "singlelisting", "Lansfast_item_230222_0739.dat" } } )) },
                new { Provider = (IPropertyDataProvider)new SvenskFast(new FileDataFetcher( new Dictionary<string, string>{
                    { "/bostad/", "svenskfastList.dat" }, { "singlelisting", "svenskfastItem.dat" } } )) },
            };

            foreach (var item in providerTestData)
            {
                await TryCatch(async () => {
                    var result = await item.Provider.SearchProvider!.FetchSearchListings();
                    item.Provider.SearchProvider!.ParseSearchListings(result.Source, result.Content);
                }, $"{item.Provider.GetType().Name} Search");
                await TryCatch(async () => await item.Provider.ListingProvider!.FetchListing("singlelisting"), $"{item.Provider.GetType().Name} Listing");
            }

            async Task TryCatch(Func<Task> act, string exceptionInfo)
            {
                try
                {
                    await act();
                }
                catch (Exception ex)
                {
                    throw new Exception(exceptionInfo, ex);
                }
            }
        }

        class FileDataFetcher : IDataFetcher
        {
            private readonly Dictionary<string, string> uriMatchAndResult;

            public FileDataFetcher(Dictionary<string, string> uriMatchAndResult)
            {
                this.uriMatchAndResult = uriMatchAndResult;
            }

            public async Task<string> Fetch(HttpRequestMessage request)
            {
                var found = uriMatchAndResult.FirstOrDefault(o => request.RequestUri!.AbsoluteUri.Contains(o.Key));
                if (found.Value == null)
                    throw new Exception($"No key found: {request.RequestUri}");

                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "property-market", found.Value);
                if (!File.Exists(path))
                    throw new Exception($"File doesn't exist: {path}");

                return await File.ReadAllTextAsync(path);
            }
        }
    }
}