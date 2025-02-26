using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Settings {
  public class WeatherForecastSettings 
  {
    public string ApiUrl { get; set; }
    public int ForecastDays { get; set; }
    public double WarningThresholdPercent { get; set; }
    public int WarningFrequencyInSeconds { get; set; }
    public int WarningHours { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
  }
}
