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
using Microsoft.AspNetCore.Http;
using KNote.Server.Hubs;
using KNote.Service.Core;
using System.Net.Http;
using System.Security.Claims;
using static Dapper.SqlMapper;
using System.Threading;

namespace KNote.Server;

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
        services.AddCors( o => o.AddPolicy("KntPolicy", buider =>
        {
            buider.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));

        var appSettingsSection = configuration.GetSection("AppSettings");
        services.Configure<AppSettings>(appSettingsSection);
        var appSettings = appSettingsSection.Get<AppSettings>();

        var orm = configuration["ConnectionStrings:DefaultORM"];
        var prov = configuration["ConnectionStrings:DefaultProvider"];
        var conn = configuration["ConnectionStrings:DefaultConnection"];  
        
        var repositoryRef = new RepositoryRef
        {
            Alias = "KaNote",
            ConnectionString = conn,
            Provider = prov,
            Orm = orm,
            ResourceContentInDB = appSettings.ResourcesContentInDB,
            ResourcesContainer = appSettings.ResourcesContainer,
            ResourcesContainerCacheRootPath = appSettings.ResourcesContainerRootPath,
            ResourcesContainerCacheRootUrl = appSettings.ResourcesContainerRootUrl
        };

        if(orm == "Dapper")                
            services.AddScoped<IKntRepository>(provider => new DP.KntRepository(repositoryRef));
        else if (orm == "EntityFramework")                                
            services.AddScoped<IKntRepository>(provider => new EF.KntRepository(repositoryRef));

        services.AddScoped<IKntService, KntService>();

        #region Doc, test
        // For test, use DbContext 
        //services.AddDbContext<KntDbContext>(options =>
        //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        #endregion 
        
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
        
        services.AddSignalR();

        services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
        });
        
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) // 
    {        
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
        app.UseCors("KntPolicy");            

        app.UseEndpoints(endpoints =>
        {                
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHub<ChatHub>("/chathub");
            endpoints.MapFallbackToFile("index.html");
        });
    }
}
