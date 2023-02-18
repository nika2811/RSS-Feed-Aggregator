using Microsoft.EntityFrameworkCore;
using RSS_Feed_Aggregator.Db;
using RSSParser.Services;

namespace RSSParser;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var rssDbContext = new RssDbContext(CreateDbContextOptions());
        var articleService = new ArticleService();
        var tagService = new TagService(rssDbContext);

        var feedsParser = new RssFeedParser(rssDbContext, articleService, tagService);
        var rssFeedUrls = new List<string>
        {
            "https://stackoverflow.blog/feed/",
            "https://www.freecodecamp.org/news/rss",
            "https://martinfowler.com/feed.atom",
            "https://codeblog.jonskeet.uk/feed/",
            "https://devblogs.microsoft.com/visualstudio/feed/",
            "https://feed.infoq.com/",
            "https://css-tricks.com/feed/",
            "https://codeopinion.com/feed/",
            "https://andrewlock.net/rss.xml",
            "https://michaelscodingspot.com/index.xml",
            "https://www.tabsoverspaces.com/feed.xml"
        };


        await feedsParser.ParseRssFeeds(rssFeedUrls);
        Console.WriteLine("RSS feed parsing completed");
    }

    private static DbContextOptions<RssDbContext> CreateDbContextOptions()
    {
        var builder = new DbContextOptionsBuilder<RssDbContext>();
        builder.UseSqlServer("Server=localhost;Database=RSS-Feed_db;User Id=nika;Password=123;Encrypt=false;");
        return builder.Options;
    }
}