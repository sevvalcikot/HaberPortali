using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaberPortali.ViewModels
{
    // Haber ekleme ve düzenleme formu. Entity yerine ViewModel kullanılarak
    // formda yalnızca gereken alanlar gösterilir ve dosya yükleme alanı eklenir.
    public class NewsFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Başlık 10 ile 200 karakter arasında olmalıdır.")]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Özet zorunludur.")]
        [StringLength(500, MinimumLength = 20, ErrorMessage = "Özet 20 ile 500 karakter arasında olmalıdır.")]
        [Display(Name = "Özet (spot)")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Haber metni zorunludur.")]
        [MinLength(50, ErrorMessage = "Haber metni en az 50 karakter olmalıdır.")]
        [Display(Name = "Haber metni")]
        public string Content { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Kategori seçin.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Display(Name = "Yayın tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime PublishedAt { get; set; } = DateTime.Now;

        [Display(Name = "Etiketler")]
        [StringLength(300)]
        public string? TagsText { get; set; }

        [Display(Name = "Manşette göster")]
        public bool IsHeadline { get; set; }

        [Display(Name = "Son dakika")]
        public bool IsBreaking { get; set; }

        [Display(Name = "Yayında")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Görsel")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }

        [Display(Name = "Mevcut görseli kaldır")]
        public bool RemoveImage { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
