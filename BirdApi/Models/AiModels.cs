using System.Text.Json.Serialization;

namespace BirdApi.Models;

//---модель того какую информацию я должен получить от апи пайтона
//---я буду передавать url картинки или типо url ссылку для установки 
//---звука и получать от апи пайтона по этому url фото / аудио 

//---Species - вид птицы
//---Confidence - уровень уверености ИИ в своих выводах

public class BirdUrlResponse
{
    [JsonPropertyName("species")]
    public string? Species { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

//---модель того какую информацию я должен получить от апи пайтона 
//---при отправке вида птицы, я даю ии на пайтоне вид путицы а он

//---Species - вернуть тот же вид птицы для удобства
//---Appearance - описание внешности птицы
//---Habitat - место проживание
//---Diet - рацион питания

public class BirdInfoResponse
{
    [JsonPropertyName("species")]
    public string? Species { get; set; }

    [JsonPropertyName("appearance")]
    public string? Appearance { get; set; }

    [JsonPropertyName("habitat")]
    public string? Habitat { get; set; }

    [JsonPropertyName("diet")]
    public string? Diet { get; set; }
}

//---модель того какие данные мне передать в пайтон апи, Url вмысле 
//---картинки или ссылки на установку звука птиц чтобы узнать ее вид от ии

public class UrlRequest { public string Url { get; set; } = string.Empty; }

//---модель того какие данные мне передать в пайтон апи, Species 
//---товесть вид птицы, чтобы дать ии пайтону и получить информацию 
//---об этом виде птицы

public class InfoRequest { public string Species { get; set; } = string.Empty; }


