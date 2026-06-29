using Application.Abstractions.Data;
using Application.Abstractions.System;
using Application.Ports;
using Infrastructure.Adapters;
using Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration) =>
            services
                .AddRepositories()
                .AddDatabase(configuration)
                .AddAuthenticationInternal(configuration);

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IGuidProvider, GuidProvider>();

            var _repositories = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                {
                    return (assembly.FullName is null) || assembly.FullName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase);
                })
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(RepositoryAttribute)));

            foreach (var repository in _repositories)
            {
                Type typeInterface = repository.GetInterfaces().Single();
                services.AddScoped(typeInterface, repository);
            }

            return services;
        }


        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("Database");

            services.AddDbContext<ApplicationDbContext>(
                options => options
                    .UseSqlServer(connectionString)
            );

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            return services;
        }

        private static IServiceCollection AddAuthenticationInternal(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddHttpContextAccessor();

            return services;
        }
    }
};
