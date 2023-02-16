namespace RSS_Feed_Aggregator.Models;

public class Article
{
    public int Id { get; set; }
    public string Link { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string? Image { get; set; }
    public ICollection<ArticleTag> ArticleTags { get; set; }
    public DateTime PublicationDate { get; set; }
}