using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherForecastApp.Models;

namespace WeatherForecastApp.Interfaces {
  public interface IWeatherService 
  {
    Task<PrecipitationProbabilityResponse> GetPrecipitationProbability(double latitude, double longitude);
    Task NotifyPrecipitation(double latitude, double longitude);
    Task CheckPeriodically(CancellationToken token);
    Task CheckOnInput(CancellationToken token);
  }
}
