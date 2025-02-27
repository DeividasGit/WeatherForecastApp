using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherForecastApp.Interfaces;
using WeatherForecastApp.Models;
using WeatherForecastApp.Settings;

namespace WeatherForecastApp.Services {
  public class WeatherService : IWeatherService
  {
    private readonly WeatherForecastSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly IConsoleService _consoleService;
    public WeatherService(IOptions<WeatherForecastSettings> settings, HttpClient httpClient, ILogger<WeatherService> logger, IConsoleService consoleService) 
    {
      _settings = settings.Value;
      _httpClient = httpClient;
      _logger = logger;
      _consoleService = consoleService;
    }

    public async Task<PrecipitationProbabilityResponse> GetPrecipitationProbability(double latitude, double longitude) 
    {
      try 
      {
        var url = _settings.ApiUrl +
                  $"forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                  $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                  $"&hourly=precipitation_probability" +
                  $"&forecast_days={_settings.ForecastDays}";
                   
        var httpResponse = await _httpClient.GetAsync(url);
        if (!httpResponse.IsSuccessStatusCode)
        {
          _logger.LogError($"Error: {httpResponse.StatusCode}");
        }

        var result = await httpResponse.Content.ReadAsStringAsync();

        var data = JsonSerializer.Deserialize<PrecipitationProbabilityResponse>(result, new JsonSerializerOptions() 
        {
          PropertyNameCaseInsensitive = true
        });

        return data;
      } 
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        throw;
      }
    }

    public async Task NotifyPrecipitation(double latitude, double longitude) 
    {
      try 
      {
        var data = await GetPrecipitationProbability(latitude, longitude);
        if (data.HourlyData == null)
        {
          _logger.LogError("No weather data found");
          _consoleService.PrintLog("No weather data found");
          return;
        }

        var currentDate = DateTime.UtcNow;

        var index = data.HourlyData.Time.FindIndex(x => x.CompareTo(currentDate) >= 0);
        if (index == -1) 
        {
          _logger.LogError("No future data");
          _consoleService.PrintLog("No future data");
          return;
        }

        for (var i = index; i < index + _settings.WarningHours && i < data.HourlyData.Time.Count; i++) 
        {
          if (data.HourlyData.Probability[i] >= Convert.ToDouble(_settings.WarningThresholdPercent)) 
          {
            _logger.LogInformation($"Grab an umbrella on {data.HourlyData.Time[i].ToLocalTime()}, " +
                                   $"precipitation probability: {data.HourlyData.Probability[i]} %");

            _consoleService.PrintLog($"[ALERT] Grab an umbrella on {data.HourlyData.Time[i].ToLocalTime()}, " +
                                   $"precipitation probability: {data.HourlyData.Probability[i]} %");

            return;
          }
        }
        _logger.LogInformation($"No precipitation warnings in the next {_settings.WarningHours} hours");
        _consoleService.PrintLog($"[INFO] No precipitation warnings in the next {_settings.WarningHours} hours");
      } 
      catch (Exception ex) 
      {
        _logger.LogError(ex.Message);
      }
    }

    public async Task CheckPeriodically(CancellationToken token) 
    {
      try
      {
        while (!token.IsCancellationRequested)
        {
          _consoleService.PrintLog("[WEATHER UPDATE] Checking precipitation...");

          await NotifyPrecipitation(_settings.Latitude, _settings.Longitude);

          await Task.Delay(TimeSpan.FromSeconds(_settings.WarningFrequencyInSeconds), token);
        }
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogWarning($"Operation was canceled: {ex.Message}");
      }
    }

    public async Task CheckOnInput(CancellationToken token)
    {
      double latitude, longitude;

      _consoleService.PrintHelp();

      while (!token.IsCancellationRequested)
      {

        _consoleService.PrintUserInput("> ");
        var input = _consoleService.ReadLine();

        if (input == "exit")
        {
          _consoleService.PrintLog("Exiting app...");
          Environment.Exit(0);
        }
        else if (input == "help")
        {
          _consoleService.PrintHelp();
        }
        else if (input == "check")
        {
          _consoleService.PrintUserInput("> Enter Latitude: ");
          var latitudeInput = _consoleService.ReadLine();

          if (!double.TryParse(latitudeInput, NumberStyles.Any, CultureInfo.InvariantCulture, out latitude))
          {
            _consoleService.PrintLog("Latitude should be a number");
            continue;
          }

          _consoleService.PrintUserInput("> Enter Longitude: ");
          var longitudeInput = _consoleService.ReadLine();

          if (!double.TryParse(longitudeInput, NumberStyles.Any, CultureInfo.InvariantCulture, out longitude))
          {
            _consoleService.PrintLog("Longitude should be a number");
            continue;
          }

          _consoleService.PrintLog("[CUSTOM UPDATE] Checking cutom location precipitation...");
          await NotifyPrecipitation(latitude, longitude);
        }
      }

    }

  }
}
