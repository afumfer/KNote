using KNote.DomainModel.Infrastructure;
using KNote.DomainModel.Services;
using KNote.Server.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace KNote.Server
{
    public class Startup
    {

        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            //// Para pruebas de uso del DbContext directo 
            //services.AddDbContext<KntDbContext>(options =>
            //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IKntService>(provider => new KntService(configuration["ConnectionStrings:DefaultConnection"]));
            //services.AddSingleton<IConfiguration>(configuration);

            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            //services.AddMvc();

            services.AddMvc().AddNewtonsoftJson(options =>
               options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UsePathBase("/KNote");

            app.UseStaticFiles();
            //app.UseClientSideBlazorFiles<Client.Program>();
            app.UseClientSideBlazorFiles<Client.Startup>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                //endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html");
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });
        }
    }
}
