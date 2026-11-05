namespace BirdApi.Models;

public class MainTable
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = string.Empty;
    public int IdTable { get; set; }
    public string? Species { get; set; }
    public string? InputQuery { get; set; }
}