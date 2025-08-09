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
    .WaitFor(cache);

builder.AddAWSAPIGatewayEmulator("APIGatewayEmulator", Aspire.Hosting.AWS.Lambda.APIGatewayType.HttpV2)
    .WithReference(addFunction, Aspire.Hosting.AWS.Lambda.Method.Get, "/add/{x}/{y}");

builder.Build().Run();
