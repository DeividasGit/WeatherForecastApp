using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using WeatherForecastApp.Interfaces;
using WeatherForecastApp.Models;
using WeatherForecastApp.Services;
using WeatherForecastApp.Settings;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WeatherForecastApp.Tests
{
  public class WeatherServiceTests
  {
    private readonly Mock<IOptions<WeatherForecastSettings>> _mockSettings;
    private readonly Mock<ILogger<WeatherService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _mockHttpClient;
    private readonly WeatherService _weatherService;
    private readonly Mock<IConsoleService> _mockConsoleService;

    public WeatherServiceTests()
    {
      _mockSettings = new Mock<IOptions<WeatherForecastSettings>>();
      _mockLogger = new Mock<ILogger<WeatherService>>();
      _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
      _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object);
      _mockConsoleService = new Mock<IConsoleService>();

      var settings = new WeatherForecastSettings()
      {
        ApiUrl = "https://test.com/v1/",
        WarningHours = 5,
        WarningThresholdPercent = 20,
        Latitude = 40.7128,
        Longitude = 24.0060,
        WarningFrequencyInSeconds = 60,
        ForecastDays = 2
      };

      _mockSettings.Setup(s => s.Value).Returns(settings);

      _weatherService = new WeatherService(_mockSettings.Object, _mockHttpClient, _mockLogger.Object, _mockConsoleService.Object);
    }

    [Fact]
    public async Task NotifyPrecipitation_ShouldLogMessage_WhenPrecipitationProbabilityIsHigh()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;
      var date = DateTime.UtcNow.AddHours(1);
      var probability = 20;

      SetupHttpMockResponse(latitude, longitude, date, probability);

      await _weatherService.NotifyPrecipitation(latitude, longitude);

      _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Grab an umbrella")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
              Times.Once);

    }

    [Fact]
    public async Task NotifyPrecipitation_ShouldNotLogMessage_WhenPrecipitationProbabilityIsTooLow()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;
      var date = DateTime.UtcNow.AddHours(1);
      var probability = 2;

      SetupHttpMockResponse(latitude, longitude, date, probability);

      await _weatherService.NotifyPrecipitation(latitude, longitude);

      _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Grab an umbrella")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
              Times.Never);
    }

    [Fact]
    public async Task NotifyPrecipitation_ShouldLogInformation_WhenPrecipitationProbabilityIsTooLow()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;
      var date = DateTime.UtcNow.AddHours(1);
      var probability = 2;

      SetupHttpMockResponse(latitude, longitude, date, probability);

      await _weatherService.NotifyPrecipitation(latitude, longitude);

      _mockLogger.Verify(
            logger => logger.Log(
              LogLevel.Information,
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No precipitation warnings")), 
              null,
              It.IsAny<Func<It.IsAnyType, Exception, string>>()
              ),
            Times.Once);
    }

    [Fact]
    public async Task NotifyPrecipitation_ShouldLogError_WhenNoFutureData()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;
      var date = DateTime.UtcNow.AddHours(-1);
      var probability = 20;

      SetupHttpMockResponse(latitude, longitude, date, probability);

      await _weatherService.NotifyPrecipitation(longitude, latitude);

      _mockLogger.Verify(
          logger => logger.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No future data")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
          Times.Once);
    }

    [Fact]
    public async Task NotifyPrecipitation_ShouldLogError_WhenNoApiData()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;

      var mockApiResponse = "{}";

      _mockHttpMessageHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
          )
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(mockApiResponse, Encoding.UTF8, "application/json")
          });

      await _weatherService.NotifyPrecipitation(longitude, latitude);

      _mockLogger.Verify(
          logger => logger.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No weather data")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
          Times.Once);
    }

    [Fact]
    public async Task CheckOnInput_ShouldLogMessage_WhenPrecipitationProbabilityIsHigh()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;
      var date = DateTime.UtcNow.AddHours(1);
      var probability = 20;

      _mockConsoleService.SetupSequence(cs => cs.ReadLine())
          .Returns("check")
          .Returns("10")    // Latitude
          .Returns("22.2"); // Longitude

      SetupHttpMockResponse(latitude, longitude, date, probability);

      var cts = new CancellationTokenSource();
      var task = Task.Run(() => _weatherService.CheckOnInput(cts.Token));

      await Task.WhenAny(task, Task.Delay(5000));
      cts.Cancel();          

      await task;

      _mockLogger.Verify(
          logger => logger.Log(
              LogLevel.Information,
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Grab an umbrella")),
              null,
              It.IsAny<Func<It.IsAnyType, Exception, string>>()
              ),
            Times.Once);
    }

    [Fact]
    public async Task CheckOnInput_ShouldNotLogMessage_WhenPrecipitationProbabilityIsTooLow()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;
      var date = DateTime.UtcNow.AddHours(1);
      var probability = 10;

      _mockConsoleService.SetupSequence(cs => cs.ReadLine())
          .Returns("check")
          .Returns("10")    // Latitude
          .Returns("22.2"); // Longitude

      SetupHttpMockResponse(latitude, longitude, date, probability);

      var cts = new CancellationTokenSource();
      var task = Task.Run(() => _weatherService.CheckOnInput(cts.Token));

      await Task.WhenAny(task, Task.Delay(5000));
      cts.Cancel();

      await task;

      _mockLogger.Verify(
          logger => logger.Log(
              LogLevel.Information,
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Grab an umbrella")),
              null,
              It.IsAny<Func<It.IsAnyType, Exception, string>>()
              ),
            Times.Never);
    }

    [Fact]
    public async Task CheckPeriodically_ShouldLogMessage_WhenPrecipitationProbabilityIsHigh()
    {
      var latitude = _mockSettings.Object.Value.Latitude;
      var longitude = _mockSettings.Object.Value.Longitude;
      var date = DateTime.UtcNow.AddHours(1);
      var probability = 20;

      SetupHttpMockResponse(latitude, longitude, date, probability);

      var cts = new CancellationTokenSource();
      var task = Task.Run(() => _weatherService.CheckPeriodically(cts.Token));

      await Task.WhenAny(task, Task.Delay(5000));
      cts.Cancel();

      await task;

      _mockLogger.Verify(
          logger => logger.Log(
              LogLevel.Information,
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Grab an umbrella")),
              null,
              It.IsAny<Func<It.IsAnyType, Exception, string>>()
              ),
            Times.Once);
    }

    private void SetupHttpMockResponse(double latitude, double longitude, DateTime date, double probability)
    {
      var mockApiResponse = $$"""
        {
            "latitude": {{latitude.ToString(CultureInfo.InvariantCulture)}},
            "longitude": {{longitude.ToString(CultureInfo.InvariantCulture)}},
            "hourly": {
                "time": [
                    "{{date.ToString("yyyy-MM-ddTHH:00", CultureInfo.InvariantCulture)}}"
                ],
                "precipitation_probability": [
                    {{probability.ToString(CultureInfo.InvariantCulture)}}
                ]
            }
        }
        """;

      _mockHttpMessageHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
          )
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(mockApiResponse, Encoding.UTF8, "application/json")
          });
    }





  }
}