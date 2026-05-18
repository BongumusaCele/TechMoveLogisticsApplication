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

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(client => client.ClientId);
            entity.Property(client => client.Name).HasMaxLength(120).IsRequired();
            entity.Property(client => client.ContactDetails).HasMaxLength(200).IsRequired();
            entity.Property(client => client.Region).HasMaxLength(80).IsRequired();
            entity.HasIndex(client => client.Name);
        });

        modelBuilder.Entity<Contract>()
            .ToTable("Contracts", table =>
            {
                table.HasCheckConstraint("CK_Contracts_DateRange", "[StartDate] <= [EndDate]");
                table.HasCheckConstraint("CK_Contracts_PriorityLevel", "[PriorityLevel] IS NULL OR ([PriorityLevel] >= 1 AND [PriorityLevel] <= 5)");
            })
            .HasDiscriminator<string>("ContractDiscriminator")
            .HasValue<StandardContract>("Standard")
            .HasValue<InternationalContract>("International")
            .HasValue<PremiumContract>("Premium");

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(contract => contract.ContractId);
            entity.Property(contract => contract.ServiceLevel).HasMaxLength(80).IsRequired();
            entity.Property(contract => contract.SignedAgreementFileName).HasMaxLength(180);
            entity.Property<string>("ContractDiscriminator").HasMaxLength(40).IsRequired();
            entity.HasIndex(contract => new { contract.Status, contract.StartDate, contract.EndDate });
        });

        modelBuilder.Entity<Contract>()
            .HasOne(contract => contract.Client)
            .WithMany(client => client.Contracts)
            .HasForeignKey(contract => contract.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InternationalContract>(entity =>
        {
            entity.Property(contract => contract.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(contract => contract.ExchangeRule).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<ServiceRequest>()
            .ToTable("ServiceRequests", table =>
            {
                table.HasCheckConstraint("CK_ServiceRequests_RequestedAmountUsd", "[RequestedAmountUsd] > 0");
            });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(request => request.ServiceRequestId);
            entity.Property(request => request.RequestType).HasMaxLength(100).IsRequired();
            entity.Property(request => request.Description).HasMaxLength(500).IsRequired();
            entity.Property(request => request.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.HasIndex(request => new { request.ContractId, request.Status });
            entity.HasIndex(request => request.CreatedAt);
        });

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(request => request.Contract)
            .WithMany(contract => contract.ServiceRequests)
            .HasForeignKey(request => request.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .ToTable("Invoices", table =>
            {
                table.HasCheckConstraint("CK_Invoices_AmountZar", "[AmountZar] >= 0");
            });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(invoice => invoice.InvoiceId);
            entity.HasIndex(invoice => invoice.ServiceRequestId).IsUnique();
            entity.HasIndex(invoice => invoice.IssuedAt);
        });

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

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(log => log.AuditLogId);
            entity.Property(log => log.EventType).HasMaxLength(80).IsRequired();
            entity.Property(log => log.Message).HasMaxLength(600).IsRequired();
            entity.HasIndex(log => log.CreatedAt);
            entity.HasIndex(log => log.ContractId);
            entity.HasIndex(log => log.ServiceRequestId);
        });
    }
}
