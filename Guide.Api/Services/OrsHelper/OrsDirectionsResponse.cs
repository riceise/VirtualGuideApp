namespace Guide.Services.OrsHelper;

public class OrsDirectionsResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("features")]
    public List<OrsFeature> Features { get; set; }
}