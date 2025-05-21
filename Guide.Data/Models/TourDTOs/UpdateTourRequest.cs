using System.ComponentModel.DataAnnotations;

namespace Guide.Data.Models.TourDTOs;

public class UpdateTourRequest : CreateTourRequest
{
    [Required]
    public Guid TourId { get; set; }
}