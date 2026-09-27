namespace CarsiDekor.Web.Services;

/// <summary>
/// Yüklenen fotoğrafları doğrular, wwwroot/uploads/{folder} klasörüne kaydeder ve siler.
/// "folder" belirtilmezse projeler için kullanılan "projects" klasörü varsayılır;
/// kategori fotoğrafları "categories" klasörüne kaydedilir.
/// </summary>
public class ImageStorage
{
    private const long MaxBytes = 8 * 1024 * 1024;   // 8 MB
    private const string DefaultFolder = "projects";
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly string _webRoot;
    private readonly ILogger<ImageStorage> _logger;

    public ImageStorage(IWebHostEnvironment env, ILogger<ImageStorage> logger)
    {
        _webRoot = env.WebRootPath;
        _logger = logger;
    }

    /// <summary>Dosya uygunsa null, değilse kullanıcıya gösterilecek hata mesajını döner.</summary>
    public async Task<string?> ValidateAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            return $"\"{file.FileName}\" boş bir dosya.";
        }

        if (file.Length > MaxBytes)
        {
            return $"\"{file.FileName}\" 8 MB'tan büyük. Daha küçük bir fotoğraf seçin.";
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return $"\"{file.FileName}\" desteklenmiyor. JPG, PNG veya WEBP yükleyin.";
        }

        // Uzantıya güvenmiyoruz: dosyanın ilk baytları gerçekten bir fotoğrafa ait mi?
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        if (!LooksLikeImage(header, read))
        {
            return $"\"{file.FileName}\" geçerli bir fotoğraf değil.";
        }

        return null;
    }

    /// <summary>Fotoğrafı rastgele bir adla kaydeder, sitede kullanılacak adresi döner.</summary>
    public async Task<string> SaveAsync(IFormFile file, string folder = DefaultFolder)
    {
        var dir = Path.Combine(_webRoot, "uploads", folder);
        Directory.CreateDirectory(dir);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var name = $"{Guid.NewGuid():N}{ext}";

        await using var target = File.Create(Path.Combine(dir, name));
        await file.CopyToAsync(target);

        return $"/uploads/{folder}/{name}";
    }

    /// <summary>Sadece bizim yüklediğimiz dosyaları siler. Dış adresler (örnek görseller) yok sayılır.</summary>
    public void Delete(string? path)
    {
        if (string.IsNullOrEmpty(path) || !path.StartsWith("/uploads/", StringComparison.Ordinal))
        {
            return;
        }

        var fullPath = Path.Combine(_webRoot, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Fotoğraf silinemedi: {Path}", fullPath);
        }
    }

    private static bool LooksLikeImage(byte[] h, int length)
    {
        if (length < 12) return false;

        var isJpeg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        var isPng = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
        var isWebp = h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                  && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';

        return isJpeg || isPng || isWebp;
    }
}
