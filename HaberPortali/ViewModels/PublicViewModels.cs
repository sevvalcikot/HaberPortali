using HaberPortali.Models;

namespace HaberPortali.ViewModels
{
    public class HomeViewModel
    {
        public List<News> Headlines { get; set; } = new();
        public List<News> Latest { get; set; } = new();
        public List<News> MostRead { get; set; } = new();
        public List<CategorySection> Sections { get; set; } = new();
    }

    public class CategorySection
    {
        public Category Category { get; set; } = null!;
        public List<News> Items { get; set; } = new();
    }

    public class NewsDetailViewModel
    {
        public News News { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new();
        public List<News> Related { get; set; } = new();
        public List<News> MostRead { get; set; } = new();
    }

    // Kategori, etiket ve arama sonuç sayfaları için ortak model.
    public class NewsArchiveViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Kind { get; set; } = "category"; // category | tag | search
        public string? Slug { get; set; }
        public string? Query { get; set; }

        public List<News> Items { get; set; } = new();
        public List<News> MostRead { get; set; } = new();

        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
