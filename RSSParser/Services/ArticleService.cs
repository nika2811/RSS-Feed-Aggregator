using HtmlAgilityPack;

namespace RSSParser.Services;

public class ArticleService
{
    // private readonly string _removeJavaScriptRegex = "<script[^>]*>[\\s\\S]*?</script>";
    //
    // public string RemoveJavaScriptCode(string input)
    // {
    //     return Regex.Replace(input, _removeJavaScriptRegex, string.Empty,
    //         RegexOptions.IgnoreCase | RegexOptions.Multiline);
    // }

    public string RemoveJavaScriptCode(string input)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(input);
        htmlDoc.DocumentNode.Descendants("script").ToList().ForEach(n => n.Remove());
        return htmlDoc.DocumentNode.OuterHtml;
    }
}