using Azure;
using Azure.Identity;
using DocumentInvoice.Api;
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
using Microsoft.Extensions.Logging.ApplicationInsights;

var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults((hostContext, builder) =>
                {
                    builder.UseNewtonsoftJson();
                    builder.UseWhen<AuthenticationMiddleware>(functionContext =>
                    {
                        return !functionContext.FunctionDefinition.Name.Contains("Swagger");
                    });
                    builder.UseMiddleware<GlobalExceptionMiddleware>();
                })
                .ConfigureOpenApi()
                .ConfigureAppConfiguration((context, config) =>
                {
                    var credential = new ClientSecretCredential(
                        Environment.GetEnvironmentVariable("TenantId"),
                        Environment.GetEnvironmentVariable("ClientId"),
                        Environment.GetEnvironmentVariable("ClientSecret"));
                    config.AddAzureKeyVault(new Uri(Environment.GetEnvironmentVariable("VaultUrl")), credential, new DottableKeyVaultSecretManager());
                })
                .ConfigureServices((context, services) =>
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
                    var index = configuration["ApplicationSettings:IndexName"];
                    services.AddAzureClients(options =>
                    {

                        options.UseCredential(credential);
                        options.AddSearchClient(new Uri(searchServiceEndpoint), index, new AzureKeyCredential(searchApiKey));
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

                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddApplicationInsights(
                            configureTelemetryConfiguration: (config) =>
                                config.ConnectionString = applicationInsightsConnectionString,
                                configureApplicationInsightsLoggerOptions: (options) => { }
                            );
                        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("DocumentInvoice.Api", LogLevel.Information);
                    });

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddDbContext<DocumentInvoiceContext>(opt =>
                                opt.UseSqlServer(dbConnectionString));

                    services.AddScoped(typeof(IRepositoryFactory<>), typeof(RepositoryFactory<>));
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                            typeof(CreateCustomerCommandHandler).Assembly
                        ));
                })
                .Build();

host.Run();
