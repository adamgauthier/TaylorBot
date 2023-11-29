using System.Text.Encodings.Web;

namespace TaylorBot.Net.Core.Infrastructure.Extensions;

public static class DictionaryExtensions
{
    public static string ToUrlQueryString(this IDictionary<string, string> values)
    {
        return string.Join('&', values.Select(kvp =>
            $"{UrlEncoder.Default.Encode(kvp.Key)}={UrlEncoder.Default.Encode(kvp.Value)}"));
    }
}
