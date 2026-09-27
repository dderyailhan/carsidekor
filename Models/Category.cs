namespace CarsiDekor.Web.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;   // URL için: "yuzukluk"
    public int DisplayOrder { get; set; }

    // Kategori kartlarında/ana sayfada gösterilecek tek kapak fotoğrafı
    public string? CoverImagePath { get; set; }

    // Kapak dışında bu kategoriye ait ek galeri fotoğrafları (birden çok olabilir)
    public List<CategoryImage> Images { get; set; } = new();

    // Üst kategori: null ise ana kategori (Vitrinler, Bankolar, Nişler, Diğerleri gibi)
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public List<Category> Children { get; set; } = new();

    public List<Project> Projects { get; set; } = new();
}
