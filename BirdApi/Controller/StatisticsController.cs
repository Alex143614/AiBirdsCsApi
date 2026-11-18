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
    //---функция которая возвращает список пользователей которые чаще всего делали запросы от большего к меньшему с пагинацией
    //---int page - пропуск с которого начинать
    //---int pageSize - сколько елементов взять
    public async Task<IActionResult> GetTopUsersPaginated(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        var topUsers = await _database.MainTables
            .GroupBy(x => x.UserId)
            .Select(x => new {UserId = x.Key, RequestCount = x.Count()})
            .OrderByDescending(x => x.RequestCount)
            .Skip((page-1)*pageSize)
            .Take(pageSize)
            .Join(_database.Users,
                top => top.UserId,
                user => user.Id,
                (top, user) => new{
                    UserId = top.UserId,
                    Username = user.Username,
                    RequestCount = top.RequestCount
                })
            .ToListAsync();
        return Ok(topUsers);
    }
    [HttpGet("top_birds")]
    //---функция которая возвращает список птиц которые чаще всего попадались в запросах от большего к меньшему с пагинацией
    //---int page - пропуск с которого начинать
    //---int pageSize - сколько елементов взять
    public async Task<IActionResult> GetTopBirdsPaginated(int page, int pageSize)
    {
        if (page < 1) page = 1;//--------------------------------------------на всякий если ввод пользователя кривой
        if (pageSize < 1) pageSize = 10;//-----------------------------------на всякий если ввод пользователя кривой
        var birds = await _database.MainTables//-----------------------------запись и открытие в бд
            .Where(x => x.Species != null && x.Species != "")//--------------отсеивает значения где птицы нету
            .GroupBy(x => x.Species)//---------------------------------------собирает птицы по тем же самым видам
            .Select(x => new {Species = x.Key, RequestCount = x.Count()})//--создание отдельного обьекта с ключем айди и количсетвом того как часто птиа попадалась в запросах
            .OrderByDescending(x => x.RequestCount)//------------------------сортировка от большего к меньшему
            .Skip((page-1) * pageSize)//-------------------------------------пагинация начало
            .Take(pageSize)//------------------------------------------------пагинация конец
            .ToListAsync();//------------------------------------------------обращение к бд и возврат результата в виде списка
        return Ok(birds);//--------------------------------------------------возврат результата
    }
}//fuser -k 5208/tcp