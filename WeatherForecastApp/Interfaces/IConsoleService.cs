using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Interfaces
{
  public interface IConsoleService
  {
    void PrintLog(string message);
    void PrintUserInput(string text);
    void PrintHelp();
    string ReadLine();
  }
}
