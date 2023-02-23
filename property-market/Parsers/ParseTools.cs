using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;

namespace Parsers
{
    public class ParseTools
    {
        public static DateTimeOffset? ParseDate(Dictionary<string, string?> dict, string key) =>
            dict.TryGetValue(key, out var value) ? ParseDate(value) : null;

        public static DateTimeOffset? ParseDate(string? value)
        {
            if (value == null)
                return null;
            if (value.Contains("/"))
            {
                // 17/2 14:22;
                var m = Regex.Match(value, @"(?<day>\d{1,2})/(?<month>\d{1,2})\s+(?<time>(?<hour>\d{1,2}):(?<minute>\d{1,2}):?(?<second>\d{1,2})?)?"); //:?(?<second>\d{1,2})?
                if (m.Success)
                {
                    return new DateTimeOffset(
                        DateTimeOffset.Now.Year, int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value),
                        GetFromGroup(m, "hour"), GetFromGroup(m, "minute"), GetFromGroup(m, "second"),
                        TimeSpan.Zero
                        );
                }
            }
            return null;

            int GetFromGroup(Match m, string name) =>
                m.Groups[name].Success ? int.Parse(m.Groups[name].Value) : 0;
        }

        public static DateTimeOffset? ParseYear(Dictionary<string, string?> dict, string key) =>
            dict.TryGetValue(key, out var value) ? ParseYear(value) : null;

        public static DateTimeOffset? ParseYear(string? value)
        {
            var year = ParseNumber(value);
            return year == null ? null : new DateTimeOffset((int)year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        }

        public static decimal? ParseNumber(Dictionary<string, string?> dict, string key) =>
            dict.TryGetValue(key, out var value) ? ParseNumber(value) : null;

        public static decimal? ParseNumber(string? value)
        {
            if (value == null)
                return null;
            value = Regex.Replace(value, @"\s", ""); //.Replace(" ", "").Trim();
            var m = Regex.Match(value, @"[\d.,½]+");
            if (!m.Success)
                return null;

            var trimmedValue = m.Value.Trim();
            if (decimal.TryParse(trimmedValue, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var result))
                return result;
            if (trimmedValue.Contains(","))
            {
                trimmedValue = trimmedValue.Replace(",", ".");
                if (decimal.TryParse(trimmedValue, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out result))
                    return result;
            }
            return ParseSpecialFraction(value);
        }

        public static decimal ParseSpecialFractionOrThrow(string? value, string suffix = "")
        {
            var result = ParseSpecialFraction(value, suffix);
            if (result == null)
                throw new Exception($"Couldn't parse '{value}'");
            return result.Value;
        }
            
        public static decimal? ParseSpecialFraction(string? value, string suffix = "")
        {
            if (value == null)
                return null;

            var rx = new Regex(@"(\d+[.,]?\d*½?)\s*" + suffix);
            var m = rx.Match(value);
            if (m.Success)
            {
                var val = m.Groups[1].Value;
                val = val.Replace("½", ".5").Replace(",", ".");
                if (decimal.TryParse(val, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out var result))
                    return result;
            }
            return null;
        }

        public static IHtmlDocument ParseHtml(string html, Uri? source = null)
        {
            var parser = new HtmlParser();
            try
            {
                var doc = parser.ParseDocument(html);
                if (doc == null)
                    throw new Exception($"Null result");

                return doc;
            }
            catch (Exception e)
            {
                throw new Exception($"Parse error {source}", e);
            }
        }

        public static (IHtmlDocument, IHtmlHeadElement, IHtmlElement) ParseHtmlWithHeadAndBody(string html, Uri? source = null)
        {
            var doc = ParseHtml(html, source);
            if (doc.Head == null)
                throw new Exception($"Head is null {source}");
            if (doc.Body == null)
                throw new Exception($"Body is null {source}");

            return (doc, doc.Head, doc.Body);
        }
    }
}
