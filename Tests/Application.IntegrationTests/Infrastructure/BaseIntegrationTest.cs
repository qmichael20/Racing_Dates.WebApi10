using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests.Infrastructure
{
    public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IServiceScope _scope;

        protected readonly IServiceProvider Services;

        protected readonly ApplicationDbContext DbContext;

        protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
        {
            _scope = factory.Services.CreateScope();

            Services = _scope.ServiceProvider;

            DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }
    }
}
