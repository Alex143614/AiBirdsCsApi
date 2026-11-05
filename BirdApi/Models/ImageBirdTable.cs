namespace BirdApi.Models;

public class ImageBirdTable
{
    public int Id { get; set; }
    public double Confidence { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}