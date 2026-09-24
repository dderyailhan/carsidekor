namespace CarsiDekor.Web.Models;

/// <summary>Yönetim paneline giriş yapabilen kullanıcı.</summary>
public class AdminUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    // Şifrenin kendisi değil, hash'i saklanır (bkz. PasswordService).
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
