using System.ComponentModel.DataAnnotations;

namespace HaberPortali.ViewModels
{
    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad soyad 3 ile 100 karakter arasında olmalıdır.")]
        [Display(Name = "Ad soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [StringLength(150)]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol seçin.")]
        [Display(Name = "Rol")]
        public string Role { get; set; } = HaberPortali.Models.Roles.Editor;

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        // Yeni kullanıcıda zorunlu, düzenlemede boş bırakılırsa şifre değişmez.
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre (tekrar)")]
        public string? ConfirmPassword { get; set; }
    }
}
