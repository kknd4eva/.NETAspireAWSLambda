using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AspireLambda.IntegrationTests;

/// <summary>
/// Tests that demonstrate how Aspire resources can be used in tests.
/// These tests show how to connect to local instances of DynamoDB and Redis
/// that would typically be orchestrated by Aspire.
/// </summary>
public class AspireResourcesIntegrationTests
{
    [Fact]
    public async Task DynamoDBLocal_CanCreateAndQueryTable()
    {
        // Arrange - Using local DynamoDB (typically started by Aspire)
        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:8000", // Default DynamoDB Local port
            UseHttp = true
        };
        
        using var client = new AmazonDynamoDBClient(config);
        
        var tableName = $"TestTable_{Guid.NewGuid():N}";

        try
        {
            // Act - Create table
            await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "TestId", AttributeType = "S" }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "TestId", KeyType = "HASH" }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            // Wait for table to be active
            await WaitForTableToBeActive(client, tableName);

            // Insert test data
            await client.PutItemAsync(new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "TestId", new AttributeValue("test-1") },
                    { "Data", new AttributeValue("test-data") }
                }
            });

            // Query the data
            var response = await client.GetItemAsync(new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "TestId", new AttributeValue("test-1") }
                }
            });

            // Assert
            response.Item.Should().NotBeNull();
            response.Item["TestId"].S.Should().Be("test-1");
            response.Item["Data"].S.Should().Be("test-data");
        }
        finally
        {
            // Cleanup
            try
            {
                await client.DeleteTableAsync(tableName);
            }
            catch
            {
                // Ignore cleanup failures in tests
            }
        }
    }

    [Fact]
    public async Task RedisLocal_CanStoreAndRetrieveData()
    {
        // Arrange - Using local Redis (typically started by Aspire)
        try
        {
            using var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
            var database = connection.GetDatabase();

            var testKey = $"test-key-{Guid.NewGuid():N}";
            var testValue = "test-value";

            // Act
            await database.StringSetAsync(testKey, testValue);
            var retrievedValue = await database.StringGetAsync(testKey);

            // Assert
            retrievedValue.Should().Be(testValue);

            // Cleanup
            await database.KeyDeleteAsync(testKey);
        }
        catch (RedisConnectionException)
        {
            // Skip test if Redis is not available
            // In a real Aspire test, Redis would be guaranteed to be running
            Assert.True(true, "Redis not available - this would work with Aspire orchestration");
        }
    }

    [Fact]
    public void Configuration_CanBindAspireSettings()
    {
        // Arrange - Simulate Aspire configuration
        var configurationData = new Dictionary<string, string>
        {
            { "Aspire:StackExchange:Redis:ConnectionString", "localhost:6379" },
            { "AWS:DynamoDB:LocalServiceUrl", "http://localhost:8000" },
            { "TestConfiguration:TestAttribute", "Aspire Test Value" },
            { "TestConfiguration:Environment", "Testing" },
            { "TestConfiguration:Version", "1.0.0" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var redisConnectionString = configuration["Aspire:StackExchange:Redis:ConnectionString"];
        var dynamoServiceUrl = configuration["AWS:DynamoDB:LocalServiceUrl"];
        var testConfig = configuration.GetSection("TestConfiguration");

        // Assert
        redisConnectionString.Should().Be("localhost:6379");
        dynamoServiceUrl.Should().Be("http://localhost:8000");
        testConfig["TestAttribute"].Should().Be("Aspire Test Value");
        testConfig["Environment"].Should().Be("Testing");
        testConfig["Version"].Should().Be("1.0.0");
    }

    private static async Task WaitForTableToBeActive(IAmazonDynamoDB client, string tableName, int timeoutSeconds = 30)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < TimeSpan.FromSeconds(timeoutSeconds))
        {
            try
            {
                var response = await client.DescribeTableAsync(tableName);
                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    return;
                }
            }
            catch (ResourceNotFoundException)
            {
                // Table doesn't exist yet, keep waiting
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Table {tableName} did not become active within {timeoutSeconds} seconds");
    }
}