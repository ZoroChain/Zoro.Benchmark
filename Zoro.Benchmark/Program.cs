using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Zoro.Benchmark.Services;

namespace Zoro.Benchmark
{
    class Program
    {
        public static IConfigurationRoot configuration;

        private static ILoggerFactory _loggerFactory = new LoggerFactory()
                //.AddDebug(LogLevel.Information)
                //.AddConsole(LogLevel.Information));
                .AddDebug(LogLevel.Trace)
                .AddConsole(LogLevel.Trace);

        private static ILogger _logger = _loggerFactory.CreateLogger("Zoro.Benchmark");

        /// <summary>
        /// Add all the services need to be tested in this method.
        /// Sample: serviceCollection.AddTransient<IChainService, TransactionService>();
        /// </summary>
        /// <param name="serviceCollection"></param>
        private static void ConfigureNeoServices(IServiceCollection serviceCollection)
        {
            // Add TransactionService
            serviceCollection.AddTransient<IChainService, TransactionService>();
            // Add WalletService
            serviceCollection.AddTransient<IChainService, WalletService>();
            // Add Other Services
        }

        static void Main(string[] args)
        {
            try
            {
                // Create service collection
                ServiceCollection serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                // create neo services collection
                ConfigureNeoServices(serviceCollection);

                // Create service provider
                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

                // Run service
                IEnumerator<IChainService> services = serviceProvider.GetServices<IChainService>().GetEnumerator();
                while (services.MoveNext())
                {
                    IChainService service = services.Current;
                    service.Run(new Dictionary<string, object>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                _logger.LogError(ex.StackTrace);
            }
            
        }

        /// <summary>
        /// Configure Non Neo Services such as logging, configuration, etc
        /// </summary>
        /// <param name="serviceCollection"></param>
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Add logging
            serviceCollection.AddSingleton(_loggerFactory);

            serviceCollection.AddLogging();

            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
        }
    }
}
