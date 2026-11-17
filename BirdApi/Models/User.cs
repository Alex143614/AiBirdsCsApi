using System;
namespace BirdApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? WalletAdress {get; set;} 
    public bool HasSubscription {get; set;} = false;
    public DateTime? SubscriptionExpiresAt {get; set;}
}