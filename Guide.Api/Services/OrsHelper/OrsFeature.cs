namespace Guide.Services.OrsHelper;


public class OrsFeature
{
    [System.Text.Json.Serialization.JsonPropertyName("geometry")]
    public OrsGeometry Geometry { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("properties")]
    public OrsProperties Properties { get; set; } 
}