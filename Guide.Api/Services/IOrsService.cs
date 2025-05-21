using Guide.Services.OrsHelper;

namespace Guide.Services;

public interface IOrsService
{
    Task<OrsDirectionsResponse> GetRouteAsync(List<List<double>> coordinates, string profile = "driving-car");

}