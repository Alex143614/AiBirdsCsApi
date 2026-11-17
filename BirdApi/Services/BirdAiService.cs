using System.Net.Http.Json;
using BirdApi.Models;
using Microsoft.Extensions.Configuration;

namespace BirdApi.Services;

public class BirdAiService
{
    private readonly HttpClient _httpClient;

    // Теперь адрес не const, а читается из конфигурации
    public BirdAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["BirdAiService:BaseUrl"] ?? "http://127.0.0.1:8000";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(3);
    }

    public async Task<BirdUrlResponse?> PredictImageAsync(string imageUrl)
    {
        var response = await _httpClient.PostAsJsonAsync("/predict/image", new UrlRequest { Url = imageUrl });
        return await response.Content.ReadFromJsonAsync<BirdUrlResponse>();
    }

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