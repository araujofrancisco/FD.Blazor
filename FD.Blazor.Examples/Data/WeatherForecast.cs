using System;

namespace FD.Blazor.Examples.Data
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string Summary { get; set; }
        public bool Selected { get; set; } = false;
        public int DaylightTime { get; set; }
        public string Phone { get; set; }
        public DateTime? WhenUpdated { get; set; }
    }
}
