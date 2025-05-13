using Guide.Data.Models;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Guide.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tour> Tours { get; set; }
    public DbSet<TourPoint> TourPoints { get; set; }
    public DbSet<PointMediaContent> PointMediaContents { get; set; } 

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var touristRoleId = Guid.NewGuid();
        var excursionistRoleId = Guid.NewGuid();
        var administratorRoleId = Guid.NewGuid();

        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid> { Id = touristRoleId, Name = "Tourist", NormalizedName = "TOURIST", ConcurrencyStamp = Guid.NewGuid().ToString()},
            new IdentityRole<Guid> { Id = excursionistRoleId, Name = "Excursionist", NormalizedName = "EXCURSIONIST", ConcurrencyStamp = Guid.NewGuid().ToString()},
            new IdentityRole<Guid> { Id = administratorRoleId, Name = "Administrator", NormalizedName = "ADMINISTRATOR", ConcurrencyStamp = Guid.NewGuid().ToString()}
        );

        builder.Entity<PointMediaContent>()
            .HasOne(pmc => pmc.TourPoint)
            .WithMany(tp => tp.MediaContents) 
            .HasForeignKey(pmc => pmc.TourPointId);
    }
}