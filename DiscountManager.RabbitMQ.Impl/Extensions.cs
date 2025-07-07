using DiscountManager.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DiscountManager.RabbitMQ.Impl
{
    public static class Extensions
    {
        public static string VirtualHost => "DISCOUNT_MANAGER_VHOST";
        public static string CodeCreationExchange => $"{VirtualHost}.CodeExchange";
        public static string CodeQueue => $"{VirtualHost}.code.message";
        public static string CodeQueueAndExchangeRoutingKey => "code.message";

        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .Configure<RabbitMqConfiguration>(options => configuration.GetSection(RabbitMqConfiguration.SECTION).Bind(options));

            services
                .AddSingleton(serviceProvider =>
                {
                    var options = serviceProvider.GetService<IOptions<RabbitMqConfiguration>>();

                    ArgumentNullException.ThrowIfNull(options?.Value, nameof(RabbitMqConfiguration));

                    return new ConnectionFactory
                    {
                        UserName = "discount_manager_user",
                        Password = "discount_manager_password",
                        HostName = "rabbitmq",
                        VirtualHost = "DISCOUNT_MANAGER_VHOST",
                    };
                });

            return services;
        }

        public static IServiceCollection AddRabbitMQProducer(this IServiceCollection services)
        {
            services.AddScoped<IMessageProducer<IEnumerable<string>>, CodeMessageProducer>();

            return services;
        }

        public static IServiceCollection AddRabbitMQConsumer(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
             {
                 cfg.RegisterServicesFromAssembly(typeof(BaseMessageClient<>).Assembly);
             });

            services.AddScoped<IMessageConsumer, CodeMessageConsumer>();

            return services;
        }
    }
}
