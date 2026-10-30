namespace HaberPortali.Services
{
    public interface IImageService
    {
        bool IsValid(IFormFile file, out string error);
        Task<string> SaveAsync(IFormFile file, string folder);
        void Delete(string? path);
    }

    // Haber görsellerini wwwroot/uploads altına kaydeder.
    public class ImageService : IImageService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 3 * 1024 * 1024; // 3 MB

        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public bool IsValid(IFormFile file, out string error)
        {
            error = string.Empty;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                error = "Sadece .jpg, .jpeg, .png ve .webp uzantılı görseller yüklenebilir.";
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                error = "Görsel boyutu en fazla 3 MB olabilir.";
                return false;
            }

            return true;
        }

        public async Task<string> SaveAsync(IFormFile file, string folder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var directory = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(directory);

            var fullPath = Path.Combine(directory, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{fileName}";
        }

        public void Delete(string? path)
        {
            // Yalnızca kullanıcıların yüklediği dosyalar silinir; örnek veri görsellerine dokunulmaz.
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/uploads/"))
            {
                return;
            }

            var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
