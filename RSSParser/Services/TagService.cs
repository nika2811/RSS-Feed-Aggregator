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

    // public async Task AddTagsToArticle(Article article)
    // {
    //     var tagsToAdd = new List<Tag>();
    //     var articleText = article.Title + " " + article.Description;
    //     var existingTags = _context.Tags.ToList();
    //
    //     foreach (var existingTag in existingTags)
    //     {
    //         if (articleText.Contains(existingTag.Name))
    //
    //             article?.ArticleTags?.Add(new ArticleTag { Article = article, Tag = existingTag });
    //     }
    //
    //     var newTags = articleText.Split(" ")
    //         .Where(s => !existingTags.Any(et => et.Name.Contains(s)))
    //         .Distinct()
    //         .Select(s => new Tag { Name = s });
    //
    //     tagsToAdd.AddRange(newTags);
    //
    //     _context.Tags.AddRange(tagsToAdd);
    //     await _context.SaveChangesAsync();
    // }
    public async Task AddTagsToArticle(Article article)
    {
        var articleText = article.Title + " " + article.Description;
        var existingTags = await _context.Tags.ToListAsync();

        var existingTagNames = existingTags.Select(tag => tag.Name).ToHashSet();
        var newTagNames = articleText.Split(" ")
            .Where(s => !existingTagNames.Contains(s))
            .Distinct();

        var tagsToAdd = newTagNames.Select(s => new Tag { Name = s }).ToList();

        // Add new tags to the database
        await _context.Tags.AddRangeAsync(tagsToAdd);
        await _context.SaveChangesAsync();

        // Get all tags again (including the new ones that were just added)
        existingTags = await _context.Tags.ToListAsync();

        // Get the existing tags that are mentioned in the article text
        var existingTagsInArticle = existingTags.Where(tag => articleText.Contains(tag.Name)).ToList();

        // Create ArticleTag entities for the existing tags
        var articleTags = existingTagsInArticle.Select(tag => new ArticleTag { Article = article, Tag = tag });

        // Add new tags to the article and create ArticleTag entities for them
        foreach (var tag in tagsToAdd)
        {
            // article?.ArticleTags?.Add(new ArticleTag { Article = article, Tag = tag });
            var articleTag = new ArticleTag { Article = article, Tag = tag };
            article.ArticleTags.Add(articleTag);
        }

        await _context.SaveChangesAsync();
    }
}