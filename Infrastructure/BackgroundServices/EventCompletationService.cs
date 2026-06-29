using Infrastructure.Database;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundServices
{
    public sealed class EventCompletionService(
        IServiceScopeFactory serviceScopeFactory)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine(
                    $"EventCompletionService running at: {DateTimeOffset.Now}");
                try
                {
                    using IServiceScope scope =
                        serviceScopeFactory.CreateScope();

                    ApplicationDbContext context =
                        scope.ServiceProvider
                            .GetRequiredService<ApplicationDbContext>();

                    var events = await context.Events
                        .Where(x =>
                            x.Status == EventState.Active &&
                            x.EndDate <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    foreach (var @event in events)
                    {
                        @event.MarkAsCompleted();
                    }

                    if (events.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // Aquí puedes usar ILogger
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }
    }
}