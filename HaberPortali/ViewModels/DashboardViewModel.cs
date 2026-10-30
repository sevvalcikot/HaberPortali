using HaberPortali.Models;

namespace HaberPortali.ViewModels
{
    public class DashboardViewModel
    {
        public int NewsCount { get; set; }
        public int ActiveNewsCount { get; set; }
        public int CategoryCount { get; set; }
        public int TagCount { get; set; }
        public int UserCount { get; set; }
        public int PendingCommentCount { get; set; }
        public int TotalViews { get; set; }

        public List<News> LatestNews { get; set; } = new();
        public List<News> MostRead { get; set; } = new();
        public List<Comment> PendingComments { get; set; } = new();
        public List<CategoryStat> CategoryStats { get; set; } = new();
    }

    public class CategoryStat
    {
        public string Name { get; set; } = string.Empty;
        public int NewsCount { get; set; }
    }
}
