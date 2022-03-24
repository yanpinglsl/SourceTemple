using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YY.MicroService.Framework.ConsulExtend;
using YY.MicroService.Framework.HttpApiExtend;
using YY.MicroService.Interface;
using YY.MicroService.Model;
using YY.MicroService.Service;

namespace YY.MicroService.OrderServiceInstance
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

            #region Consul Server IOC×¢²á
            services.Configure<ConsulRegisterOptions>(this.Configuration.GetSection("ConsulRegisterOptions"));
            services.Configure<ConsulClientOptions>(this.Configuration.GetSection("ConsulClientOptions"));
            services.AddConsulRegister();
            services.AddConsulDispatcher(ConsulDispatcherType.Polling);
            #endregion

            services.AddHttpInvoker(options =>
            {
                options.Message = "This is Program's Message";
            });
            services.AddSingleton<IUserService, UserService>();
            services.AddTransient<IOrderService, OrderService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "YY.MicroService.OrderServiceInstance", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "YY.MicroService.OrderServiceInstance v1"));
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
