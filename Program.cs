using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using TechMoveLogisticsApplication.Data;
using TechMoveLogisticsApplication.Services;
using TechMoveLogisticsApplication.Services.Currency;
using TechMoveLogisticsApplication.Services.Factories;
using TechMoveLogisticsApplication.Services.Observers;
using TechMoveLogisticsApplication.Services.Storage;
using TechMoveLogisticsApplication.Services.Strategies;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys")));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Configuration.GetValue<bool>("UsePrototypeMemoryStore"))
    {
        options.UseInMemoryDatabase("TechMoveGlmsPrototype");
    }
    else
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 1));
    }
});

builder.Services.AddScoped<IContractFactory, StandardContractFactory>();
builder.Services.AddScoped<IContractFactory, InternationalContractFactory>();
builder.Services.AddScoped<IContractFactory, PremiumContractFactory>();
builder.Services.AddScoped<IContractFactoryResolver, ContractFactoryResolver>();

builder.Services.AddScoped<IValidationStrategy, ActiveContractValidationStrategy>();
builder.Services.AddScoped<IValidationStrategy, SlaValidationStrategy>();
builder.Services.AddScoped<IValidationStrategy, InternationalRequestValidationStrategy>();
builder.Services.AddScoped<TechMoveLogisticsApplication.Services.Strategies.ValidationContext>();

builder.Services.AddScoped<IInvoiceStrategy, LocalInvoiceStrategy>();
builder.Services.AddScoped<IInvoiceStrategy, InternationalInvoiceStrategy>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddHttpClient<ICurrencyConversionService, CurrencyConversionService>();

builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<SlaMonitoringService>();
builder.Services.AddScoped<IContractSubject>(provider =>
{
    var subject = new ContractSubject();
    subject.Attach(provider.GetRequiredService<NotificationService>());
    subject.Attach(provider.GetRequiredService<AuditService>());
    subject.Attach(provider.GetRequiredService<SlaMonitoringService>());
    return subject;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Database initialization failed: {exception.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
