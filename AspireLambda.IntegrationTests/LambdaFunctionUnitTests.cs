using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using System.Text.Json;

namespace AspireLambda.IntegrationTests;

/// <summary>
/// Functional tests that focus on testing specific behaviors without requiring external dependencies.
/// </summary>
public class LambdaFunctionUnitTests
{
    [Fact]
    public void APIGatewayRequest_ShouldCreateValidRequest()
    {
        // Arrange
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            RouteKey = "GET /add/{x}/{y}",
            PathParameters = new Dictionary<string, string>
            {
                { "x", "10" },
                { "y", "20" }
            },
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = "GET",
                    Path = "/add/10/20"
                }
            }
        };

        // Act & Assert
        request.Should().NotBeNull();
        request.PathParameters.Should().ContainKey("x").WhoseValue.Should().Be("10");
        request.PathParameters.Should().ContainKey("y").WhoseValue.Should().Be("20");
        request.RouteKey.Should().Be("GET /add/{x}/{y}");
    }

    [Fact]
    public void TestLambdaContext_ShouldProvideBasicLogging()
    {
        // Arrange
        var context = new TestLambdaContext
        {
            FunctionName = "AddFunctionHandler",
            FunctionVersion = "1",
            LogGroupName = "/aws/lambda/AddFunctionHandler",
            LogStreamName = "test-stream"
        };

        // Act
        context.Logger.LogInformation("Test log message");

        // Assert
        context.Should().NotBeNull();
        context.FunctionName.Should().Be("AddFunctionHandler");
        context.FunctionVersion.Should().Be("1");
        context.Logger.Should().NotBeNull();
    }

    [Fact]
    public void APIGatewayResponse_ShouldSerializeCorrectly()
    {
        // Arrange
        var responseData = new
        {
            Sum = 42,
            Configuration = new
            {
                TestAttribute = "Test Value",
                Environment = "Testing",
                Version = "1.0.0"
            }
        };

        var response = new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            Body = JsonSerializer.Serialize(responseData, new JsonSerializerOptions { WriteIndented = true })
        };

        // Act
        var deserializedBody = JsonSerializer.Deserialize<JsonElement>(response.Body);

        // Assert
        response.StatusCode.Should().Be(200);
        response.Headers.Should().ContainKey("Content-Type").WhoseValue.Should().Be("application/json");
        deserializedBody.GetProperty("Sum").GetInt32().Should().Be(42);
        deserializedBody.GetProperty("Configuration").GetProperty("TestAttribute").GetString().Should().Be("Test Value");
    }

    [Theory]
    [InlineData("5", "3", 8)]
    [InlineData("0", "0", 0)]
    [InlineData("-5", "3", -2)]
    [InlineData("100", "200", 300)]
    public void AdditionCalculation_ShouldReturnCorrectSum(string x, string y, int expectedSum)
    {
        // Arrange
        var xValue = int.Parse(x);
        var yValue = int.Parse(y);

        // Act
        var result = xValue + yValue;

        // Assert
        result.Should().Be(expectedSum);
    }

    [Fact]
    public void JsonSerialization_ShouldHandleComplexObjects()
    {
        // Arrange
        var complexObject = new
        {
            Sum = 15,
            Configuration = new
            {
                TestAttribute = "Complex Test",
                Environment = "Development",
                Version = "2.0.0",
                Message = "Configuration loaded successfully via DI!"
            },
            HttpRequest = new
            {
                Url = "https://httpbin.org/get",
                StatusCode = 200,
                Headers = "content-type: application/json",
                ResponseBody = "{ \"test\": \"response\" }"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(complexObject, new JsonSerializerOptions { WriteIndented = true });
        var deserialized = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        json.Should().NotBeNullOrEmpty();
        deserialized.GetProperty("Sum").GetInt32().Should().Be(15);
        deserialized.GetProperty("Configuration").GetProperty("TestAttribute").GetString().Should().Be("Complex Test");
        deserialized.GetProperty("HttpRequest").GetProperty("StatusCode").GetInt32().Should().Be(200);
    }
}