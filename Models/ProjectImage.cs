namespace CarsiDekor.Web.Models;

public class ProjectImage
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}