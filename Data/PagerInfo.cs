namespace CarsiDekor.Web.Data;

/// <summary>Sayfalama çubuğu için gereken bilgiler.</summary>
public record PagerInfo(int Page, int TotalPages, string? Q);
