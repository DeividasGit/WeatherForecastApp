using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherForecastApp.Interfaces;

namespace WeatherForecastApp.Services
{
  public class ConsoleService : IConsoleService
  {
    private static readonly object ConsoleLock = new object();
    private static int inputRow = Console.WindowHeight - 1;

    public void PrintLog(string message)
    {
      lock (ConsoleLock)
      {
        var cursorLeft = Console.CursorLeft;

        Console.MoveBufferArea(0, 1, Console.WindowWidth, inputRow - 1, 0, 0);

        Console.SetCursorPosition(0, inputRow - 1);
        Console.WriteLine(message);

        Console.SetCursorPosition(cursorLeft, inputRow);
      }
    }

    public void PrintUserInput(string text)
    {
      lock (ConsoleLock)
      {
        Console.SetCursorPosition(0, inputRow);
        Console.Write(text);
      }
    }

    public void PrintHelp()
    {
      PrintLog("Type 'check' if you want to check current weather prediction for your location\n" +
               "Type 'exit' if you want to exit\n" +
               "Type 'help' for instructions\n");
    }

    public string ReadLine()
    {
      return Console.ReadLine()?.Trim() ?? string.Empty;
    }
  }
}
