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

        builder.Entity<PointMediaContent>()
            .HasOne(pmc => pmc.TourPoint)
            .WithMany(tp => tp.MediaContents)
            .HasForeignKey(pmc => pmc.TourPointId);

        builder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .IsRequired();
        builder.Entity<Tour>()
            .Property(t => t.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}