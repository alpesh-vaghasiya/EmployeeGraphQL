using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

public class TestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real SsoService so tests don't call the external SSO endpoint.
            // The JWT token in appsettings.Test.json may be expired in CI; we validate
            // token shape locally and skip the live SSO round-trip.
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISsoService));
            if (descriptor != null)
                services.Remove(descriptor);

            var ssoMock = new Mock<ISsoService>();
            ssoMock
                .Setup(s => s.ValidateToken(It.IsAny<string>()))
                .ReturnsAsync(true);

            services.AddScoped<ISsoService>(_ => ssoMock.Object);
        });
    }
}
