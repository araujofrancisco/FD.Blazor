using FD.Blazor.Examples.Data;
using FD.SampleData.Contexts;
using FD.SampleData.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace FD.Blazor.Examples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // set dbcontext factory options and register it as singleton to allow using memory database
            // until the service gets shutdown
            services.AddDbContextFactory<WeatherForecastDbContext>();
            services
                .AddSingleton<SampleData.Interfaces.IDbContextFactory<WeatherForecastDbContext>, DbContextFactory<WeatherForecastDbContext>>()
                // register the method to obtain a new context and creates the database if there is no a previous connection
                .AddScoped(p => p.GetRequiredService<SampleData.Interfaces.IDbContextFactory<WeatherForecastDbContext>>().CreateContext())
                // custom data access service
                .AddScoped<IDataService, DataService>();
            //services.AddSingleton<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            SeedDb(app.ApplicationServices);
        }

        // on generated data seed size will indicate how many records we want to create
        private const int seedSize = 500;

        /// <summary>
        /// Executes database seeder just after all application services has started.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void SeedDb(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<WeatherForecastDbContext>();
            DbInitializer<WeatherForecastDbContext>.Initialize(context, seedSize);
        }
    }
}
