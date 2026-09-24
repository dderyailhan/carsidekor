namespace CarsiDekor.Web.Data;

/// <summary>
/// Veritabanında tarihler UTC saklanır. Ekranda Türkiye saatiyle göstermek için kullanılır.
/// </summary>
public static class TurkeyTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Turkey Standard Time" : "Europe/Istanbul");

    public static DateTime FromUtc(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

    public static string Format(DateTime utc) => FromUtc(utc).ToString("dd.MM.yyyy HH:mm");
}
