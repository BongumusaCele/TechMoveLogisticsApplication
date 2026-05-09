using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contract>()
            .HasDiscriminator<string>("ContractDiscriminator")
            .HasValue<StandardContract>("Standard")
            .HasValue<InternationalContract>("International")
            .HasValue<PremiumContract>("Premium");

        modelBuilder.Entity<Contract>()
            .HasOne(contract => contract.Client)
            .WithMany(client => client.Contracts)
            .HasForeignKey(contract => contract.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(request => request.Contract)
            .WithMany(contract => contract.ServiceRequests)
            .HasForeignKey(request => request.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(invoice => invoice.ServiceRequest)
            .WithOne(request => request.Invoice)
            .HasForeignKey<Invoice>(invoice => invoice.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.RequestedAmountUsd)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.ExchangeRate)
            .HasColumnType("decimal(18,4)");

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.Cost)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Invoice>()
            .Property(invoice => invoice.AmountZar)
            .HasColumnType("decimal(18,2)");
    }
}
