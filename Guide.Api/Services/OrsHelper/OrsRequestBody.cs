namespace Guide.Services.OrsHelper;

public class OrsRequestBody
{
    [System.Text.Json.Serialization.JsonPropertyName("coordinates")]
    public List<List<double>> Coordinates { get; set; }
}