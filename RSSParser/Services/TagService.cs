using Microsoft.EntityFrameworkCore;
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
        await using var context = new RssDbContext(CreateDbContextOptions());
        var tagsToAdd = new List<Tag>();
        var articleText = article.Title + " " + article.Description;
        var existingTags = context.Tags.ToList();

        foreach (var existingTag in existingTags.Where(existingTag => articleText.Contains(existingTag.Name)))
            article?.ArticleTags?.Add(new ArticleTag
                { ArticleId = article.Id, Tag = existingTag, TagId = existingTag.Id, Article = article });

        var newTags = articleText.Split(" ")
            .Where(s => !existingTags.Any(et => et.Name.Contains(s)))
            .Distinct()
            .Select(s => new Tag { Name = s });

        tagsToAdd.AddRange(newTags);

        context.Tags.AddRange(tagsToAdd);
        await context.SaveChangesAsync();
    }


    private static DbContextOptions<RssDbContext> CreateDbContextOptions()
    {
        var builder = new DbContextOptionsBuilder<RssDbContext>();
        builder.UseSqlServer("Server=localhost;Database=RSS-Feed_db;User Id=nika;Password=123;Encrypt=false;");
        return builder.Options;
    }
}