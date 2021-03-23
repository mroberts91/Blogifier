using Blogifier.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Linq;

namespace Blogifier
{
	public class Program
	{
		public static void Main(string[] args)
		{
            try
            {
                var host = CreateHostBuilder(args).Build();

                CreateLogger(host);

			    using (var scope = host.Services.CreateScope())
			    {
				    var services = scope.ServiceProvider;
				    var dbContext = services.GetRequiredService<AppDbContext>();

				    try
				    {
					    if (dbContext.Database.GetPendingMigrations().Any())
						    dbContext.Database.Migrate();
				    }
				    catch { }
			    }

                Log.Information("Starting Host...");

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                throw;
            }
            finally
            {
                Log.Fatal("Host Shutdown ...");
                Log.CloseAndFlush();
            }
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			 Host.CreateDefaultBuilder(args)
				  .ConfigureWebHostDefaults(webBuilder =>
					  webBuilder
                        .UseSerilog()
					    .UseContentRoot(Directory.GetCurrentDirectory())
					    .UseStartup<Startup>()
                  );

        private static void CreateLogger(IHost host)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var config = host.Services.GetService<IConfiguration>();

            if (config != null)
            {
                logger.LogError("Checking if Conn String exists: {str}", config.GetValue<string>("Blogifier:ConnString"));
            }
            else
            {
                logger.LogError("Configuration is null");
            }

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Information))
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                    theme: AnsiConsoleTheme.Literate);

            if (config != null)
            {
                var logDB = config.GetValue<string>("Blogifier:ConnString");
                var sinkOpts = new MSSqlServerSinkOptions { TableName = "Logs" };
                var columnOpts = new ColumnOptions();
                columnOpts.Store.Remove(StandardColumn.Properties);
                columnOpts.Store.Remove(StandardColumn.Id);
                columnOpts.Store.Add(StandardColumn.LogEvent);
                columnOpts.LogEvent.DataLength = 2048;
                columnOpts.TimeStamp.NonClusteredIndex = true;

                loggerConfig
                    .WriteTo.MSSqlServer(
                        connectionString: logDB,
                        sinkOptions: sinkOpts,
                        columnOptions: columnOpts
                    );
            }


            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
