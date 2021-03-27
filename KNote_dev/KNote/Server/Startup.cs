using KNote.Service.Services;
using KNote.Service;
using KNote.Server.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;

using KNote.Repository;
using KNote.Model;
using EF = KNote.Repository.EntityFramework;
using DP = KNote.Repository.Dapper;

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
            //services.AddCors();
            services.AddCors( o => o.AddPolicy("KntPolicy", buider =>
            {
                buider.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            // For test, use DbContext 
            //services.AddDbContext<KntDbContext>(options =>
            //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            // Inject repository (see appconfig).
            var orm = configuration["ConnectionStrings:DefaultORM"]; ;
            var prov = configuration["ConnectionStrings:DefaultProvider"];
            var conn = configuration["ConnectionStrings:DefaultConnection"];

            if(orm == "Dapper")
                services.AddScoped<IKntRepository>(provider => new DP.KntRepository(conn, prov));
            else if (orm == "EntityFramework")                
                services.AddScoped<IKntRepository>(provider => new EF.KntRepository(conn, prov));

            services.AddScoped<IKntService, KntService>();

            //services.AddSingleton<IConfiguration>(configuration);

            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            
            var appSettings = appSettingsSection.Get<AppSettings>();

            // TODO: Experimental  .....
            //   Implementar en el futuro. Si hay distinta a la app para la caché de recursos
            //   entonces hay que repensar la inserción de recursos dentro de los textos. 
            //   habría que incluirla / o la url completa. (Estudiar alternativas). 
            //   Par ahora forzamos que no haya una raíz alternativa (o directorio virtual)
            //   para la caché de recursos.
            // KntConst.ContainerResourcesRootPath = appSettings.ContainerResourcesRootPath;
            // KntConst.ContainerResourcesRootPath = "D:\\Resources\\knt";
            // KntConst.ContainerResources = appSettings.ContainerResources;
            // KntConst.ContainerResourcesRootUrl = appSettings.ContainerResourcesRootUrl;
            //
            KntConst.ContainerResources = "NotesResources";
            KntConst.ContainerResourcesCacheRootPath = "";
            KntConst.ContainerResourcesCacheRootUrl = "";
            // .......................................

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = false,
                     ValidateAudience = false,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(
                         // Encoding.UTF8.GetBytes(configuration["jwt:key"]) 
                         Encoding.ASCII.GetBytes(appSettings.Secret)
                        ),
                     ClockSkew = TimeSpan.Zero
                 });

            services.AddScoped<IFileStore, LocalFileStore>();
            services.AddHttpContextAccessor();

            services.AddControllersWithViews().AddNewtonsoftJson(options =>
               options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddRazorPages();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UsePathBase("/KNote");

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
                        
            // global cors policy
            //app.UseCors(x => x
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .AllowCredentials());
            
            app.UseCors("KntPolicy");

            app.UseEndpoints(endpoints =>
            {                
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
