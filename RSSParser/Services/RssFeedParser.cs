using System.Net.Mime;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using MoreLinq.Extensions;
using RSS_Feed_Aggregator.Db;
using RSS_Feed_Aggregator.Models;
using static System.Net.Mime.MediaTypeNames;

namespace RSSParser.Services;

public class RssFeedParser
{
    private readonly ArticleService _articleService;
    private readonly TagService _tagService;

    public RssFeedParser(ArticleService articleService, TagService tagService)
    {
        _articleService = articleService;
        _tagService = tagService;
    }

    public async Task ParseRssFeeds(List<string> rssFeedUrls)
    {
        var tasks = new List<Task>();
        var batchSize = 10;
        var batches = rssFeedUrls.Batch(batchSize).ToList();

        await using var db = new RssDbContext(CreateDbContextOptions());
        await db.Database.EnsureCreatedAsync();

        foreach (var batch in batches)
            tasks.Add(Task.Run(async () =>
            {
                await using var dbContext = new RssDbContext(CreateDbContextOptions());
                foreach (var url in batch)
                    try
                    {
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
                                    Description =
                                        _articleService.RemoveJavaScriptCode(item.Summary == null
                                            ? ""
                                            : item.Summary.Text),
                                    Author = item.Authors.Count > 0
                                        ? _articleService.RemoveJavaScriptCode(item.Authors[0].Name)
                                        : null,
                                    PublicationDate = item.PublishDate.DateTime,
                                };
                                article.Image = feed.ImageUrl?.ToString();
                                await _tagService.AddCategoriesToArticle(item.Categories.ToList(), article);
                               // dbContext.Articles.Add(article);
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