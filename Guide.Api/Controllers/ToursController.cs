using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Guide.Data;
using Guide.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Guide.Data.Models.TourDTOs;
using Guide.Services;
using Microsoft.AspNetCore.Authorization;

namespace Guide.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ToursController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IOrsService _orsService;
    private readonly ILogger<ToursController> _logger;

    public ToursController(ApplicationDbContext context, IOrsService orsService, ILogger<ToursController> logger)
    {
        _context = context;
        _orsService = orsService;
        _logger = logger;
    }

    public class TourDetailsResponse
    {
        public Guid TourId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<TourPointDto> Stops { get; set; } = new List<TourPointDto>();

        public List<List<List<double>>> RouteSegmentsGeometry { get; set; } =
            new List<List<List<double>>>(); // [[lng,lat],[lng,lat],...], ...

        public double? TotalDistanceMeters { get; set; }
        public double? TotalDurationSeconds { get; set; }
    }

    public class TourPointDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TextDescription { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Order { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }


    // GET: api/Tours
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tour>>> GetTours() // Упрощенный список туров
    {
        return await _context.Tours
            .Where(t => t.Status == TourStatus.Approved) // Только одобренные
            .OrderBy(t => t.Title)
            .ToListAsync();
    }


    // GET: api/Tours/{id}/details
    [HttpGet("{id}/details")]
    public async Task<ActionResult<TourDetailsResponse>> GetTourDetails(Guid id)
    {
        var tour = await _context.Tours
            .Include(t => t.TourPoints)
            .ThenInclude(tp => tp.MediaContents)
            .FirstOrDefaultAsync(t => t.TourId == id && t.Status == TourStatus.Approved);

        if (tour == null)
        {
            _logger.LogWarning("Тур с ID: {TourId} не найден или не одобрен.", id);
            return NotFound();
        }

        var response = new TourDetailsResponse
        {
            TourId = tour.TourId,
            Title = tour.Title,
            Description = tour.Description,
            Stops = tour.TourPoints
                .OrderBy(tp => tp.Order)
                .Select(tp => new TourPointDto
                {
                    Id = tp.TourPointId,
                    Name = tp.Name,
                    TextDescription = tp.TextDescription,
                    Latitude = (double)tp.Latitude,
                    Longitude = (double)tp.Longitude,
                    Order = tp.Order,
                    ImageUrls = tp.MediaContents.Select(mc => mc.Url).ToList()
                }).ToList()
        };

        if (response.Stops.Count >= 2)
        {
            // ORS ожидает координаты в формате [долгота, широта]
            var orsCoordinates = response.Stops
                .Select(s => new List<double> { s.Longitude, s.Latitude })
                .ToList();

            // Если ORS API поддерживает запрос сразу для всех сегментов (передавая все точки):
            var orsResponse = await _orsService.GetRouteAsync(orsCoordinates, "foot-walking"); // Или driving-car
            if (orsResponse?.Features != null && orsResponse.Features.Any())
            {
                // Обычно ORS возвращает один Feature для всего маршрута, если переданы все точки
                var routeFeature = orsResponse.Features.First();
                if (routeFeature.Geometry?.Coordinates != null)
                {
                    // В этом случае RouteSegmentsGeometry будет содержать одну "большую" полилинию
                    response.RouteSegmentsGeometry.Add(routeFeature.Geometry.Coordinates);
                }

                if (routeFeature.Properties?.Summary != null)
                {
                    response.TotalDistanceMeters = routeFeature.Properties.Summary.Distance;
                    response.TotalDurationSeconds = routeFeature.Properties.Summary.Duration;
                }
            }
            else
            {
                _logger.LogWarning("Не удалось построить маршрут для тура {TourId} через ORS.", id);
                // Можно попытаться построить по сегментам, если общий запрос не удался или если так надо
                // for (int i = 0; i < response.Stops.Count - 1; i++)
                // {
                //     var segmentCoords = new List<List<double>>
                //     {
                //         new List<double> { response.Stops[i].Longitude, response.Stops[i].Latitude },
                //         new List<double> { response.Stops[i+1].Longitude, response.Stops[i+1].Latitude }
                //     };
                //     var segmentOrsResponse = await _orsService.GetRouteAsync(segmentCoords, "foot-walking");
                //     if (segmentOrsResponse?.Features != null && segmentOrsResponse.Features.Any())
                //     {
                //         response.RouteSegmentsGeometry.Add(segmentOrsResponse.Features.First().Geometry.Coordinates);
                //     }
                // }
            }
        }

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Excursionist,Administrator")]
    public async Task<ActionResult<TourDetailsResponse>> CreateTour([FromBody] CreateTourRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid creatorUserId))
        {
            return Unauthorized("Не удалось определить пользователя.");
        }

        var tour = new Tour
        {
            TourId = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Theme = request.Theme,
            CreatorUserId = creatorUserId,
            Status = TourStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        int pointOrder = 1;
        foreach (var pointReq in request.Points)
        {
            var tourPoint = new TourPoint
            {
                TourPointId = Guid.NewGuid(),
                TourId = tour.TourId,
                Name = pointReq.Name,
                TextDescription = pointReq.TextDescription,
                Latitude = pointReq.Latitude,
                Longitude = pointReq.Longitude,
                Order = pointOrder++
            };

            if (pointReq.TempImageReferences != null)
            {
                int mediaOrder = 1;
                foreach (var imageUrl in pointReq.TempImageReferences)
                {
                    tourPoint.MediaContents.Add(new PointMediaContent
                    {
                        PointMediaContentId = Guid.NewGuid(),
                        TourPointId = tourPoint.TourPointId,
                        Url = imageUrl,
                        Order = mediaOrder++
                    });
                }
            }

            tour.TourPoints.Add(tourPoint);
        }

        _context.Tours.Add(tour);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создан новый тур: {TourTitle} (ID: {TourId}) пользователем {UserId}", tour.Title,
            tour.TourId, creatorUserId);

        var response = new TourDetailsResponse
        {
            TourId = tour.TourId,
            Title = tour.Title,
            Description = tour.Description,
            Stops = tour.TourPoints
                .OrderBy(tp => tp.Order)
                .Select(tp => new TourPointDto
                {
                    Id = tp.TourPointId,
                    Name = tp.Name,
                    TextDescription = tp.TextDescription,
                    Latitude = (double)tp.Latitude,
                    Longitude = (double)tp.Longitude,
                    Order = tp.Order,
                    ImageUrls = tp.MediaContents.Select(mc => mc.Url).ToList()
                }).ToList()
        };

        return CreatedAtAction(nameof(GetTourDetails), new { id = tour.TourId }, response);
    }
}