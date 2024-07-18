using Azure;
using Azure.AI.OpenAI;
using GenOrcAdvisor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            var appBuilderSettings = new HostApplicationBuilderSettings()
            {
                EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Microsoft.Extensions.Hosting.Environments.Production,
                Args = args,
                ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
            };

            var builder = Host.CreateApplicationBuilder(appBuilderSettings);

            builder.Configuration.AddEnvironmentVariables();

            builder.Services.Configure<AzureAISettings>(builder.Configuration.GetSection("AzureAI"));
            builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

            #region Updating Configuration read from Envrionment Variable Secrets

            builder.Services.Configure<AzureAISettings>(azureAIConfig =>
            {
                var azureAISettings = builder.Configuration.GetSection("AzureAI").Get<AzureAISettings>() ?? throw new InvalidOperationException($"Couldn't find AzureAISettings configuration");
                var API_KEY = builder.Configuration["AzureAI:API_KEY"];
                if (string.IsNullOrWhiteSpace(API_KEY))
                    throw new ArgumentNullException("ConnectionString", "No ConnectionString Provided");

                azureAISettings.API_KEY = API_KEY;
            });

            builder.Services.Configure<MongoDBSettings>(mongoConfig =>
            {
                var mongoDBSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>() ?? throw new InvalidOperationException($"Couldn't find MongoDBSettings configuration");
                var ConnectionString = builder.Configuration["MongoDB:ConnectionString"];
                if (string.IsNullOrWhiteSpace(ConnectionString))
                    throw new ArgumentNullException("ConnectionString", "No ConnectionString Provided");

                mongoDBSettings.ConnectionString = ConnectionString;
            });

            #endregion

            builder.Services.AddSingleton<OrderAdviceGenerator>();
            builder.Services.AddSingleton<MongoDataAccessorService>();

            builder.Services.AddHostedService<GenOrcAdvisorBackgroundService>();

            var app = builder.Build();

            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Global Exception occured...\n\n Details::{ex.ToString()}");
        }
    }
}