using BirdApi.Data;
using BirdApi.Models;
using BirdApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BirdApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BirdsController : ControllerBase
{
    //---поля для подключения
    private readonly AppDbContext _database;//------------к базе данных
    private readonly BirdAiService _aiService;//----------подключение к апи пайтона
    private readonly RabbitMqProducer _rabbitMq;//--------брокера сообщений
    private int UserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);//---получение айдишника текущего пользователя
    //---они будут подключаться в program.cs 
    public BirdsController(AppDbContext context, BirdAiService aiService, RabbitMqProducer rabbitMq)
    {
        _database = context;
        _aiService = aiService;
        _rabbitMq = rabbitMq;
    }

    [HttpPost("image")]//------------------------------------------------------------------------------запрос для анализа url изображения. возвращает название птицы и увереность ИИ в своем ответе 
    public async Task<IActionResult> AnalyzeImage(UrlRequest request)//--------------------------------асинхронная функция которая принимает url картинки
    {
        var ai = await _aiService.PredictImageAsync(request.Url);//------------------------------------обращение к ии, await потому что асинхроно, _aiService потому что обращаемя к файлу BirdAiService где находиться функция PredictImageAsync в которую передается url изображения
        var img = new ImageBirdTable { ImageUrl = request.Url, Confidence = ai.Confidence };//---------создание записи для возврата, ImageQuery это модель, взята с того же файла 
        _database.ImageBirdTables.Add(img);//----------------------------------------------------------записываем изменения но сохранение в бд будут ниже
        await _database.SaveChangesAsync();//----------------------------------------------------------запись в бд таблицы ImageQuery 
        await _rabbitMq.SendRabbitMqMessageAsync(UserId, "image", img.Id, ai.Species, request.Url);//--запись в бд таблицы BaseQuery через rabbitMq, так как нам не нужно ждать записи, я получил айди и знаю url, пускай записшет после того как пользователь получит ответ
        return Ok(new BirdUrlResponse{Species = ai.Species, Confidence = ai.Confidence});//------------возврат результата
    }

    [HttpPost("audio")]//------------------------------------------------------------------------------запрос для анализа типо url ссылки на аудио, возвращает вид птицы по этому айдио
    public async Task<IActionResult> AnalyzeAudio(UrlRequest request)//--------------------------------асинхронная функция которая принимает типо url установки аудио
    {
        var ai = await _aiService.PredictAudioAsync(request.Url);//------------------------------------обращение к ии пайтону, PredictAudioAsync прописан в файле BirdAiService
        var snd = new SoundBirdTable { AudioUrl = request.Url, Confidence = ai.Confidence };//---------создание записи для возврата, SoundQuery это модель, взята с того же файла 
        _database.SoundBirdTables.Add(snd);//----------------------------------------------------------записываем изменения но сохранение в бд будут ниже
        await _database.SaveChangesAsync();//----------------------------------------------------------запись в бд 
        await _rabbitMq.SendRabbitMqMessageAsync(UserId, "sound", snd.Id, ai.Species, request.Url);//--запись в бд таблицы BaseQuery через rabbitMq, так как нам не нужно ждать записи, я получил айди и знаю url, пускай записшет после того как пользователь получит ответ
        return Ok(new BirdUrlResponse{Species = ai.Species, Confidence = ai.Confidence});//------------возврат результата
    }

    [HttpPost("info")]//-------------------------------------------------------------------------------запрос для получения информации по виду птицы
    public async Task<IActionResult> GetBirdInfo(InfoRequest request)//--------------------------------асинхронная функция которая принимает вид птицы и вохвращает доп информацию о ней
    {
        var cache = await GetFromCache(request.Species);//---------------------------------------------изначально для получения информации о птице я проверяю нету ли такой же птицы в бд чтобы не открывать сервер и только потом делать запрос к апи
        if (cache != null) 
        {
            await _rabbitMq.SendRabbitMqMessageAsync(UserId, "birdinfo", cache.Id, request.Species, request.Species);
            return Ok(new BirdInfoResponse{
                Species = request.Species,
                Appearance = cache.Appearance,
                Habitat = cache.Habitat,
                Diet = cache.Diet
            });//-------------------------------------------------------------------------------------возвращаем ответ в нужной форме
        }
        var ai = await _aiService.PredictBirdInfoAsync(request.Species);//----------------------------отправка информации к пайтон ии, PredictBirdInfoAsync 
        if (ai == null) return BadRequest("Error with Python Ai");//----------------------------------если апи пайтона ничего не вернуло то вывод ошибки
        var info = new InfoBirdTable
        {
            Appearance = ai.Appearance,
            Habitat = ai.Habitat,
            Diet = ai.Diet
        };//-----------------------------------------------------------------------------------------а если апи пайтона вернуло ответ то записываем его в обьект 
        _database.InfoBirdTables.Add(info);//--------------------------------------------------------добавлем ответ в бд но я сохраню изменения стокой ниже
        await _database.SaveChangesAsync();//--------------------------------------------------------сохранем изменения в таблице InfoQuery
        await _rabbitMq.SendRabbitMqMessageAsync(UserId, "birdinfo", info.Id, request.Species, request.Species);//--сохранения изменения в таблицу BaseQuery через ребит mq 
        return Ok(new BirdInfoResponse{
            Species = request.Species,
            Appearance = info.Appearance,
            Habitat = info.Habitat,
            Diet = info.Diet
        });
    }

    private async Task<InfoBirdTable?> GetFromCache(string species)//--------------------------------достает информаю о виде птицы из бд
    {
        var query = await _database.MainTables//-----------------------------------------------------открытие бд с таблицей всех запросов по птицам
            .FirstOrDefaultAsync(b => b.Species == species && b.Type == "birdinfo");//---------------если тип запроса info и вид тот же то записывает
        if (query == null) 
        {
            return null;
        };
        var info = await _database.InfoBirdTables.FindAsync(query.IdTable);
        return info;
    }
}