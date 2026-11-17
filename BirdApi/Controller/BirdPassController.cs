using BirdApi.Data;
using BirdApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BirdApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BirdPassController : ControllerBase
{
    private readonly AppDbContext _database;
    private readonly BlockchainService _blockchain;

    public BirdPassController(AppDbContext database, BlockchainService blockchain)
    {
        _database = database;
        _blockchain = blockchain;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // 2. Привязать кошелек
    [HttpPost("link-wallet")]
    public async Task<IActionResult> LinkWallet([FromBody] string walletAddress)
    {
        var user = await _database.Users.FindAsync(UserId);//------------------------------подключение в бд и поиск пользователя по айди
        if (user == null) return NotFound("Пользователь не найден.");
        user.WalletAdress = walletAddress;//----------------------------------------------запись в бд
        await _database.SaveChangesAsync();//----------------------------------------------сохранение изменений в бд

        return Ok(new{Message="Кошелек успешно привязан.",Wallet = walletAddress });//----возврат результата
    }
    [HttpGet("status")]//-----------------------------------------------------------------вывод статуса пользователя
    public async Task<IActionResult> GetStatus()
    {
        var user = await _database.Users.FindAsync(UserId);//------------------------------поиск пользователя в бд пр айди
        if (user == null) return NotFound("Пользователь не найден.");
        if (string.IsNullOrEmpty(user.WalletAdress))//------------------------------------проверка наличия криптокошелька
            return BadRequest("Кошелек не привязан.");

        var hasSub = await _blockchain.HasSubscriptionAsync(user.WalletAdress);//---------проверка наличия подписки через функцию в файле сервисе
        var endDate = await _blockchain.GetSubscriptionEndDateAsync(user.WalletAdress);//-получение окончания подписки через функцию в файле сервесе

        return Ok(new
        {
            HasSubscription = hasSub,
            Wallet = user.WalletAdress,
            ExpiresAt = endDate
        });//-----------------------------------------------------------------------------возврат наличия или отсутсвие подписки, сам адресс кошелька, окончание подписки
    }
}