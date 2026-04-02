using Api.GraphQL.Inputs;
using EmployeeGraphQL.Application.Services;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;

public class TestFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Stable InMemory database name for this factory instance.
    /// All tests sharing the same IClassFixture&lt;TestFactory&gt; share this store.
    /// </summary>
    public string DbName { get; } = "TestDb_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ── 1. Replace PostgreSQL DbContext with InMemory ─────────────────
            var existingDbOptions = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (existingDbOptions != null)
                services.Remove(existingDbOptions);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(DbName));

            // ── 2. Override Hangfire PostgreSQL storage with InMemory ─────────
            //      AddHangfire is additive; the storage set by the last call wins.
            services.AddHangfire(cfg => cfg.UseInMemoryStorage());

            // ── 3. Remove background hosted services that hit real I/O ────────
            RemoveHostedService<DepartmentImportWorker>(services);
            RemoveHostedService<CsvImportValidationWorker>(services);
            RemoveHostedService<CsvImportExecuteWorker>(services);
            RemoveHostedService<ProjectSchedulerWorker>(services);

            // ── 4. Mock ISsoService ───────────────────────────────────────────
            var ssoDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ISsoService));
            if (ssoDescriptor != null)
                services.Remove(ssoDescriptor);

            var ssoMock = new Mock<ISsoService>();
            ssoMock.Setup(s => s.ValidateToken(It.IsAny<string>()))
                   .ReturnsAsync(true);
            services.AddScoped<ISsoService>(_ => ssoMock.Object);

            // ── 5. Replace ProjectInputValidator ─────────────────────────────
            //      TestProjectInputValidator has all real rules (name, templateId,
            //      date range) but omits the Dapper/NpgsqlConnection LocationId
            //      check that requires a real PostgreSQL connection.
            services.RemoveAll<IValidator<ProjectInput>>();
            services.AddScoped<IValidator<ProjectInput>>(sp =>
                new TestProjectInputValidator(sp.GetRequiredService<AppDbContext>()));

            // ── 6. Replace IProjectService with InMemory-safe test version ────
            //      ProjectService.Projects() uses Dapper; TestProjectService
            //      replaces that method with an EF Core query.
            services.RemoveAll<IProjectService>();
            services.AddScoped<IProjectService, TestProjectService>();
        });
    }

    /// <summary>Synchronously seed data into the shared InMemory database.</summary>
    public void SeedDatabase(Action<AppDbContext> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seed(db);
        db.SaveChanges();
    }

    /// <summary>Asynchronously seed data into the shared InMemory database.</summary>
    public async Task SeedDatabaseAsync(Func<AppDbContext, Task> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await seed(db);
        await db.SaveChangesAsync();
    }

    private static void RemoveHostedService<T>(IServiceCollection services) where T : class
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService)
                     && d.ImplementationType == typeof(T))
            .ToList();
        foreach (var d in descriptors)
            services.Remove(d);
    }
}
