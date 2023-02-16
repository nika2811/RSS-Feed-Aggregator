using RSS_Feed_Aggregator.Db;
using RSS_Feed_Aggregator.Models;

namespace RSSParser.Services;

public class TagService
{
    private readonly RssDbContext _context;

    public TagService(RssDbContext context)
    {
        _context = context;
    }

    public async Task AddTagsToArticle(Article article)
    {
        var tagsToAdd = new List<Tag>();
        var articleText = article.Title + " " + article.Description;
        var existingTags = _context.Tags.ToList();

        foreach (var existingTag in existingTags)
            if (articleText.Contains(existingTag.Name))
                article.ArticleTags.Add(new ArticleTag { Article = article, Tag = existingTag });

        var newTags = articleText.Split(" ")
            .Where(s => !existingTags.Any(et => et.Name.Contains(s)))
            .Distinct()
            .Select(s => new Tag { Name = s });

        tagsToAdd.AddRange(newTags);

        _context.Tags.AddRange(tagsToAdd);
        await _context.SaveChangesAsync();
    }
}