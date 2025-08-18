# .NET Aspire + AWS Lambda Integration Demo

**ALWAYS follow these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match these instructions.**

This repository demonstrates modern serverless development patterns using .NET Aspire orchestration with AWS Lambda, featuring local development environments, observability, and dependency injection in Lambda functions.

## Working Effectively

### Prerequisites and Setup
- Install .NET 8 SDK (confirmed working with version 8.0.118)
- Install Docker Desktop (required for Redis and DynamoDB Local containers)
- Install AWS Lambda development tools:
  ```bash
  dotnet tool install -g Amazon.Lambda.Tools
  dotnet tool install -g Amazon.Lambda.TestTool-8.0
  ```

### Core Development Commands
Bootstrap and build the repository:
```bash
# Clean slate restoration
dotnet clean              # Takes ~2 seconds
dotnet restore            # Takes ~50 seconds initially, ~1-2 seconds when cached
dotnet build              # Takes ~8-20 seconds. NEVER CANCEL. Set timeout to 60+ minutes for safety.
```

**CRITICAL TIMING NOTES:**
- **NEVER CANCEL builds or long-running commands** - Build times vary significantly
- Initial `dotnet restore` takes up to 50 seconds downloading packages
- `dotnet build` typically takes 8-20 seconds but can take longer on slower systems
- Set timeout values of 60+ minutes for build commands to be safe

### Local Development Options

#### Option 1: Full Aspire Orchestration (Preferred but May Fail in CI)
Start the complete application stack:
```bash
cd AspireLambda.AppHost
dotnet run                # NEVER CANCEL: May take 60+ seconds to start all services
```

**Access Points:**
- **Aspire Dashboard**: http://localhost:15888 (main orchestration UI)
- **API Gateway Emulator**: http://localhost:5000/add/{x}/{y}
- **Example API Call**: http://localhost:5000/add/5/3

**IMPORTANT**: Aspire orchestration requires DCP (Developer Control Plane) which may fail in CI environments with connection errors. If this fails, use Option 2 below.

#### Option 2: Manual Container Management (Reliable Alternative)
When Aspire orchestration fails, manually start dependencies:

1. **Start Redis container:**
   ```bash
   docker run --rm -d --name redis-test -p 6379:6379 redis:7-alpine
   # Takes ~10-30 seconds to download and start if image not cached
   ```

2. **Start DynamoDB Local container:**
   ```bash
   docker run --rm -d --name dynamodb-local -p 8000:8000 amazon/dynamodb-local:latest -jar DynamoDBLocal.jar -sharedDb
   # Takes ~60-120 seconds to download and start if image not cached
   ```

3. **Verify services are running:**
   ```bash
   docker ps  # Should show both containers running
   curl -s http://localhost:8000  # Should return DynamoDB authentication error (expected)
   ```

4. **Start Lambda Test Tool:**
   ```bash
   cd AspireLambda
   dotnet lambda-test-tool-8.0    # Starts web UI on http://localhost:5050
   ```

### Manual Validation and Testing

**ALWAYS manually validate changes** by testing actual functionality:

1. **Test Lambda Function Execution:**
   - Navigate to http://localhost:5050 (Lambda Test Tool UI)
   - Use the example request from `.lambda-test-tool/SavedRequests/AddRequest.json`
   - Verify the function executes and returns expected results
   - Check log output for configuration loading and telemetry data

2. **Test API Gateway Integration (when using Aspire):**
   ```bash
   curl http://localhost:5000/add/5/3
   # Should return JSON response with sum result
   ```

3. **Verify Container Dependencies:**
   ```bash
   docker ps | grep -E "(redis|dynamodb)"  # Should show both containers running
   ```

### Build Warnings (Safe to Ignore)
The build produces these expected warnings - **DO NOT attempt to fix them**:
- `CS8618`: Non-nullable property warnings in DynamoDB entity classes
- `ASPIRE004`: Aspire project reference warning for Lambda project

### No Tests Available
This repository has **no unit test projects**. Do not attempt to run `dotnet test` - it will find no tests to execute.

## Architecture and Key Components

### Project Structure
```
AspireLambdaSolution/
├── AspireLambda.AppHost/           # Aspire orchestration host
├── AspireLambda/                   # Lambda function implementation  
├── AspireLambda.ServiceDefaults/   # Shared service configurations
└── README.md
```

### Port Assignments
- **5050**: AWS Lambda Test Tool web interface
- **5000**: API Gateway emulator (when using Aspire)
- **6379**: Redis cache
- **8000**: DynamoDB Local
- **15888**: Aspire Dashboard (when using Aspire)

### Key Configuration Files
- `AspireLambda/appsettings.json`: Base Lambda function configuration
- `AspireLambda/aws-lambda-tools-defaults.json`: AWS deployment settings
- `AspireLambda/template.yaml`: SAM template for AWS deployment
- `AspireLambda.AppHost/AppHost.cs`: Aspire orchestration configuration

## Common Development Tasks

### Testing Configuration Changes
After modifying configuration files:
1. Stop any running processes (Lambda Test Tool, Aspire)
2. Rebuild: `dotnet build`
3. Restart services using your preferred option above
4. Test function execution through the Lambda Test Tool UI

### Testing Lambda Function Changes
After modifying `Functions.cs` or related code:
1. `dotnet build` in repository root
2. Restart Lambda Test Tool: `cd AspireLambda && dotnet lambda-test-tool-8.0`
3. Execute test requests through http://localhost:5050
4. Verify logs show expected behavior and configuration loading

### Adding Dependencies
When adding NuGet packages:
1. Add to appropriate project file (`AspireLambda.csproj`, etc.)
2. Run `dotnet restore` (may take 30-60 seconds)
3. Run `dotnet build` to verify compilation
4. Test functionality as described above

### Deployment Preparation
For AWS deployment:
```bash
cd AspireLambda
dotnet lambda package  # Creates deployment package
# Or use SAM: sam build && sam deploy
```

## Troubleshooting

### Aspire Orchestration Fails
If `dotnet run` in `AspireLambda.AppHost` fails with DCP connection errors:
- This is normal in CI environments
- Switch to Option 2 (manual container management) above
- Continue development using Lambda Test Tool + manual containers

### Container Issues
If Docker containers fail to start:
```bash
docker rm -f redis-test dynamodb-local  # Clean up existing containers
# Then retry container startup commands
```

### Port Conflicts
If ports are in use:
```bash
# Check what's using ports
ss -tlnp | grep -E "(5050|6379|8000|5000|15888)"
# Stop conflicting processes or containers
```

### Lambda Test Tool Not Loading
If http://localhost:5050 doesn't respond:
- Ensure you're in the `AspireLambda` directory when starting
- Check that `dotnet build` completed successfully
- Verify no firewall is blocking port 5050

## Fast Reference

### Common Commands Output
```bash
# Repository structure
ls -la
.git/
.github/
.gitattributes
.gitignore
AspireLambda/
AspireLambda.AppHost/
AspireLambda.ServiceDefaults/
AspireLambdaSolution.sln
Readme.md

# Solution projects
dotnet sln list
AspireLambda.ServiceDefaults/AspireLambda.ServiceDefaults.csproj
AspireLambda/AspireLambda.csproj
AspireLambda.AppHost/AspireLambda.AppHost.csproj
```

### Key Files to Monitor
When making changes, always check these files for side effects:
- `AspireLambda/Functions.cs` - Main Lambda function implementation
- `AspireLambda/Configuration/TestConfiguration.cs` - Configuration model
- `AspireLambda.AppHost/AppHost.cs` - Orchestration setup
- `AspireLambda.ServiceDefaults/Extensions.cs` - Shared service configuration

This demo showcases enterprise-grade Lambda development with observability, dependency injection, and local development capabilities that mirror production environments.