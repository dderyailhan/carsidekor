namespace CarsiDekor.Web.Models;

// Bir kategoriye ait ek galeri fotoğrafı (kapak fotoğrafı hariç, birden fazla olabilir)
public class CategoryImage
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string ImagePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
