using System.ServiceModel.Syndication;
using System.Xml;
using MoreLinq.Extensions;
using RSS_Feed_Aggregator.Db;
using RSS_Feed_Aggregator.Models;

namespace RSS_Feed_Aggregator.Services;

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
            tasks.Add(Task.Run(() =>
            {
                Parallel.ForEach(batch, async url =>
                {
                    using (var reader = XmlReader.Create(url))
                    {
                        var feed = SyndicationFeed.Load(reader);
                        foreach (var item in feed.Items)
                            // Check if the article with the same title and link already exists in the database
                            if (!_context.Articles.Any(a =>
                                    a.Title == item.Title.Text && a.Link == item.Links[0].Uri.ToString()))
                            {
                                var article = new Article
                                {
                                    Title = _articleService.RemoveJavaScriptCode(item.Title.Text),
                                    Link = item.Links[0].Uri.ToString(),
                                    Description = _articleService.RemoveJavaScriptCode(item.Summary.Text),
                                    Author = _articleService.RemoveJavaScriptCode(item.Authors[0].Name),
                                    PublicationDate = item.PublishDate.DateTime,
                                    Image = item.Links.Where(l => l.MediaType == "image/jpeg").FirstOrDefault()?.Uri
                                        .ToString()
                                    //Image = item.Links.FirstOrDefault(l => l.MediaType == "image/jpeg")?.Uri.ToString()
                                };
                                await _tagService.AddTagsToArticle(article);
                                _context.Articles.Add(article);
                            }
                    }
                });
            }));

        await Task.WhenAll(tasks);
        await _context.SaveChangesAsync();
    }
}