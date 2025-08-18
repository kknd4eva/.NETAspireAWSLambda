# .NET Aspire + AWS Lambda Integration Demo

A comprehensive demonstration showcasing how to combine **.NET Aspire** with **AWS Lambda** to create a superior cloud-native developer experience. This solution demonstrates modern patterns for building, testing, and observing serverless applications with enterprise-grade tooling.

## ?? Overview

This solution illustrates how .NET Aspire's orchestration capabilities can enhance AWS Lambda development by providing:

- **Local Development Environment**: Complete local infrastructure setup with Redis and DynamoDB
- **Unified Configuration Management**: Centralized configuration with environment-specific overrides
- **Advanced Observability**: OpenTelemetry integration for comprehensive tracing and metrics
- **Dependency Injection**: Full DI container support in Lambda functions
- **Service Discovery**: Automatic service resolution and connection management
- **Resilience Patterns**: Built-in retry policies and circuit breakers

## ??? Architecture

### Project Structure

```
AspireLambdaSolution/
??? AspireLambda.AppHost/           # Aspire orchestration host
??? AspireLambda/                   # Lambda function implementation
??? AspireLambda.ServiceDefaults/   # Shared service configurations
??? README.md
```

### Components

- **AppHost**: Orchestrates the entire application stack including:
  - Redis cache (port 6379)
  - DynamoDB Local (port 8000) 
  - Lambda function emulation
  - API Gateway emulator
  
- **Lambda Function**: Demonstrates modern Lambda development with:
  - Dependency injection container
  - Configuration management
  - OpenTelemetry tracing
  - Redis caching
  - DynamoDB integration
  - HTTP client instrumentation

- **Service Defaults**: Provides enterprise-grade defaults for:
  - OpenTelemetry configuration
  - HTTP resilience patterns
  - Service discovery
  - Health checks

## ?? Key Features

### 1. **Modern Lambda Architecture**
- **Dependency Injection**: Full .NET DI container in Lambda functions
- **Configuration Management**: Strongly-typed configuration with environment overrides
- **Service Registration**: Automatic service discovery and registration

### 2. **Local Development Experience**
- **Infrastructure as Code**: Aspire manages local Redis and DynamoDB instances
- **Hot Reload**: Instant feedback during development
- **Unified Debugging**: Debug Lambda functions alongside infrastructure components

### 3. **Enterprise Observability**
- **Distributed Tracing**: Complete request tracing across Lambda, Redis, DynamoDB, and HTTP calls
- **Metrics Collection**: Runtime, HTTP, AWS, and custom metrics
- **Structured Logging**: JSON-formatted logs with correlation IDs

### 4. **Production-Ready Patterns**
- **Resilience**: HTTP retry policies and circuit breakers
- **Health Checks**: Application and dependency health monitoring
- **Configuration Validation**: Startup-time configuration validation

## ??? Technologies Used

### Core Framework
- **.NET 8**: Latest LTS version with improved performance
- **C# 12**: Latest language features and syntax

### AWS Services
- **AWS Lambda**: Serverless compute
- **API Gateway**: HTTP API endpoints
- **DynamoDB**: NoSQL database
- **CloudWatch**: Logging and monitoring

### .NET Aspire Components
- **Aspire.Hosting.AWS**: AWS service orchestration
- **Aspire.StackExchange.Redis**: Redis integration
- **Aspire.Hosting.AppHost**: Application orchestration

### Observability Stack
- **OpenTelemetry**: Distributed tracing and metrics
- **AWS Instrumentation**: AWS service call tracing
- **Redis Instrumentation**: Redis operation tracing
- **HTTP Instrumentation**: HTTP client request tracing

## ????? Getting Started

### Prerequisites
- .NET 8 SDK
- Docker Desktop (for Redis and DynamoDB Local)
- AWS CLI (optional, for deployment)

### Running Locally

1. **Clone and Build**
   ```bash
   git clone <repository-url>
   cd AspireLambdaSolution
   dotnet restore
   dotnet build
   ```

2. **Start the Application**
   ```bash
   cd AspireLambda.AppHost
   dotnet run
   ```

3. **Access the Application**
   - **Aspire Dashboard**: http://localhost:15888
   - **API Endpoint**: http://localhost:5000/add/{x}/{y}
   - **Example**: http://localhost:5000/add/5/3

### What Happens When You Run It

1. **Aspire AppHost** starts and orchestrates:
   - Redis container on port 6379
   - DynamoDB Local on port 8000
   - Lambda function emulation
   - API Gateway emulator

2. **Lambda Function** initializes with:
   - Full dependency injection container
   - Configuration loaded from appsettings.json
   - OpenTelemetry tracing enabled
   - All AWS services configured

3. **API Call Processing**:
   - Adds two numbers (from URL parameters)
   - Makes HTTP call to external service (httpbin.org)
   - Demonstrates configuration injection
   - Returns comprehensive response with tracing data

## ?? Observability Features

### Distributed Tracing
The solution provides complete distributed tracing for:
- **Lambda Function Execution**: Entry/exit points and duration
- **HTTP Calls**: External service calls with timing
- **Redis Operations**: Cache hit/miss patterns
- **DynamoDB Operations**: Database query performance
- **Configuration Access**: Settings resolution tracing

### Metrics Collection
Automatic metrics for:
- **Runtime Metrics**: GC, memory, thread pool
- **HTTP Metrics**: Request rates, latency, status codes
- **AWS Metrics**: Service call success/failure rates
- **Custom Business Metrics**: Application-specific measurements

### Structured Logging
All logs include:
- **Correlation IDs**: Track requests across components
- **Structured Data**: JSON format for easy querying
- **Contextual Information**: Request IDs, user context, timing

## ?? Configuration Management

### Hierarchical Configuration
```json
{
  "Aspire": {
    "StackExchange": {
      "Redis": {
        "ConnectionString": "localhost:6379"
      }
    }
  },
  "TestConfiguration": {
    "TestAttribute": "Configuration loaded successfully!",
    "Environment": "Development",
    "Version": "1.0.0"
  },
  "AWS": {
    "DynamoDB": {
      "LocalServiceUrl": "http://localhost:8000"
    }
  }
}
```

### Environment Overrides
- **appsettings.json**: Base configuration
- **appsettings.Development.json**: Development overrides
- **Environment Variables**: Runtime overrides

## ?? Deployment Options

### Local Testing
- Use Aspire AppHost for complete local development
- All dependencies running in containers
- Hot reload and debugging support

### AWS Deployment
- Standard Lambda deployment via AWS CLI or Visual Studio
- CloudFormation/CDK templates (can be added)
- Container image deployment support

## ?? Learning Outcomes

This demo showcases how to:

1. **Modernize Lambda Development**: Move beyond simple functions to full applications
2. **Implement Enterprise Patterns**: DI, configuration, observability in serverless
3. **Accelerate Development**: Local development that mirrors production
4. **Improve Debugging**: Distributed tracing across all components
5. **Enhance Maintainability**: Structured, testable, observable code

## 🧪 Testing

This project includes comprehensive unit and integration tests that demonstrate how to effectively test serverless applications using Aspire's local development capabilities.

### Test Structure
- **Unit Tests**: 7 tests covering configuration, models, and business logic
- **Integration Tests**: 10+ tests demonstrating Lambda function behavior and Aspire resource integration

### Running Tests
```bash
# Run all unit tests (no external dependencies)
dotnet test AspireLambda.UnitTests

# Run integration tests (some require Aspire resources)
dotnet test AspireLambda.IntegrationTests

# Build and test everything
dotnet build && dotnet test
```

### What's Tested
- Configuration binding and validation
- DynamoDB model attributes and data handling
- Lambda function request/response processing
- API Gateway integration patterns
- JSON serialization and business logic
- Aspire resource connection patterns

See [TESTING.md](TESTING.md) for detailed testing documentation and best practices.

## ?? Contributing

This is a demo project showcasing integration patterns. Feel free to:
- Add additional AWS services
- Implement more observability patterns  
- Enhance the configuration management
- Add automated testing examples

## ?? Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [AWS Lambda .NET Guide](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/)

---

This solution demonstrates the powerful combination of .NET Aspire's developer experience enhancements with AWS Lambda's serverless capabilities, providing a modern approach to cloud-native application development.