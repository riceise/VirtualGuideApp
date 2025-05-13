using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Guide.Data.Models;


public class User : IdentityUser<Guid> 
{
    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    public virtual ICollection<Tour> CreatedTours { get; set; } = new List<Tour>();
}