using HaberPortali.Models;

namespace HaberPortali.ViewModels
{
    public class CommentListViewModel
    {
        public List<Comment> Items { get; set; } = new();

        // "pending", "approved" veya "all"
        public string Status { get; set; } = "pending";

        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
    }
}
