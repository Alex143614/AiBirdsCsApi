using BirdApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BirdApi.Controllers;

// [Authorize]
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _database;
    public StatisticsController(AppDbContext context)
    {
        _database = context;
    }

    [HttpGet("get_type_usege")]//-------------------------------------вывод количсетва запросов по каждому типу, поиск вида по аудио\видео либо 
    public async Task<IActionResult> GetUsageStats()//----------------асинхронная функция 
    {
        int imageCount = 0;//-----------------------------------------счетчик количества поиска по url
        int soundCount = 0;//-----------------------------------------счетчик количества поиска по аудио ссылке 
        int infoCount  = 0;//-----------------------------------------счетчик количества поиска по доп информации вида
        var DataBaseQuery= await _database.MainTables.ToListAsync();//подлючение к бд, к таблице BaseQuery
        foreach (var query in DataBaseQuery)//------------------------перебор всех елементов с таблицы
        {
            switch(query.Type)//--------------------------------------если поле типа запроса ровно
            {
                case "image":   imageCount++; break;//----------------то счетчик +
                case "sound":   soundCount++; break;//----------------то счетчик +
                case "birdinfo": infoCount++; break;//----------------то счетчик +
            }
        }
        var result = new Dictionary<string, int>{//-------------------создаю директорию для json подобного ответа
            {"image", imageCount},
            {"sound", soundCount},
            {"info",   infoCount}
        };
        return Ok(result);//-------------------------------------------возврат результата
    }

    [HttpGet("top_users")]
    public async Task<IActionResult> GetTopUsers(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        var groupedData = _database.MainTables
            .GroupBy(q => q.UserId)
            .Select(g => new { UserId = g.Key, RequestCount = g.Count() })
            .OrderByDescending(x => x.RequestCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        var result = await groupedData
            .Join(_database.Users,
                 top => top.UserId,
                 user => user.Id,
                (top, user) => new
                {
                    UserId = top.UserId,
                    Username = user.Username,
                    RequestCount = top.RequestCount
                })
            .ToListAsync();
        return Ok(result);
}
}