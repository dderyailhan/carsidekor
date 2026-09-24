namespace CarsiDekor.Web.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;   // URL için: "yuzukluk"
    public int DisplayOrder { get; set; }

    public List<Project> Projects { get; set; } = new();
}