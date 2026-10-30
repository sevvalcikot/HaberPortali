using System.Text;
using System.Text.RegularExpressions;

namespace HaberPortali.Helpers
{
    // "Şehirde Yeni Kütüphane Açıldı" -> "sehirde-yeni-kutuphane-acildi"
    public static class SlugHelper
    {
        public static string ToSlug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "icerik";
            }

            var sb = new StringBuilder(text.Trim().Length);
            foreach (var ch in text.Trim())
            {
                sb.Append(ch switch
                {
                    'ç' or 'Ç' => "c",
                    'ğ' or 'Ğ' => "g",
                    'ı' or 'I' or 'İ' or 'i' => "i",
                    'ö' or 'Ö' => "o",
                    'ş' or 'Ş' => "s",
                    'ü' or 'Ü' => "u",
                    _ => char.ToLowerInvariant(ch).ToString()
                });
            }

            var slug = sb.ToString();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

            if (slug.Length > 200)
            {
                slug = slug.Substring(0, 200).Trim('-');
            }

            return string.IsNullOrEmpty(slug) ? "icerik" : slug;
        }
    }
}
