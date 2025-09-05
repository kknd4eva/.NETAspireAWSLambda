using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using AspireLambda;
using FluentAssertions;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspireLambda.IntegrationTests;

public class AspireLambdaIntegrationTests
{
    [Fact]
    public async Task LambdaFunction_ShouldProcessAddRequest_WithValidConfiguration()
    {
        // Arrange
        var functions = new Functions();
        var context = new TestLambdaContext
        {
            FunctionName = "AddFunctionHandler",
            FunctionVersion = "1",
            LogGroupName = "/aws/lambda/AddFunctionHandler",
            LogStreamName = "2023/01/01/[$LATEST]test-stream"
        };

        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            RouteKey = "GET /add/{x}/{y}",
            PathParameters = new Dictionary<string, string>
            {
                { "x", "15" },
                { "y", "25" }
            },
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = "GET",
                    Path = "/add/15/25"
                }
            }
        };

        // Act
        var response = await functions.AddFunctionHandler(request, context);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var responseBody = JsonSerializer.Deserialize<JsonElement>(response.Body);
        responseBody.GetProperty("Sum").GetInt32().Should().Be(40);
        responseBody.GetProperty("Configuration").GetProperty("TestAttribute").GetString().Should().NotBeNullOrEmpty();
        responseBody.GetProperty("Configuration").GetProperty("Environment").GetString().Should().NotBeNullOrEmpty();
        responseBody.GetProperty("Configuration").GetProperty("Version").GetString().Should().NotBeNullOrEmpty();
        responseBody.GetProperty("HttpRequest").GetProperty("StatusCode").GetInt32().Should().Be(200);
    }

    [Fact]
    public void LambdaFunction_ShouldInitializeWithDependencyInjection()
    {
        // Arrange & Act
        var functions = new Functions();

        // Assert - If the constructor completes without throwing, DI is working
        functions.Should().NotBeNull();
    }

    [Fact]
    public async Task LambdaFunction_ShouldHandleNegativeNumbers()
    {
        // Arrange
        var functions = new Functions();
        var context = new TestLambdaContext();
        
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            RouteKey = "GET /add/{x}/{y}",
            PathParameters = new Dictionary<string, string>
            {
                { "x", "-10" },
                { "y", "5" }
            },
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = "GET",
                    Path = "/add/-10/5"
                }
            }
        };

        // Act
        var response = await functions.AddFunctionHandler(request, context);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        
        var responseBody = JsonSerializer.Deserialize<JsonElement>(response.Body);
        responseBody.GetProperty("Sum").GetInt32().Should().Be(-5);
    }

    [Fact]
    public async Task LambdaFunction_ShouldHandleZeroValues()
    {
        // Arrange
        var functions = new Functions();
        var context = new TestLambdaContext();
        
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            RouteKey = "GET /add/{x}/{y}",
            PathParameters = new Dictionary<string, string>
            {
                { "x", "0" },
                { "y", "0" }
            },
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = "GET",
                    Path = "/add/0/0"
                }
            }
        };

        // Act
        var response = await functions.AddFunctionHandler(request, context);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        
        var responseBody = JsonSerializer.Deserialize<JsonElement>(response.Body);
        responseBody.GetProperty("Sum").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task LambdaFunction_ShouldIncludeHttpRequestInformation()
    {
        // Arrange
        var functions = new Functions();
        var context = new TestLambdaContext();
        
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            RouteKey = "GET /add/{x}/{y}",
            PathParameters = new Dictionary<string, string>
            {
                { "x", "1" },
                { "y", "1" }
            },
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = "GET",
                    Path = "/add/1/1"
                }
            }
        };

        // Act
        var response = await functions.AddFunctionHandler(request, context);

        // Assert
        var responseBody = JsonSerializer.Deserialize<JsonElement>(response.Body);
        
        responseBody.GetProperty("HttpRequest").Should().NotBeNull();
        responseBody.GetProperty("HttpRequest").GetProperty("Url").GetString().Should().Be("https://httpbin.org/get");
        responseBody.GetProperty("HttpRequest").GetProperty("StatusCode").GetInt32().Should().Be(200);
        responseBody.GetProperty("HttpRequest").GetProperty("Headers").GetString().Should().NotBeNullOrEmpty();
        responseBody.GetProperty("HttpRequest").GetProperty("ResponseBody").GetString().Should().NotBeNullOrEmpty();
    }
}