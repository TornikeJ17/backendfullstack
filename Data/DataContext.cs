using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using reactProjectFull.Models;
using System.Reflection;

namespace reactProjectFull.Data;

public class DataContext : IdentityDbContext<User>
{

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ProductUser> ProductUsers { get; set; }
    
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductUser>().HasKey(pu => new { pu.ProductId, pu.UserId });
    
        modelBuilder.Entity<ProductUser>()
            .HasOne(pu => pu.Product)
            .WithMany(p => p.ProductUsers)
            .HasForeignKey(pu => pu.ProductId);
    
        modelBuilder.Entity<ProductUser>()
            .HasOne(pu => pu.User)
            .WithMany(u => u.ProductUsers)
            .HasForeignKey(pu => pu.UserId);

        modelBuilder.Entity<Product>()
            .Property(p => p.ProductPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(e => e.ImageUrls)
            .HasConversion(
                v => string.Join(";", v),
                v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

    }
}