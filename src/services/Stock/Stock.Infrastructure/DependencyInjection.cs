﻿using Core.Infrastructure;
using Core.Infrastructure.DependencyModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stock.Domain.Interfaces;
using Stock.Infrastructure.Persistence;
using System.Reflection;

namespace Stock.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
            IConfiguration configuration,
            Assembly executingAssembly,
            string baseDirectory, 
            Action<MessageBusOptions> messageBusOptions = null)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            services.AddDbContext<StockDbContext>(opt => {
                opt.UseNpgsql(configuration.GetConnectionString("Database"));
            });

            services.AddCoreInfrastructure(opt => {

                // Distributed Cache
                opt.EnableDistributedCache = true;
                opt.DistributedCacheOptions = new Core.Infrastructure.DependencyModels.DistributedCacheOptions
                {
                    Database = Convert.ToInt32(configuration["DistributedCache:Database"]),
                    Endpoints = configuration["DistributedCache:Endpoints"],
                    Password = configuration["DistributedCache:Password"]
                };

                // Message Broker - Bus
                if (messageBusOptions != null)
                {
                    opt.EnableMessageBus = true;

                    services.AddOptions<MessageBusOptions>().Configure(messageBusOptions);

                    var messageBusOpt = services.BuildServiceProvider().GetService<IOptions<MessageBusOptions>>();

                    opt.MessageBusOptions = messageBusOpt.Value;
                }

                // Service Registry - Consul
                opt.EnableServiceRegistry = true;
                opt.ServiceRegistryOptions = configuration.GetSection("ServiceRegistry");

                // Authentication
                opt.EnableAuthentication = true;
                opt.TokenOptions = configuration.GetSection("TokenOptions");
                opt.SwaggerOptions = new SwaggerOptions
                {
                    ApiAssembly = executingAssembly,
                    BaseDirectory = baseDirectory
                };
            });

            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IStockUnitOfWork, StockUnitOfWork>();
            services.Decorate<IStockRepository, CachedStockRepository>();
            return services;
        }

        public static ILoggingBuilder AddCustomLogging(this ILoggingBuilder builder, IConfiguration configuration)
        {
            builder.AddCoreLogging(configuration, options => {
                options.EnableElasticLogging = true;
                options.ApplicationName = "Stock";
                options.ElasticUri = configuration["ElasticConfiguration:Uri"];
            });

            return builder;
        }
    }
}
