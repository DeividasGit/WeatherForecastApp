using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherForecastApp.Models {
  public class PrecipitationProbabilityResponse 
  {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; }
    public double Elevation { get; set; }
    [JsonPropertyName("hourly")]
    public HourlyPrecipitationProbability HourlyData { get; set; }
  }
}
