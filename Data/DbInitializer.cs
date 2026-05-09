using Microsoft.EntityFrameworkCore;
using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

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
}
