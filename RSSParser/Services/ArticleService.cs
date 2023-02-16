using System.Text.RegularExpressions;

namespace RSSParser.Services;

public class ArticleService
{
    private readonly string _removeJavaScriptRegex = "<script[^>]*>[\\s\\S]*?</script>";

    public string RemoveJavaScriptCode(string input)
    {
        return Regex.Replace(input, _removeJavaScriptRegex, string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }
}