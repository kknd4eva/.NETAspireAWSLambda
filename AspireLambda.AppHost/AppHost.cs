using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Aspire.Hosting;
using Aspire.Hosting.AWS.Lambda;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache", port: 6379);

var dynamoDbLocal = builder.AddAWSDynamoDBLocal("DynamoDBAccounts");

var addFunction = builder.AddAWSLambdaFunction<Projects.AspireLambda>("AddFunctionHandler",
    lambdaHandler: "AspireLambda::AspireLambda.Functions::AddFunctionHandler",
    options: new LambdaFunctionOptions
    {
        LogFormat = LogFormat.JSON,
    })
    .WithReference(cache, "cache")
    .WaitFor(cache)
    .WithReference(dynamoDbLocal)
    .WaitFor(dynamoDbLocal);

builder.AddAWSAPIGatewayEmulator("APIGatewayEmulator", Aspire.Hosting.AWS.Lambda.APIGatewayType.HttpV2)
    .WithReference(addFunction, Aspire.Hosting.AWS.Lambda.Method.Get, "/add/{x}/{y}");

builder.Eventing.Subscribe<ResourceReadyEvent>(dynamoDbLocal.Resource, async (evnt, ct) =>
{
    // Configure DynamoDB service client to connect to DynamoDB local.
    var serviceUrl = dynamoDbLocal.Resource.GetEndpoint("http").Url;
    var ddbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = serviceUrl });

    // Create the Accounts table.
    var creatResponse = await ddbClient.CreateTableAsync(new CreateTableRequest
    {
        TableName = "Accounts",
        AttributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinition { AttributeName = "Id", AttributeType = "S" }
        },
        KeySchema = new List<KeySchemaElement>
        {
            new KeySchemaElement { AttributeName = "Id", KeyType = "HASH" }
        },
        BillingMode = BillingMode.PAY_PER_REQUEST
    });

    // Add an account to the Accounts table.
    await ddbClient.PutItemAsync(new PutItemRequest
    {
        TableName = "Accounts",
        Item = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue("1") },
            { "Name", new AttributeValue("Amazon") },
            { "Address", new AttributeValue("Seattle, WA") }
        }
    });
});

builder.Build().Run();
