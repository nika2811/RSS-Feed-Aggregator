using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSS_Feed_Aggregator.Db;
using RSS_Feed_Aggregator.Models;

namespace RSS_Feed_Aggregator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArticlesController : ControllerBase
{
    private readonly RssDbContext _context;

    public ArticlesController(RssDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Article>>> GetArticles(string tag)
    {
        var articles = await _context.Articles
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .ToListAsync();

        if (!string.IsNullOrEmpty(tag))
            articles = articles
                .Where(a => a.ArticleTags
                    .Any(at => at.Tag.Name.ToLower() == tag.ToLower()))
                .ToList();

        return Ok(articles);
    }
}