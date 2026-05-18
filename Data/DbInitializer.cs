using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Data;

public static class DbInitializer
{
    public const string DefaultAdminEmail = "musa@admin.co.za";
    public const string DefaultAdminPassword = "Admin@12345";

    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await EnsureAuthenticationSchemaAsync(context);
        await SeedDefaultAdminAsync(context);

        if (await context.Clients.AnyAsync())
        {
            return;
        }

        var clients = new[]
        {
            new Client
            {
                Name = "Umkhonto Freight",
                ContactDetails = "operations@umkhontofreight.example | +27 31 555 0164",
                Region = "Africa"
            },
            new Client
            {
                Name = "Nordic Port Partners",
                ContactDetails = "contracts@nordicports.example | +45 32 12 44 90",
                Region = "Europe"
            }
        };

        context.Clients.AddRange(clients);
        await context.SaveChangesAsync();

        context.Contracts.AddRange(
            new StandardContract
            {
                ClientId = clients[0].ClientId,
                StartDate = DateTime.Today.AddMonths(-2),
                EndDate = DateTime.Today.AddMonths(10),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard freight SLA"
            },
            new InternationalContract
            {
                ClientId = clients[1].ClientId,
                StartDate = DateTime.Today.AddMonths(-1),
                EndDate = DateTime.Today.AddMonths(18),
                Status = ContractStatus.Active,
                ServiceLevel = "International priority SLA",
                CurrencyCode = "USD",
                ExchangeRule = "Use latest USD to ZAR exchange rate"
            });

        context.AuditLogs.Add(new AuditLog
        {
            EventType = "Seed",
            Message = "Initial GLMS prototype data created."
        });

        await context.SaveChangesAsync();
    }

    private static async Task EnsureAuthenticationSchemaAsync(ApplicationDbContext context)
    {
        if (!context.Database.IsRelational())
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[ApplicationUsers]', N'U') IS NULL
            BEGIN
                CREATE TABLE [ApplicationUsers] (
                    [ApplicationUserId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ApplicationUsers] PRIMARY KEY,
                    [FullName] nvarchar(80) NOT NULL,
                    [Email] nvarchar(160) NOT NULL,
                    [PasswordHash] nvarchar(max) NOT NULL,
                    [Role] nvarchar(40) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL
                );

                CREATE UNIQUE INDEX [IX_ApplicationUsers_Email] ON [ApplicationUsers] ([Email]);
            END
            """);
    }

    private static async Task SeedDefaultAdminAsync(ApplicationDbContext context)
    {
        var adminEmail = DefaultAdminEmail.ToUpperInvariant();
        if (await context.ApplicationUsers.AnyAsync(user => user.Email.ToUpper() == adminEmail))
        {
            return;
        }

        var adminUser = new ApplicationUser
        {
            FullName = "System Administrator",
            Email = DefaultAdminEmail,
            Role = "Admin"
        };

        adminUser.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(adminUser, DefaultAdminPassword);
        context.ApplicationUsers.Add(adminUser);

        context.AuditLogs.Add(new AuditLog
        {
            EventType = "Seed",
            Message = $"Default administrator account created for {DefaultAdminEmail}."
        });

        await context.SaveChangesAsync();
    }
}
