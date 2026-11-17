using BirdApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BirdApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly AppDbContext _database;//-----------для работы с бд

    public HistoryController(AppDbContext database)
    {
        _database = database;
    }
    //---получаем айди пользователя
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var allSearch = await _database.MainTables//-----находим все ементы таблицы с айди пользователя который сделала запрос
            .Where(m => m.UserId == UserId)//------------фильтр по айди пользователя
            .OrderByDescending(m => m.CreatedAt)//-------фильтр по дате для красоты
            .ToListAsync();
        var result = new List<object>();//---------------контейнер для списка результата
        foreach (var search in allSearch)
        {
            object? details = null;//--------------------
            switch (search.Type)
            {
                case "image":
                    details = await _database.ImageBirdTables.FindAsync(search.IdTable);
                    break;
                case "sound":
                    details = await _database.SoundBirdTables.FindAsync(search.IdTable);
                    break;
                case "birdinfo":
                    details = await _database.InfoBirdTables.FindAsync(search.IdTable);
                    break;
            }
            result.Add(new
            {
                Id = search.Id,
                Type = search.Type,
                Species = search.Species,
                InputQuery = search.InputQuery,
                CreatedAt = search.CreatedAt,
                Details = details 
            });
        }

        return Ok(result);
    }
}