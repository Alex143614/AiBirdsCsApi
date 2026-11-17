namespace BirdApi.Models;
//---для авторизации или логина
public class AuthRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}