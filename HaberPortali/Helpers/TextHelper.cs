namespace HaberPortali.Helpers
{
    public static class TextHelper
    {
        // Haber metnini boş satırlardan bölerek paragraflara ayırır.
        // Metin HTML olarak değil düz yazı olarak saklandığı için sayfada güvenle gösterilir.
        public static IEnumerable<string> ToParagraphs(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<string>();
            }

            return text.Replace("\r\n", "\n")
                       .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public static string Shorten(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text ?? string.Empty;
            }

            return text.Substring(0, maxLength).TrimEnd() + "…";
        }

        // "3 saat önce" gibi göreli zaman ifadesi.
        public static string TimeAgo(DateTime date)
        {
            var span = DateTime.Now - date;

            if (span.TotalMinutes < 1) return "az önce";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} dakika önce";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} saat önce";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} gün önce";

            return date.ToString("d MMMM yyyy");
        }
    }
}
