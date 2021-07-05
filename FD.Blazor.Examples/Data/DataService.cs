using FD.Blazor.Core;
using FD.SampleData.Contexts;
using FD.SampleData.Models.Weather;
using FD.SampleData.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FD.Blazor.Examples.Data
{
    public interface IDataService
    {
        Task<List<ReportType>> GetReportTypes();
        Task<List<WeatherForecast>> GetForecastAsync();
    }

    public class DataService : IDataService
    {
        private readonly ILogger<DataService> _logger;
        private readonly WeatherForecastDbContext _context;

        public DataService(ILogger<DataService> logger, WeatherForecastDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<ReportType>> GetReportTypes()
        {
            return await new WeatherForecastService(_context).GetReportTypes(null, "Name", SortDirection.Ascending, 0, 100);
        }

        public async Task<List<WeatherForecast>> GetForecastAsync()
        {
            return await new WeatherForecastService(_context).GetForecastAsync(null, "Date", SortDirection.Ascending, 0, 100);
        }
    }
}