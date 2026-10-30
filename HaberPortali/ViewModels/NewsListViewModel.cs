using HaberPortali.Models;

namespace HaberPortali.ViewModels
{
    public class NewsListViewModel
    {
        public List<News> Items { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public string? Search { get; set; }
        public int? CategoryId { get; set; }

        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
