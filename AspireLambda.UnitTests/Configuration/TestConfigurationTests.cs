using AspireLambda.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspireLambda.UnitTests.Configuration;

public class TestConfigurationTests
{
    [Fact]
    public void TestConfiguration_ShouldBindFromConfiguration()
    {
        // Arrange
        var configurationData = new Dictionary<string, string>
        {
            { "TestConfiguration:TestAttribute", "Test Value" },
            { "TestConfiguration:Environment", "Testing" },
            { "TestConfiguration:Version", "2.0.0" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        var services = new ServiceCollection();
        services.Configure<TestConfiguration>(configuration.GetSection("TestConfiguration"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var testConfig = serviceProvider.GetRequiredService<IOptions<TestConfiguration>>().Value;

        // Assert
        testConfig.Should().NotBeNull();
        testConfig.TestAttribute.Should().Be("Test Value");
        testConfig.Environment.Should().Be("Testing");
        testConfig.Version.Should().Be("2.0.0");
    }

    [Fact]
    public void TestConfiguration_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var testConfig = new TestConfiguration();

        // Assert
        testConfig.TestAttribute.Should().Be(string.Empty);
        testConfig.Environment.Should().Be(string.Empty);
        testConfig.Version.Should().Be(string.Empty);
    }

    [Fact]
    public void TestConfiguration_SectionName_ShouldBeCorrect()
    {
        // Arrange & Act & Assert
        TestConfiguration.SectionName.Should().Be("TestConfiguration");
    }
}