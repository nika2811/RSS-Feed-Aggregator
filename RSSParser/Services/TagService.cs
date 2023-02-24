using System.ServiceModel.Syndication;
using Microsoft.EntityFrameworkCore;
using RSS_Feed_Aggregator.Db;
using RSS_Feed_Aggregator.Models;

namespace RSSParser.Services;

public class TagService
{
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
