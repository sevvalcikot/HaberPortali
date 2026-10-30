using System.ComponentModel.DataAnnotations;

namespace HaberPortali.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required, StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Yorumlar yönetici onayından sonra sitede görünür.
        public bool IsApproved { get; set; }

        public int NewsId { get; set; }
        public News News { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
