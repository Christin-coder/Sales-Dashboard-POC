using Microsoft.EntityFrameworkCore;
using SalesAPI.Models; // Ensure this matches your namespace

namespace SalesAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // These represent your physical tables in SSMS
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }

    // This part is crucial for .NET 9 to understand the relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Telling EF that a Sale has one Customer and one Product
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerID);

        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductID);
    }
}