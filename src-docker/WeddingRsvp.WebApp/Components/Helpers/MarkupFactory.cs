using System.Net;
using System.Text.RegularExpressions;

namespace WeddingRsvp.WebApp.Components.Helpers;

public static class MarkupFactory
{
    public static string Linkify(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var encoded = WebUtility.HtmlEncode(text);

        // [label](https://example.com)
        var markdownLinkRegex = new Regex(@"\[(.+?)\]\((https?://[^\s]+)\)");
        return markdownLinkRegex.Replace(
            encoded,
            "<a href=\"$2\" target=\"_blank\" rel=\"noopener\">$1</a>");
    }

    public static string SelectPluralForm(bool isPlural, string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Look for {singular/plural}
        var pattern = @"\{([^{}\/]+)\/([^{}\/]+)\}";

        return Regex.Replace(text, pattern, match =>
            isPlural ? match.Groups[2].Value : match.Groups[1].Value);
    }
}