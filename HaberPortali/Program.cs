using System.Globalization;
using HaberPortali.Data;
using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Tarih ve sayıların Türkçe biçimde gösterilmesi için.
var turkishCulture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = turkishCulture;
CultureInfo.DefaultThreadCurrentUICulture = turkishCulture;

builder.Services.AddControllersWithViews();

// ---- Veritabanı (SQL Server, Code-First) ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Repository deseni ----
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<INewsRepository, NewsRepository>();

// ---- Yardımcı servisler ----
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IImageService, ImageService>();

// ---- Cookie tabanlı oturum açma ve yetkilendirme ----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Account/Login";
        options.LogoutPath = "/Admin/Account/Logout";
        options.AccessDeniedPath = "/Admin/Account/AccessDenied";
        options.Cookie.Name = "HaberPortali.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Migration'ları uygula ve veritabanı boşsa örnek verileri ekle.
await DbInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Yönetim paneli (Areas/Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Okunabilir haber adresi: /haber/5/sehir-merkezine-yeni-bisiklet-yolu
app.MapControllerRoute(
    name: "newsDetail",
    pattern: "haber/{id:int}/{slug?}",
    defaults: new { controller = "News", action = "Detail" });

app.MapControllerRoute(
    name: "newsCategory",
    pattern: "kategori/{slug}",
    defaults: new { controller = "News", action = "ByCategory" });

app.MapControllerRoute(
    name: "newsTag",
    pattern: "etiket/{slug}",
    defaults: new { controller = "News", action = "ByTag" });

app.MapControllerRoute(
    name: "newsSearch",
    pattern: "ara",
    defaults: new { controller = "News", action = "Search" });

app.MapControllerRoute(
    name: "about",
    pattern: "hakkimizda",
    defaults: new { controller = "Home", action = "About" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
