namespace HaberPortali.Models
{
    // Rol adları tek yerde tutulur, böylece yazım hatası yapılmaz.
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Editor = "Editor";
        public const string Member = "Member";

        // [Authorize(Roles = ...)] içinde virgülle ayrılmış roller "veya" anlamına gelir.
        public const string AdminOrEditor = "Admin,Editor";

        public static readonly string[] All = { Admin, Editor, Member };

        public static string DisplayName(string role) => role switch
        {
            Admin => "Yönetici",
            Editor => "Editör",
            Member => "Üye",
            _ => role
        };
    }
}
