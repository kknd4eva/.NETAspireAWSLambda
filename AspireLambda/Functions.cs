using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using AspireLambda.Configuration;
using AspireLambda.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AspireLambda;

public class Functions
{
    private readonly IHost _host;
    private readonly TracerProvider _traceProvider;
    private readonly ILogger<Functions> _logger;
    private readonly TestConfiguration _testConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;
    public Functions()
    {
        var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = Directory.GetCurrentDirectory(),
        });

        // Configure configuration sources explicitly
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
        builder.Configuration.AddEnvironmentVariables();

        // Add service defaults (includes OpenTelemetry, health checks, etc.)
        builder.AddServiceDefaults();
        builder.Services.AddHttpClient();

        // Configure application settings
        builder.Services.ConfigureApplicationSettings(builder.Configuration);

        // Add Redis client
        builder.AddRedisClient(connectionName: "cache");
        
        // Add AWS services
        builder.Services.AddAWSService<IAmazonDynamoDB>();
        builder.Services.AddSingleton<DynamoDBContext>(sp =>
        {
            var contextBuilder = new DynamoDBContextBuilder();
            contextBuilder.ConfigureContext(config => config.DisableFetchingTableMetadata = true);
            return contextBuilder.Build();
        });

        _host = builder.Build();
        _traceProvider = _host.Services.GetRequiredService<TracerProvider>();
        _logger = _host.Services.GetRequiredService<ILogger<Functions>>();
        _httpClientFactory = _host.Services.GetRequiredService<IHttpClientFactory>();

        // Load test configuration to verify DI is working
        var testConfigOptions = _host.Services.GetRequiredService<IOptions<TestConfiguration>>();
        _testConfiguration = testConfigOptions.Value;
        
        // Log the test configuration to verify it's loaded
        _logger.LogInformation("Configuration loaded successfully. Test Attribute: {TestAttribute}, Environment: {Environment}, Version: {Version}", 
            _testConfiguration.TestAttribute, _testConfiguration.Environment, _testConfiguration.Version);
    }

    /// <summary>
    /// Gateway URL including path parameters: https://<api-id>.execute-api.<region>.amazonaws.com/<stage>/add/{x}/{y}
    /// </summary>
    ///
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> AddFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        => await AWSLambdaWrapper.TraceAsync(_traceProvider, async (request, context) =>
        {
            context.Logger.LogDebug("Starting adding operation");
            
            // Log the test configuration to show it's working in the handler
            context.Logger.LogInformation("Test Configuration - TestAttribute: {TestAttribute}, Environment: {Environment}, Version: {Version}", 
                _testConfiguration.TestAttribute, _testConfiguration.Environment, _testConfiguration.Version);

            var x = (int)Convert.ChangeType(request.PathParameters["x"], typeof(int));
            var y = (int)Convert.ChangeType(request.PathParameters["y"], typeof(int));

            var sum = x + y;

            context.Logger.LogInformation("Adding {x} with {y} is {sum}", x, y, sum);

            // Make GET request to httpbin.org
            string httpResponseBody = "";
            int httpStatusCode = 0;
            string httpHeaders = "";
        
            try
            {
                context.Logger.LogInformation("Making GET request to https://httpbin.org/get");
                using var httpClient = _httpClientFactory.CreateClient();

                using var httpResponse = await httpClient.GetAsync("https://httpbin.org/get");
                httpStatusCode = (int)httpResponse.StatusCode;
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            
                // Record response headers
                var headerDict = new Dictionary<string, IEnumerable<string>>();
                foreach (var header in httpResponse.Headers)
                {
                    headerDict[header.Key] = header.Value;
                }
                foreach (var header in httpResponse.Content.Headers)
                {
                    headerDict[header.Key] = header.Value;
                }
                httpHeaders = JsonSerializer.Serialize(headerDict);
            
                context.Logger.LogInformation("HTTP Request completed - Status: {statusCode}, Body Length: {bodyLength}", 
                    httpStatusCode, httpResponseBody.Length);
            }
            catch (Exception ex)
            {
                context.Logger.LogError("Failed to make HTTP request: {error}", ex.Message);
                httpResponseBody = $"Error: {ex.Message}";
                httpStatusCode = -1;
            }

            // Create response object that includes both the sum, HTTP request results, and configuration test
            var responseData = new
            {
                Sum = sum,
                Calculation = new { X = x, Y = y, Result = sum },
                ConfigurationTest = new 
                {
                    TestAttribute = _testConfiguration.TestAttribute,
                    Environment = _testConfiguration.Environment,
                    Version = _testConfiguration.Version,
                    Message = "Configuration loaded successfully via DI!"
                },
                HttpRequest = new
                {
                    Url = "https://httpbin.org/get",
                    StatusCode = httpStatusCode,
                    Headers = httpHeaders,
                    ResponseBody = httpResponseBody
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

            return response;
        }, request, context);

    [DynamoDBTable("Accounts")]
    public class Accounts
    {
        [DynamoDBHashKey("Id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
    }
}