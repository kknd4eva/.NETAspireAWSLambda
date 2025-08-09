using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using AspireLambda.Configuration;

namespace AspireLambda.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureApplicationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TestConfiguration>(configuration.GetSection(TestConfiguration.SectionName));
        
        return services;
    }
}