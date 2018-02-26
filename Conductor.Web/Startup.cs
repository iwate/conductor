using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conductor.Core.Repositories;
using Conductor.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Conductor.Web
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
            services.AddSingleton<IJobRegistry, JobRegistry>();
            services.AddTransient<IBlobRepository, BlobRepository>();
            services.AddTransient<IJobDefinitionRepository, JobDefinitionRepository>();
            services.AddTransient<IJobResultRepository, JobResultRepository>();
            services.AddTransient<IJobService, JobService>();
            services.AddTransient<IACIService, ACIService>();
            services.AddTransient<ILockFactory, LockFactory>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var services = app.ApplicationServices;
            var jobService = services.GetRequiredService<IJobService>();
            var jobRegistry = services.GetRequiredService<IJobRegistry>();
            var lockFactory = services.GetRequiredService<ILockFactory>();
            var aciService = services.GetRequiredService<IACIService>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            var cronWorker = new CronWorker(jobService, jobRegistry, lockFactory, aciService, loggerFactory.CreateLogger("CronWorker"));
            var queueWorker = new QueueWorker(jobService, jobRegistry, lockFactory, aciService, loggerFactory.CreateLogger("QueueWorker"));

            lifetime.ApplicationStopping.Register(() => {
                cronWorker.Stop();
                queueWorker.Stop();
            });

            cronWorker.Start();
            queueWorker.Start();
        }
    }
}
