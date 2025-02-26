using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherForecastApp.Models {
  public class HourlyPrecipitationProbability 
  {
    public List<DateTime> Time { get; set; }
    [JsonPropertyName("precipitation_probability")]
    public List<double> Probability { get; set; }
  }
}
