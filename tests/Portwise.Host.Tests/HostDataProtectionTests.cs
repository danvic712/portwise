using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Portwise.Configuration;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class HostDataProtectionTests
{
    [Fact]
    public void AddPortwiseHost_PersistsRelativeKeysUnderContentRoot()
    {
        var contentRoot = Path.Combine(
            Path.GetTempPath(),
            $"portwise-data-protection-{Guid.NewGuid():N}");
        Directory.CreateDirectory(contentRoot);

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{PortwiseDataProtectionOptions.SectionName}:KeysPath"] =
                        ".portwise/keys"
                })
                .Build();
            var environment = new TestHostEnvironment(contentRoot);

            string protectedValue;
            using (var firstProvider = CreateServiceProvider(configuration, environment))
            {
                var dataProtection = firstProvider
                    .GetRequiredService<IDataProtectionProvider>();
                protectedValue = dataProtection
                    .CreateProtector("Portwise.HostDataProtectionTests.v1")
                    .Protect("persistent-secret");
            }

            var keysPath = Path.Combine(contentRoot, ".portwise", "keys");
            Assert.True(Directory.Exists(keysPath));
            Assert.NotEmpty(Directory.GetFiles(keysPath, "key-*.xml"));

            using var secondProvider = CreateServiceProvider(configuration, environment);
            var plaintext = secondProvider
                .GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("Portwise.HostDataProtectionTests.v1")
                .Unprotect(protectedValue);

            Assert.Equal("persistent-secret", plaintext);
        }
        finally
        {
            if (Directory.Exists(contentRoot))
            {
                Directory.Delete(contentRoot, recursive: true);
            }
        }
    }

    private static ServiceProvider CreateServiceProvider(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var services = new ServiceCollection();
        services.AddPortwiseHost(configuration, environment);
        return services.BuildServiceProvider();
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = nameof(HostDataProtectionTests);

        public string ContentRootPath { get; set; } = contentRootPath;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new PhysicalFileProvider(contentRootPath);
    }
}
