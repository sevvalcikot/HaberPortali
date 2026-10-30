using HaberPortali.Helpers;
using HaberPortali.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Data
{
    // Uygulama açılırken bekleyen migration'ları uygular ve veritabanı boşsa örnek veri ekler.
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            if (!db.Database.GetMigrations().Any())
            {
                logger.LogWarning("Henüz migration yok. Package Manager Console'da 'Add-Migration InitialCreate' ve 'Update-Database' komutlarını çalıştırın.");
                return;
            }

            await db.Database.MigrateAsync();

            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            await SeedAsync(db, hasher);
        }

        private static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> hasher)
        {
            if (await db.Users.AnyAsync())
            {
                return;
            }

            // ---- Kullanıcılar ----
            var admin = CreateUser("Site Yöneticisi", "admin@haberportali.com", Roles.Admin, "Admin123!", hasher);
            var editor = CreateUser("Ayşe Yılmaz", "editor@haberportali.com", Roles.Editor, "Editor123!", hasher);
            var editor2 = CreateUser("Mehmet Demir", "mehmet@haberportali.com", Roles.Editor, "Editor123!", hasher);
            var member = CreateUser("Zeynep Kaya", "uye@haberportali.com", Roles.Member, "Uye12345!", hasher);
            var member2 = CreateUser("Can Öztürk", "can@haberportali.com", Roles.Member, "Uye12345!", hasher);
            db.Users.AddRange(admin, editor, editor2, member, member2);

            // ---- Kategoriler ----
            var categories = new Dictionary<string, Category>
            {
                ["gundem"] = CreateCategory("Gündem", "Kentten ve ülkeden son gelişmeler", 1),
                ["ekonomi"] = CreateCategory("Ekonomi", "Piyasa, iş dünyası ve üretim haberleri", 2),
                ["spor"] = CreateCategory("Spor", "Maç sonuçları, transferler ve amatör spor", 3),
                ["teknoloji"] = CreateCategory("Teknoloji", "Bilim, yazılım ve yeni girişimler", 4),
                ["dunya"] = CreateCategory("Dünya", "Uluslararası gelişmeler", 5),
                ["saglik"] = CreateCategory("Sağlık", "Sağlıklı yaşam ve tıp dünyasından haberler", 6),
                ["kultur-sanat"] = CreateCategory("Kültür-Sanat", "Sergi, konser, tiyatro ve kitap", 7)
            };
            db.Categories.AddRange(categories.Values);

            await db.SaveChangesAsync();

            // ---- Haberler ----
            var seedNews = new List<(string Category, string Title, string Summary, string[] Paragraphs, string Tags, bool Headline, bool Breaking, int HoursAgo, int Views, User Author)>
            {
                ("gundem", "Şehir merkezine 18 kilometrelik yeni bisiklet yolu geliyor",
                    "Belediyenin hazırladığı ulaşım planı kapsamında şehir merkezini sahile bağlayacak kesintisiz bisiklet yolu ağı için çalışmalar başladı.",
                    new[] {
                        "Belediye meclisinde kabul edilen ulaşım planına göre şehir merkezi ile sahil şeridi arasında toplam 18 kilometrelik kesintisiz bir bisiklet yolu ağı oluşturulacak. İlk etapta ana caddelerdeki otopark alanlarının bir bölümü bisiklet şeridine dönüştürülecek.",
                        "Proje sorumluları, yolların trafikten fiziksel bariyerlerle ayrılacağını ve kavşaklarda bisikletliler için ayrı sinyalizasyon kullanılacağını açıkladı. Güzergâh üzerinde 12 noktada bisiklet park ve tamir istasyonu kurulması da planlanıyor.",
                        "Çalışmaların yaz aylarından önce tamamlanması hedefleniyor. Yetkililer, bisiklet kullanımının artmasıyla şehir içi trafikte ve hava kirliliğinde belirgin bir azalma beklediklerini belirtti."
                    }, "ulaşım, bisiklet, belediye", true, false, 2, 1240, editor),

                ("ekonomi", "Yerel üreticiler için kooperatif destek programı başladı",
                    "Küçük ölçekli üreticilerin ürünlerini aracısız satabilmesi için hazırlanan destek programına başvurular açıldı.",
                    new[] {
                        "Tarım ve hayvancılıkla uğraşan küçük üreticilerin bir araya gelerek kooperatif kurmasını teşvik eden destek programı başladı. Program kapsamında kooperatiflere ortak depo, soğuk zincir ve ambalaj ekipmanı için hibe verilecek.",
                        "Programın en dikkat çeken bölümü, üreticilerin ürünlerini doğrudan tüketiciye ulaştırabileceği çevrim içi satış platformu. Platform üzerinden yapılan satışlarda komisyon alınmayacak.",
                        "Başvuruların ay sonuna kadar süreceği, değerlendirme sonuçlarının ise iki ay içinde açıklanacağı bildirildi. Uzmanlar, aracısız satışın hem üretici gelirini artıracağını hem de tüketici fiyatlarını düşüreceğini vurguluyor."
                    }, "kooperatif, tarım, girişimcilik", true, false, 5, 860, editor2),

                ("spor", "Gençlik ligi finalinde kazanan son dakikada belli oldu",
                    "Sezonun en çok izlenen gençlik ligi finalinde ev sahibi takım, uzatma dakikalarında bulduğu golle şampiyonluğa ulaştı.",
                    new[] {
                        "Gençlik ligi finali, tribünleri dolduran seyircilere unutulmaz bir akşam yaşattı. İlk yarıyı golsüz kapatan takımlar, ikinci yarıda karşılıklı gollerle skoru eşitledi.",
                        "Maçın kaderini uzatma dakikalarında sahneye çıkan genç forvet belirledi. Ceza sahası dışından attığı şutla fileleri havalandıran oyuncu, takımına sezonun ilk kupasını kazandırdı.",
                        "Teknik direktör maç sonrası yaptığı açıklamada, altyapıdan yetişen oyuncuların sezon boyunca gösterdiği kararlılığın bu sonucu getirdiğini söyledi."
                    }, "futbol, gençlik ligi, final", true, true, 1, 2310, editor2),

                ("teknoloji", "Üniversite öğrencilerinden enerji tasarrufu sağlayan akıllı priz",
                    "Bilgisayar programcılığı öğrencilerinin geliştirdiği akıllı priz, bekleme modundaki cihazların elektrik tüketimini otomatik olarak kesiyor.",
                    new[] {
                        "Meslek yüksekokulunda okuyan bir grup öğrenci, evlerde bekleme modunda unutulan cihazların harcadığı enerjiyi azaltmak için akıllı bir priz geliştirdi. Priz, bağlı cihazın tüketimini ölçerek kullanılmadığını anladığında elektriği kendiliğinden kesiyor.",
                        "Mobil uygulama üzerinden günlük ve aylık tüketim grafiklerini görmek de mümkün. Öğrenciler, prototipi bir ay boyunca kendi evlerinde test ettiklerini ve elektrik faturalarında yüzde 8'e varan düşüş gördüklerini anlattı.",
                        "Proje, bölgesel girişimcilik yarışmasında birincilik ödülü aldı. Ekip şimdi ürünü seri üretime hazırlamak için yatırımcı arıyor."
                    }, "girişim, enerji, öğrenci projesi, nesnelerin interneti", true, false, 8, 1530, editor),

                ("dunya", "Akdeniz ülkeleri deniz kirliliğine karşı ortak eylem planında uzlaştı",
                    "Bölge ülkelerinin çevre bakanları, plastik atıkların denize ulaşmasını önlemeye yönelik ortak bir eylem planı imzaladı.",
                    new[] {
                        "Akdeniz'e kıyısı olan ülkelerin çevre bakanları, iki gün süren toplantının ardından deniz kirliliğiyle mücadelede ortak bir eylem planı üzerinde uzlaştı. Plan, özellikle tek kullanımlık plastiklerin azaltılmasını ve atık su arıtma kapasitesinin artırılmasını öngörüyor.",
                        "Anlaşmaya göre ülkeler her yıl kıyı temizliği verilerini ortak bir veri tabanında paylaşacak. Böylece kirliliğin kaynağı daha hızlı tespit edilebilecek.",
                        "Çevre örgütleri anlaşmayı olumlu bir adım olarak değerlendirse de hedeflerin bağlayıcı yaptırımlarla desteklenmesi gerektiğini hatırlattı."
                    }, "çevre, deniz kirliliği, akdeniz", false, false, 14, 640, editor),

                ("saglik", "Uzmanlardan sonbahar aylarında bağışıklığı güçlendirme önerileri",
                    "Havaların soğumasıyla artan mevsimsel hastalıklara karşı uzmanlar düzenli uyku, dengeli beslenme ve hareketli yaşamın önemine dikkat çekiyor.",
                    new[] {
                        "Sonbahar aylarında soğuk algınlığı ve grip vakalarında artış görülüyor. Uzmanlar, bağışıklık sistemini desteklemenin en etkili yolunun günlük alışkanlıklardan geçtiğini belirtiyor.",
                        "Önerilerin başında her gün aynı saatte yatıp kalkmak, mevsim sebze ve meyvelerini sofradan eksik etmemek ve gün içinde en az yarım saat yürüyüş yapmak geliyor. Kapalı ortamların sık sık havalandırılması da bulaş riskini azaltıyor.",
                        "Uzmanlar, risk grubundaki kişilerin aşı takvimleri için aile hekimlerine danışmalarını tavsiye ediyor."
                    }, "sağlıklı yaşam, bağışıklık, grip", false, false, 20, 980, editor2),

                ("kultur-sanat", "Antik tiyatroda açık hava konserleri sezonu kapattı",
                    "Yaz boyunca binlerce müzikseveri ağırlayan açık hava konser dizisi, senfoni orkestrasının verdiği kapanış konseriyle sona erdi.",
                    new[] {
                        "Tarihi antik tiyatroda düzenlenen açık hava konserleri bu yıl da yoğun ilgi gördü. Sezon boyunca klasik müzikten caza uzanan geniş bir programda 20'den fazla konser gerçekleştirildi.",
                        "Kapanış gecesinde sahne alan senfoni orkestrası, bestecilerin en sevilen eserlerinden oluşan bir program sundu. Konser boyunca tribünleri dolduran izleyiciler sanatçıları uzun süre ayakta alkışladı.",
                        "Organizasyon komitesi, gelecek sezonda gençlere yönelik ücretsiz konserlerin sayısını artırmayı planladıklarını açıkladı."
                    }, "konser, müzik, kültür", false, false, 30, 520, editor),

                ("gundem", "Sahil şeridinde gönüllülerle büyük temizlik kampanyası",
                    "Hafta sonu düzenlenen temizlik kampanyasına katılan yüzlerce gönüllü, sahil şeridinden tonlarca atık topladı.",
                    new[] {
                        "Sivil toplum kuruluşlarının çağrısıyla bir araya gelen yüzlerce gönüllü, hafta sonu sahil şeridinde kapsamlı bir temizlik çalışması yaptı. Sabah saatlerinde başlayan etkinliğe öğrenciler, aileler ve emekliler katıldı.",
                        "Toplanan atıkların büyük bölümünü plastik şişeler, sigara izmaritleri ve ambalaj atıkları oluşturdu. Atıklar türlerine göre ayrıştırılarak geri dönüşüm tesislerine gönderildi.",
                        "Organizatörler, kampanyanın her ay tekrarlanacağını ve katılmak isteyenlerin internet sitesi üzerinden kayıt olabileceğini duyurdu."
                    }, "çevre, gönüllülük, sahil", false, true, 3, 1720, editor),

                ("teknoloji", "Yapay zekâ destekli tarım uygulaması çiftçilerin işini kolaylaştırıyor",
                    "Telefon kamerasıyla çekilen yaprak fotoğrafından bitki hastalıklarını tespit eden uygulama, kısa sürede binlerce kullanıcıya ulaştı.",
                    new[] {
                        "Yerli bir yazılım ekibinin geliştirdiği tarım uygulaması, çiftçilerin bitki hastalıklarını erken fark etmesini sağlıyor. Kullanıcılar şüphelendikleri yaprağın fotoğrafını çektiğinde uygulama birkaç saniye içinde olası hastalığı ve önerilen müdahaleyi gösteriyor.",
                        "Uygulama internet bağlantısı olmayan tarlalarda da çalışabiliyor. Sonuçlar bağlantı sağlandığında ziraat mühendislerine iletilerek ikinci bir kontrolden geçiriliyor.",
                        "Geliştiriciler, uygulamanın ilaç kullanımını azaltarak hem maliyeti düşürdüğünü hem de çevreye verilen zararı azalttığını belirtiyor."
                    }, "yapay zekâ, tarım, mobil uygulama", false, false, 26, 1105, editor2),

                ("ekonomi", "Turizm sezonu beklentilerin üzerinde tamamlandı",
                    "Sektör temsilcileri, uzayan sezon ve artan yabancı ziyaretçi sayısıyla bu yılın son on yılın en verimli dönemlerinden biri olduğunu açıkladı.",
                    new[] {
                        "Turizm sektörü temsilcileri, sezonun beklentilerin üzerinde kapandığını açıkladı. Konaklama tesislerindeki doluluk oranları özellikle eylül ve ekim aylarında geçen yılın belirgin şekilde üzerinde seyretti.",
                        "Sezonun uzamasında kültür turizmine ve doğa yürüyüşü rotalarına artan ilginin etkili olduğu belirtildi. Küçük işletmeler de bu yoğunluktan payını aldı.",
                        "Sektör temsilcileri, gelecek yıl için personel eğitimine ve sürdürülebilir turizm uygulamalarına daha fazla yatırım yapılması gerektiğini vurguladı."
                    }, "turizm, ekonomi, sezon", false, true, 6, 890, editor),

                ("spor", "Kent maratonuna rekor katılım bekleniyor",
                    "Bu yıl dördüncüsü düzenlenecek kent maratonuna şimdiden 6 binden fazla koşucu kayıt yaptırdı.",
                    new[] {
                        "Kent maratonu için kayıtlar rekor seviyeye ulaştı. Organizasyon komitesi, 42 kilometrelik ana parkurun yanı sıra 10 kilometrelik halk koşusu ve engelli sporcular için ayrı bir kategori düzenleneceğini duyurdu.",
                        "Parkur boyunca 15 su ve sağlık istasyonu kurulacak. Yarış günü bazı caddeler sabah saatlerinde trafiğe kapatılacak.",
                        "Koşudan elde edilecek gelirin bir bölümü, amatör spor kulüplerine malzeme desteği olarak aktarılacak."
                    }, "maraton, koşu, amatör spor", false, false, 40, 610, editor2),

                ("saglik", "Hastanelerde yeni randevu sistemi devreye giriyor",
                    "Yenilenen randevu sistemiyle hastalar, iptal edilen randevulardan anlık bildirim alarak daha erken muayene olabilecek.",
                    new[] {
                        "Kamu hastanelerinde kullanılan randevu sistemi yenileniyor. Yeni sistemde iptal edilen randevular bekleme listesindeki hastalara anlık bildirim olarak gönderilecek.",
                        "Hastalar bildirim geldikten sonra 15 dakika içinde onay verirse randevu kendilerine tanımlanacak. Böylece boş kalan muayene saatlerinin azalması bekleniyor.",
                        "Sistemin önümüzdeki ay pilot hastanelerde denenmesi, ardından tüm hastanelere yaygınlaştırılması planlanıyor."
                    }, "hastane, randevu, dijital dönüşüm", false, false, 52, 745, editor)
            };

            var newsEntities = new List<News>();
            var imageIndex = 1;
            foreach (var item in seedNews)
            {
                var news = new News
                {
                    Title = item.Title,
                    Slug = SlugHelper.ToSlug(item.Title),
                    Summary = item.Summary,
                    Content = string.Join("\n\n", item.Paragraphs),
                    ImagePath = $"/images/news/seed-{imageIndex:00}.jpg",
                    CategoryId = categories[item.Category].Id,
                    AuthorId = item.Author.Id,
                    IsHeadline = item.Headline,
                    IsBreaking = item.Breaking,
                    IsActive = true,
                    PublishedAt = DateTime.Now.AddHours(-item.HoursAgo),
                    ViewCount = item.Views
                };
                newsEntities.Add(news);
                db.News.Add(news);
                imageIndex++;
            }

            await db.SaveChangesAsync();

            // ---- Etiketler ----
            var tagCache = new Dictionary<string, Tag>(StringComparer.CurrentCultureIgnoreCase);
            for (var i = 0; i < seedNews.Count; i++)
            {
                var tagNames = seedNews[i].Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var name in tagNames)
                {
                    if (!tagCache.TryGetValue(name, out var tag))
                    {
                        tag = new Tag { Name = name, Slug = SlugHelper.ToSlug(name) };
                        tagCache[name] = tag;
                        db.Tags.Add(tag);
                    }
                    db.NewsTags.Add(new NewsTag { News = newsEntities[i], Tag = tag });
                }
            }

            // ---- Yorumlar ----
            db.Comments.AddRange(
                new Comment { NewsId = newsEntities[0].Id, UserId = member.Id, Text = "Çok güzel bir proje, bisikletle işe gitmek artık daha güvenli olacak.", IsApproved = true, CreatedAt = DateTime.Now.AddMinutes(-90) },
                new Comment { NewsId = newsEntities[0].Id, UserId = member2.Id, Text = "Otopark sorunu ne olacak, onu da düşünmek lazım.", IsApproved = true, CreatedAt = DateTime.Now.AddMinutes(-60) },
                new Comment { NewsId = newsEntities[2].Id, UserId = member2.Id, Text = "Tribündeydim, son dakika golü harikaydı!", IsApproved = true, CreatedAt = DateTime.Now.AddMinutes(-30) },
                new Comment { NewsId = newsEntities[3].Id, UserId = member.Id, Text = "Öğrencileri tebrik ediyorum, ürün satışa çıkınca ilk ben alırım.", IsApproved = false, CreatedAt = DateTime.Now.AddMinutes(-20) },
                new Comment { NewsId = newsEntities[7].Id, UserId = member.Id, Text = "Bir sonraki kampanyaya ben de katılmak istiyorum.", IsApproved = false, CreatedAt = DateTime.Now.AddMinutes(-10) },
                new Comment { NewsId = newsEntities[9].Id, UserId = member2.Id, Text = "Esnaf olarak bu sezon gerçekten yoğundu.", IsApproved = false, CreatedAt = DateTime.Now.AddMinutes(-5) }
            );

            await db.SaveChangesAsync();
        }

        private static User CreateUser(string fullName, string email, string role, string password, IPasswordHasher<User> hasher)
        {
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30)
            };
            user.PasswordHash = hasher.HashPassword(user, password);
            return user;
        }

        private static Category CreateCategory(string name, string description, int order)
        {
            return new Category
            {
                Name = name,
                Slug = SlugHelper.ToSlug(name),
                Description = description,
                DisplayOrder = order,
                IsActive = true
            };
        }
    }
}
