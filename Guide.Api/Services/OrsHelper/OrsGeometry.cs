namespace Guide.Services.OrsHelper;

//ТАК КАК ВОЗРАЩАЕТ ДОЛГОТУ И ШИРИНУ 
// ПОЛУЧАЕМ МАССИВ ТОЧЕК


public class OrsGeometry
{
    [System.Text.Json.Serialization.JsonPropertyName("coordinates")]
    public List<List<double>> Coordinates { get; set; } 
    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public string Type { get; set; }
}