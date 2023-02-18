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
                                    // Author = _articleService.RemoveJavaScriptCode(item.Authors[0].Name),
                                    Author = item.Authors.Count > 0
                                        ? _articleService.RemoveJavaScriptCode(item.Authors[0].Name)
                                        : null,
                                    PublicationDate = item.PublishDate.DateTime,
                                    Image = item.Links.FirstOrDefault(l => l.MediaType == "image/")?.Uri.ToString()
                                };

                                await _tagService.AddTagsToArticle(article);
                                dbContext.Articles.Add(article);

                                var tags = item.Categories.Select(c => c.Name);
                                foreach (var tagName in tags)
                                {
                                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                                    var articleTag = new ArticleTag { Article = article, Tag = tag };
                                    dbContext.ArticleTags.Add(articleTag);
                                }
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