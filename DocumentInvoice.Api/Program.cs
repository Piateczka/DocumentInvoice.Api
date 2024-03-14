using Azure;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using DocumentInvoice.Api;
using DocumentInvoice.Api.Extensions;
using DocumentInvoice.Api.Middleware;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Handlers.Command;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults((hostContext, builder) =>
                {
                    builder.UseNewtonsoftJson();

                    builder.UseWhen<AuthenticationMiddleware>(functionContext =>
                    {
                        return !functionContext.FunctionDefinition.Name.Contains("Swagger")
                        && !functionContext.FunctionDefinition.Name.Contains("HealthCheckServiceFunction")
                        && !functionContext.FunctionDefinition.Name.Contains("DocumentProcess")
                        && !functionContext.FunctionDefinition.Name.Contains("DocumentAnalysis");
                    });
                    builder.UseWhen<AuthorizationMiddleware>(functionContext =>
                    {
                        return !functionContext.FunctionDefinition.Name.Contains("Swagger")
                        && !functionContext.FunctionDefinition.Name.Contains("HealthCheckServiceFunction")
                        && !functionContext.FunctionDefinition.Name.Contains("DocumentProcess")
                        && !functionContext.FunctionDefinition.Name.Contains("DocumentAnalysis");
                    });
                    builder.UseMiddleware<GlobalExceptionMiddleware>();
                })
                .ConfigureOpenApi()
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var credential = new ClientSecretCredential(
                        Environment.GetEnvironmentVariable("TenantId"),
                        Environment.GetEnvironmentVariable("ClientId"),
                        Environment.GetEnvironmentVariable("ClientSecret"));
                    configBuilder.AddAzureKeyVault(new Uri(Environment.GetEnvironmentVariable("VaultUrl")), credential, new KeyVaultSecretManager());
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddAzureClients(options =>
                    {
                        IConfiguration configuration = context.Configuration;
                        var credential = new ClientSecretCredential(
                            Environment.GetEnvironmentVariable("TenantId"),
                            Environment.GetEnvironmentVariable("ClientId"),
                            Environment.GetEnvironmentVariable("ClientSecret"));
                        var blobConnectionString = configuration["ApplicationSettings:BlobConnectionString"];
                        var applicationInsightsConnectionString = configuration["ApplicationSettings:ApplicationInsightsCs"];
                        var dbConnectionString = configuration["ApplicationSettings:DbConnectionString"];
                        var searchServiceEndpoint = configuration["ApplicationSettings:SearchServiceEndpoint"];
                        var searchApiKey = configuration["ApplicationSettings:SearchApiKey"];
                        var documentIndex = configuration["ApplicationSettings:DocumentIndex"];
                        options.UseCredential(credential);
                        options.AddSearchClient(new Uri(searchServiceEndpoint), documentIndex, new AzureKeyCredential(searchApiKey));
                        options.AddSearchIndexClient(new Uri(searchServiceEndpoint), new AzureKeyCredential(searchApiKey));
                        options.AddBlobServiceClient(blobConnectionString);
                    });

                    services.AddHealthChecks();

                    services.AddOptions<ApplicationSettings>().Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection(nameof(ApplicationSettings)).Bind(settings);
                    });
                    services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
                     {
                         var options = new OpenApiConfigurationOptions();

                         return options;
                     });

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
                        // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });

                    services.AddDbContext<DocumentInvoiceContext>((service, options) =>
                    {
                        var configuration = service.GetRequiredService<IOptions<ApplicationSettings>>();
                        options.UseSqlServer(configuration.Value.DbConnectionString);
                    });

                    services.AddScoped(typeof(IRepositoryFactory<>), typeof(RepositoryFactory<>));
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                            typeof(CreateCustomerCommandHandler).Assembly
                        ));
                })
                .Build();

host.Run();
