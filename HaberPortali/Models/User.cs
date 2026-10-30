using System.ComponentModel.DataAnnotations;

namespace HaberPortali.Models
{
    // Sisteme giriş yapan kişiler (yönetici, editör, üye).
    // Ara sınavda cookie tabanlı giriş bu tablo üzerinden yapılır.
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Role { get; set; } = Roles.Member;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public ICollection<News> News { get; set; } = new List<News>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
