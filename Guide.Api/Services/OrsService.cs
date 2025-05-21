using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; 
using System.Text.Json; 
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Guide.Data.Models;
using Guide.Services.OrsHelper;


namespace Guide.Services;

public class OrsService : IOrsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<OrsService> _logger;

    public OrsService(HttpClient httpClient, IConfiguration configuration, ILogger<OrsService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenRouteService:ApiKey"];
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.openrouteservice.org");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/geo+json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey); // Или просто _apiKey если он передается как параметр
    }

    public async Task<OrsDirectionsResponse> GetRouteAsync(List<List<double>> coordinates, string profile = "driving-car")
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("OpenRouteService API Key не настроен.");
            return null; 
        }
        if (coordinates == null || coordinates.Count < 2)
        {
            _logger.LogWarning("Для построения маршрута необходимо как минимум 2 точки.");
            return null;
        }

        var requestBody = new OrsRequestBody { Coordinates = coordinates };
        var requestUrl = $"/v2/directions/{profile}/geojson"; 
        
        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_apiKey); 

        _logger.LogInformation("Отправка запроса на ORS: URL={Url}, Тело={Body}", _httpClient.BaseAddress + requestUrl, jsonRequestBody);

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, content); 
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<OrsDirectionsResponse>();
                _logger.LogInformation("Ответ от ORS получен успешно.");
                return responseData;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ошибка от OpenRouteService: {StatusCode} - {ReasonPhrase}. Content: {ErrorContent}",
                    response.StatusCode, response.ReasonPhrase, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Исключение при запросе к OpenRouteService.");
            return null;
        }
    }
}