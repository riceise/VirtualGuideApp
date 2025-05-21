namespace Guide.Services.OrsHelper;

//ПОЛУЧАЕТ ДЛИТЕЛЬНОСТЬ И РАСТОЯНИЕ

public class OrsProperties
{
    [System.Text.Json.Serialization.JsonPropertyName("summary")]
    public OrsSummary Summary { get; set; }
}