namespace Guide.Services.OrsHelper;


//ДЛИТЕЛЬНОСТЬ И РАСТОЯНИЕ В МЕТРАХ И СУКУНДАХ

public class OrsSummary
{
    [System.Text.Json.Serialization.JsonPropertyName("distance")]
    public double Distance { get; set; } 
    [System.Text.Json.Serialization.JsonPropertyName("duration")]
    public double Duration { get; set; }
}