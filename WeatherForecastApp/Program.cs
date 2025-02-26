using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WeatherForecastApp.Interfaces;
using WeatherForecastApp.Models;
using WeatherForecastApp.Services;
using WeatherForecastApp.Settings;

internal class Program 
{
  private static async Task Main(string[] args) 
  {
    using IHost host = Host.CreateDefaultBuilder(args)
      .ConfigureLogging(logging => 
      {
        logging.ClearProviders();
        logging.AddDebug();
      })
      .ConfigureServices(services => {
        IConfiguration configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", false, true)
          .Build();

        services.Configure<WeatherForecastSettings>(configuration.GetSection("WeatherForecastSettings"));

        services.AddHttpClient<IWeatherService, WeatherService>();
        services.AddSingleton<IWeatherService, WeatherService>();
        services.AddSingleton<IConsoleService, ConsoleService>();
      }).Build();

    var weatherService = host.Services.GetRequiredService<IWeatherService>();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    try 
    {
      var cts = new CancellationTokenSource();

      var periodicalTask = Task.Run(() => weatherService.CheckPeriodically(cts.Token));
      var inputTask = Task.Run(() => weatherService.CheckOnInput(cts.Token));

      await Task.WhenAll(periodicalTask, inputTask);
    } 
    catch (Exception ex)
    {
      logger.LogError(ex.Message);
    }
  }
}