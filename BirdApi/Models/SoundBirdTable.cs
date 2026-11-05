namespace BirdApi.Models;

public class SoundBirdTable
{
    public int Id { get; set; }
    public double Confidence { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
}