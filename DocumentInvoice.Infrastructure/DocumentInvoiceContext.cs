using Azure.Security.KeyVault.Secrets;
using DocumentInvoice.Domain;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DocumentInvoice.Infrastructure;

public class DocumentInvoiceContext : DbContext
{
    private readonly ApplicationSettings _configuration;

    public DocumentInvoiceContext(IOptions<ApplicationSettings> configuration)
    {
        _configuration = configuration.Value;
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Users> Users { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<UserCompaniesAccess> UserCompaniesAccess { get; set; }
    public DbSet<Invoices> Invoices { get; set; }
    public DbSet<InvoiceItems> InvoiceItems { get; set; }
    public DbSet<DocumentTag> DocumentTag { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserCompaniesAccess>()
            .HasKey(ua => new { ua.UserId, ua.CompanyId });

        // Define the many-to-many relationship
        modelBuilder.Entity<UserCompaniesAccess>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAccessList)
            .HasForeignKey(ua => ua.UserId);

        modelBuilder.Entity<UserCompaniesAccess>()
            .HasOne(ua => ua.Company)
            .WithMany(c => c.UserAccessList)
            .HasForeignKey(ua => ua.CompanyId);

        modelBuilder.Entity<Document>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CompanyId);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_configuration.DbConnectionString);
    }
}
