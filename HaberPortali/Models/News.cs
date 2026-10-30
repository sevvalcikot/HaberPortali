using System.ComponentModel.DataAnnotations;

namespace HaberPortali.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        // Adres çubuğunda okunabilir bağlantı için (ör. /haber/5/yeni-kutuphane-acildi).
        [Required, StringLength(220)]
        public string Slug { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Summary { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(300)]
        public string? ImagePath { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int ViewCount { get; set; }

        // Ana sayfadaki manşet alanında gösterilir.
        public bool IsHeadline { get; set; }

        // "Son dakika" bandında gösterilir.
        public bool IsBreaking { get; set; }

        // Pasif haberler sitede görünmez.
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<NewsTag> NewsTags { get; set; } = new List<NewsTag>();
    }
}
