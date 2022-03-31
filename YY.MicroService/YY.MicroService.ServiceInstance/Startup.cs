using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SkyApm.Utilities.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YY.MicroService.Framework;
using YY.MicroService.Framework.ConsulExtend;
using YY.MicroService.Framework.HttpApiExtend;
using YY.MicroService.Interface;
using YY.MicroService.Service;

namespace YY.MicroService.ServiceInstance
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

            services.AddControllers();

            services.AddTransient<IUserService, UserService>();

            #region Consul Server IOC×¢²á
            services.Configure<ConsulRegisterOptions>(this.Configuration.GetSection("ConsulRegisterOptions"));
            services.Configure<ConsulClientOptions>(this.Configuration.GetSection("ConsulClientOptions"));
            services.AddConsulRegister();
            services.AddConsulDispatcher(ConsulDispatcherType.Polling);
            #endregion

            #region SkyWalking
            //¼Óskywalking¼à¿ØÁ´Â·
            services.AddSkyApmExtensions();
            #endregion

            services.AddHttpInvoker(options =>
            {
                options.Message = "This is Program's Message";
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "YY.MicroService.ServiceInstance", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "YY.MicroService.ServiceInstance v1"));
            }

            #region Consul×¢²á
            app.UseHealthCheckMiddleware("/Api/Health/Index");//ÐÄÌøÇëÇóÏìÓ¦
            app.ApplicationServices.GetService<IConsulRegister>()?.UseConsulRegist();
            #endregion

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
