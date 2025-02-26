using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherForecastApp.Interfaces;
using WeatherForecastApp.Services;

namespace WeatherForecastApp.Tests
{
  public class ConsoleServiceTests
  {
    private readonly Mock<IConsoleService> _mockConsoleService;
    public ConsoleServiceTests() 
    {
      _mockConsoleService = new Mock<IConsoleService>();
    }

    [Fact]
    public void PrintLog_ShouldPrintAMessage()
    {
      var message = "test";

      _mockConsoleService.Object.PrintLog(message);

      _mockConsoleService.Verify(
        x => x.PrintLog(
          It.Is<string>(x => x == message)),
        Times.Once);
    }

    [Fact]
    public void PrintUserInput_ShouldPrintAMessage()
    {
      var message = "test";

      _mockConsoleService.Setup(
        x => x.PrintUserInput(
          It.IsAny<string>()))
        .Callback<string>(msg => Console.Write(msg));

      _mockConsoleService.Object.PrintUserInput(message);

      _mockConsoleService.Verify(
        x => x.PrintUserInput(
          It.Is<string>(x => x == message)),
        Times.Once);
    }

    [Fact]
    public void PrintHelp_ShouldPrintHelpMessage()
    {
      _mockConsoleService.Setup(
        x => x.PrintHelp())
        .Callback(() => _mockConsoleService.Object.PrintLog(
          "Type 'check' if you want to check current weather prediction for your location\n" +
          "Type 'exit' if you want to exit\n" +
          "Type 'help' for instructions\n"));

      _mockConsoleService.Object.PrintHelp();

      _mockConsoleService.Verify(
        x => x.PrintLog(
          It.Is<string>(x => x.Contains("Type 'check' if you want to check current weather prediction for your location"))),
        Times.Once);
    }
  }
}
