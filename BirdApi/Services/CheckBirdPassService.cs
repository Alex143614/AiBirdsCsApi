using BirdApi.Data;
using System;
using Microsoft.EntityFrameworkCore;

namespace BirdApi.Services;

public class CheckBirdPassService
{
    private readonly AppDbContext _database;//------------------------------------------для обращения к бд
    private readonly BlockchainService _blockchain;//-----------------------------------с файла BlockchainServices.cs в той же папке
    public CheckBirdPassService(AppDbContext context, BlockchainService blockchain)//---передаеться в program.cs
    {
        _database = context;//------для работы с бд
        _blockchain = blockchain;//-для работы с блокчейном
    }
    //---проверка наличия подписки\лимита по айди пользователя
    public async Task<bool> IsLimitExceededAsync(int userId)
    {
        //---находим пользователя по айди
        var user = await _database.Users.FindAsync(userId);
        //---если пользователя нету либо у него отсутсвует криптокошелек то
        if (user == null || string.IsNullOrEmpty(user.WalletAdress))
        {
            //---придеься проверить количества его запросов за сегодня, функция описана ниже
            return await CheckDailyLimitAsync(userId);
        }
        //---проверка наличия подписки через блокчейн в файле сервиса BlockchainServices.cs
        var hasSub = await _blockchain.HasSubscriptionAsync(user.WalletAdress);
        if(hasSub)//---функция HasSubscriptionAsync возыпвщает булевское значение так что проверяем
        {
            return false;
        }
        //---проверка количества запросов за сегодня, функция описана ниже
        return await CheckDailyLimitAsync(userId);
    }
    //---проверка эжедневного лимита пользователя на запросы пользователя по
    //---userId его айди 
    private async Task<bool> CheckDailyLimitAsync(int UserId)
    {
        //---получаем сегодняшнее время для проверки лимита на сегодня
        var today = DateTime.UtcNow.Date;
        //---получаем количество запросов пользователя на сегодня
        var todayFind = await _database.MainTables
            .Where(q => q.UserId == UserId && q.CreatedAt >= today)
            .CountAsync();
        if (todayFind >= 10){//---если их больше 10 то 
            return true;//---лимит исчерпан
        }
        else{
            return false;//--или еще нет
        }
    }
}