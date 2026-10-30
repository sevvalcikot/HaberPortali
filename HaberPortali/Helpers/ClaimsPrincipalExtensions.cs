using System.Security.Claims;

namespace HaberPortali.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        // Giriş yapan kullanıcının veritabanındaki Id'si (cookie içindeki NameIdentifier claim'i).
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        }
    }
}
