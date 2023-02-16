using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using MoreLinq.Extensions;
using RSS_Feed_Aggregator.Db;
using RSS_Feed_Aggregator.Models;

namespace RSSParser.Services;

public class RssFeedParser
{
    private readonly ArticleService _articleService;
    private readonly RssDbContext _context;
    private readonly TagService _tagService;

    public RssFeedParser(RssDbContext context, ArticleService articleService, TagService tagService)
    {
        _context = context;
        _articleService = articleService;
        _tagService = tagService;
    }

    public async Task ParseRssFeeds(List<string> rssFeedUrls)
    {
        var tasks = new List<Task>();
        var batchSize = 10;
        var batches = rssFeedUrls.Batch(batchSize).ToList();

        foreach (var batch in batches)
            tasks.Add(Task.Run(async () =>
            {
                foreach (var url in batch)
                    try
                    {
                        await using var dbContext = new RssDbContext(CreateDbContextOptions());
                        using var reader = XmlReader.Create(url);
                        var feed = SyndicationFeed.Load(reader);

                        foreach (var item in feed.Items)
                            if (!dbContext.Articles.Any(a =>
                                    a.Title == item.Title.Text && a.Link == item.Links[0].Uri.ToString()))
                            {
                                var article = new Article
                                {
                                    Title = _articleService.RemoveJavaScriptCode(item.Title.Text),
                                    Link = item.Links[0].Uri.ToString(),
                                    Description = _articleService.RemoveJavaScriptCode(item.Summary.Text),
                                    Author = _articleService.RemoveJavaScriptCode(item.Authors[0].Name),
                                    PublicationDate = item.PublishDate.DateTime,
                                    Image = item.Links.FirstOrDefault(l => l.MediaType == "image/jpeg")?.Uri.ToString()
                                };

                                await _tagService.AddTagsToArticle(article);
                                dbContext.Articles.Add(article);
                            }

                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while processing RSS feed {url}: {ex.Message}");
                    }
            }));

        await Task.WhenAll(tasks);
    }

    private static DbContextOptions<RssDbContext> CreateDbContextOptions()
    {
        var builder = new DbContextOptionsBuilder<RssDbContext>();
        builder.UseSqlServer("Server=localhost;Database=RSS-Feed_db;User Id=nika;Password=123;Encrypt=false;");
        return builder.Options;
    }
}