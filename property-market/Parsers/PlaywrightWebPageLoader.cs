
using Microsoft.Playwright;
using System.Text.Json;

namespace Parsers
{
    internal class PlaywrightWebPageLoader
    {
        public class Filter
        {
            public required Func<IResponse, string, bool> IsMatch { get; set; }

            public Action<string?>? HandleText { get; set; }
            public Action<JsonElement?>? HandleJson { get; set; }
        }

        public async Task<IResponse?> Load(Uri uri, IEnumerable<Filter>? filters = null)
        {
            using var pw = await Playwright.CreateAsync();
            await using var browser = await pw.Chromium.LaunchAsync();

            var page = await browser.NewPageAsync();

            page.Response += async (sender, response) =>
            {
                if (response.Ok)
                {
                    if (filters != null)
                    {
                        var contentType = response.GetHeaderFallback("content-type").ToLower().Trim();
                        Console.WriteLine($"{contentType}\t{response.Url}");

                        foreach (var f in filters.Where(f => ExTools.TryOrDefault(() => f.IsMatch(response, contentType), false)))
                        {
                            if (f.HandleText != null)
                            {
                                f.HandleText(await ExTools.TryOrDefault(async () => await response.TextAsync(), ""));
                            }
                            else if (f.HandleJson != null)
                            {
                                f.HandleJson(await ExTools.TryOrDefault(async () => await response.JsonAsync(), Task.FromResult((JsonElement?)null)));
                            }
                        }
                    }
                }
            };

            try
            {
                return await page.GotoAsync(uri.AbsoluteUri, new PageGotoOptions { Timeout = (float)TimeSpan.FromSeconds(30).TotalMilliseconds, WaitUntil = WaitUntilState.NetworkIdle });
            }
            catch (TimeoutException)
            { }
            return null;
        }
    }
}
