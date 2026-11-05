using System.Net.Http.Json;
using BirdApi.Models;

namespace BirdApi.Services;

public class BirdAiService
{
    private readonly HttpClient _httpClient;//------------------нужен для работы с http запросов, чтобы обратиться к http запросам пайтона
    private const string BaseUrl = "http://127.0.0.1:8000";//---порт к ии пайтону для подключения

    public BirdAiService(HttpClient httpClient)//----------конктруктор класа без параметров так как они прописаны свеху а не передаються извне, если я правильно понял
    {
        _httpClient = httpClient;//------------------------для работы с http запросами 
        _httpClient.BaseAddress = new Uri(BaseUrl);//------предаем адрес
        _httpClient.Timeout = TimeSpan.FromMinutes(3);//---ограничитель по времени если пайтон долго молчит, хотя 3 миниту перебор
    }

    public async Task<BirdUrlResponse?> PredictImageAsync(string imageUrl)//-------------------------------------обращение к http запросу пайтона, async Task потому что асинхронный запрос, BirdSpeciesResponse прописано в файле AiModels, это то что я должен от него получить, а string imageUrl это url изоюражения который нужно передать
    {
        var response = await _httpClient.PostAsJsonAsync("/predict/image", new UrlRequest { Url = imageUrl });//--обращение к запросу, _httpClient-для работы с http запросами, PostAsJsonAsync-для преображения информации в json формат потому что именно json передается в http запросах, /predict/image-конкретный запрос от апи пайтона, UrlRequest { Url = imageUrl } - то какой формат мы должны передать, прописан в AiModels.py
        return await response.Content.ReadFromJsonAsync<BirdUrlResponse>();//-------------------------------------возвращет ответ в виде json
    }
    //---тоже самое что и в функциях выше
    public async Task<BirdUrlResponse?> PredictAudioAsync(string audioUrl)
    {
        var response = await _httpClient.PostAsJsonAsync("/predict/audio", new UrlRequest { Url = audioUrl });
        return await response.Content.ReadFromJsonAsync<BirdUrlResponse>();
    }

    public async Task<BirdInfoResponse?> PredictBirdInfoAsync(string species)
    {
        var response = await _httpClient.PostAsJsonAsync("/bird/info", new InfoRequest { Species = species });
        return await response.Content.ReadFromJsonAsync<BirdInfoResponse>();
    }
}