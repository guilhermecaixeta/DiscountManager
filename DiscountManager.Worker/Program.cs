using Autofac.Core;
using DiscountManager.Core;
using DiscountManager.RabbitMQ.Impl;
using Microsoft.EntityFrameworkCore;

namespace DiscountManager.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var connectionString = builder
              .Configuration
              .GetConnectionString(DiscountDbContext.CONNECTION_STRING_NAME);

            builder
                .Services
                .AddDbContext<DiscountDbContext>(options => options
                    .UseNpgsql(connectionString, config => config.MigrationsAssembly(typeof(Program).Assembly.ToString()))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll));

            builder.Services
                .AddRabbitMQ(builder.Configuration)
                .AddRabbitMQConsumer();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}