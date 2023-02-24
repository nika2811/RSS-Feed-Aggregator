using System.ServiceModel.Syndication;
using Microsoft.EntityFrameworkCore;
using RSS_Feed_Aggregator.Db;
using RSS_Feed_Aggregator.Models;

namespace RSSParser.Services;

public class TagService
{
    public TagService()
    {
    }

        public async Task AddCategoriesToArticle(List<SyndicationCategory> categories, Article article)
        {
            try
            {
                await using var context = new RssDbContext(CreateDbContextOptions());
    
                var categoriesToAdd = new List<Tag>();
    
                foreach (var category in categories)
                {
                    var categoryName = category.Name;
                    var existingCategory = await context.Tags.FirstOrDefaultAsync(c => c.Name == categoryName);
    
                    if (existingCategory != null)
                    {
                        article.ArticleTags.Add(new ArticleTag { Article = article, Tag = existingCategory });
                    }
                    else
                    {
                        var newCategory = new Tag { Name = categoryName };
                        categoriesToAdd.Add(newCategory);
                        article.ArticleTags.Add(new ArticleTag
                            { Article = article, Tag = newCategory, ArticleId = article.Id, TagId = newCategory.Id });
                    }
                }
                await context.Articles.AddAsync(article);
                await context.Tags.AddRangeAsync(categoriesToAdd);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    
    
        private static DbContextOptions<RssDbContext> CreateDbContextOptions()
        {
            var builder = new DbContextOptionsBuilder<RssDbContext>();
            builder.UseSqlServer("Server=localhost;Database=RSS-Feed_db;User Id=nika;Password=123;Encrypt=false;");
            return builder.Options;
        }
    }

    // public async Task AddCategoriesToArticle(List<SyndicationCategory> categories, Article article)
    // {
    //     await using var dbContext = new RssDbContext(CreateDbContextOptions());
    //     var existingTags = await dbContext.Tags.ToListAsync();
    //     var existingTagNames = existingTags.Select(t => t.Name).ToList();
    //
    //     var newTags = categories
    //         .Where(c => !existingTagNames.Contains(c.Name))
    //         .Select(c => new Tag { Name = c.Name })
    //         .ToList();
    //
    //     var articleTags = new List<ArticleTag>();
    //     var tagsToAdd = new List<Tag>();
    //
    //     // Check article title and description for existing tags
    //     var tagMatches = existingTags
    //         .Where(t =>
    //             (article.Title?.Contains(t.Name) ?? false) ||
    //             (article.Description?.Contains(t.Name) ?? false))
    //         .ToList();
    //
    //     foreach (var match in tagMatches)
    //     {
    //         // Add matching tags to article
    //         if (articleTags.Count < 5 && article.ArticleTags.All(at => at.TagId != match.Id))
    //         {
    //             articleTags.Add(new ArticleTag { ArticleId = article.Id, TagId = match.Id });
    //         }
    //     }
    //
    //     // Add new tags to database and article
    //     foreach (var tag in newTags)
    //     {
    //         if (articleTags.Count < 5)
    //         {
    //             var existingTag = existingTags.FirstOrDefault(t => t.Name == tag.Name);
    //
    //             if (existingTag != null)
    //             {
    //                 articleTags.Add(new ArticleTag { ArticleId = article.Id, TagId = existingTag.Id });
    //             }
    //             else
    //             {
    //                 tagsToAdd.Add(tag);
    //                 articleTags.Add(new ArticleTag { ArticleId = article.Id, Tag = tag });
    //             }
    //         }
    //     }
    //
    //     // Add new tags to database
    //     await dbContext.Tags.AddRangeAsync(tagsToAdd);
    //
    //     // Add article tags to database
    //     await dbContext.ArticleTags.AddRangeAsync(articleTags);
    //
    //     await dbContext.SaveChangesAsync();
    // }
    //
    //
    // private static DbContextOptions<RssDbContext> CreateDbContextOptions()
    // {
    //     var builder = new DbContextOptionsBuilder<RssDbContext>();
    //     builder.UseSqlServer("Server=localhost;Database=RSS-Feed_db;User Id=nika;Password=123;Encrypt=false;");
    //     return builder.Options;
    // }
// }
