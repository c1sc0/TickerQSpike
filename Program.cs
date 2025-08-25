using Microsoft.EntityFrameworkCore;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models.Ticker;
using TickerQSpike;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddTickerQ(options =>
{
    options.AddOperationalStore<AppDbContext>(efOpt =>
    {
        efOpt.UseModelCustomizerForMigrations();
        efOpt.CancelMissedTickersOnAppStart();
    });

    options.AddDashboard(configuration =>
    {
        configuration.EnableBasicAuth = true;
        configuration.BasePath = "/tickerq-dashboard";
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (context.Database.IsRelational())
    {
        var pendingMigrations = context.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying pending migrations...");
            context.Database.Migrate();
        }
    }
}

app.UseTickerQ();

app.MapGet("/create", async (ITimeTickerManager<TimeTicker> timeTickerManager) =>
    {
        var reservationJob = new TimeTicker
        {
            Request = TickerHelper.CreateTickerRequest<string>("create"),
            ExecutionTime = DateTime.Now.AddHours(5),
            Function = "SomeFuncName",
            Description = "description",
            Retries = 3,
            RetryIntervals = [20, 60, 100]
        };

        await timeTickerManager.AddAsync(reservationJob);

        return Results.Ok(reservationJob.Id);
    })
    .WithName("create");

app.MapGet("/update/{tickerId}", async (Guid tickerId, ITimeTickerManager<TimeTicker> timeTickerManager) =>
    {
        var updateResult = await timeTickerManager.UpdateAsync(tickerId, ticker =>
        {
            ticker.Description = "update test";
            ticker.ExecutionTime = DateTime.UtcNow.AddDays(1);
        });

        return Results.Ok(updateResult);

    })
    .WithName("update");

app.Run();