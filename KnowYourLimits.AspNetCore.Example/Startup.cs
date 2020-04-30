using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowYourLimits.Strategies.LeakyBucket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KnowYourLimits.AspNetCore.Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddLeakyBucketRateLimiting(c =>
            {
                c.AddDefaultConfiguration(new LeakyBucketConfiguration
                {
                    EnableHeaders = true,
                    HeaderPrefix = "X-TEST-",
                    LeakAmount = 2,
                    LeakRate = TimeSpan.FromSeconds(1),
                    MaxRequests = 40,
                    RequestCost = 1
                });
                c.AddConfiguration(ctx => ctx.Request.Path.StartsWithSegments("/test"),
                    new LeakyBucketConfiguration
                    {
                        EnableHeaders = true,
                        HeaderPrefix = "X-TEST2-",
                        LeakAmount = 4,
                        LeakRate = TimeSpan.FromSeconds(2),
                        MaxRequests = 40,
                        RequestCost = 1
                    });
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseLeakyBucketRateLimiting();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}