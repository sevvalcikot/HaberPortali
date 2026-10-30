namespace HaberPortali.Models
{
    // Haber ile etiket arasındaki çoka-çok ilişkinin ara tablosu.
    // Birincil anahtar (NewsId, TagId) ikilisidir; AppDbContext içinde tanımlanır.
    public class NewsTag
    {
        public int NewsId { get; set; }
        public News News { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
