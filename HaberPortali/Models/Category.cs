using System.ComponentModel.DataAnnotations;

namespace HaberPortali.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        // Menüde ve listelerde sıralama için.
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<News> News { get; set; } = new List<News>();
    }
}
