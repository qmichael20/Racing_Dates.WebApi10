using Application.Ports;
using Infrastructure.Database;

namespace Infrastructure.Adapters
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        public async Task SaveAsync(CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? new CancellationTokenSource().Token;

            await context.SaveChangesAsync(token);
        }
    }
}
