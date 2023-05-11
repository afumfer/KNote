using System.Text;
using System;
using System.Linq;

using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;

using KNote.Model;
using KNote.Repository;
using KNote.Server.Helpers;
using KNote.Service.Core;
using EF = KNote.Repository.EntityFramework;
using DP = KNote.Repository.Dapper;
using KNote.Server.Hubs;
using KNote.MessageBroker.RabbitMQ;
using System.Text.Json.Serialization;


/////////////////////////////////////////////////////////////////
/// Create HostBuilder
/////////////////////////////////////////////////////////////////

var builder = WebApplication.CreateBuilder(args);


/////////////////////////////////////////////////////////////////
/// Configure the application and add services to the container.
/////////////////////////////////////////////////////////////////

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
var connectionStrings = builder.Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();

var repositoryRef = new RepositoryRef
{
    Alias = KntConst.AppName,
    ConnectionString = connectionStrings.Connection,
    Provider = connectionStrings.Provider,
    Orm = connectionStrings.ORM,
    ResourceContentInDB = appSettings.ResourcesContentInDB,
    ResourcesContainer = appSettings.ResourcesContainer,
    ResourcesContainerCacheRootPath = appSettings.ResourcesContainerRootPath,
    ResourcesContainerCacheRootUrl = appSettings.ResourcesContainerRootUrl
};

if (connectionStrings.ORM == "Dapper")
    builder.Services.AddScoped<IKntRepository>(provider => new DP.KntRepository(repositoryRef));
else if (connectionStrings.ORM == "EntityFramework")
    builder.Services.AddScoped<IKntRepository>(provider => new EF.KntRepository(repositoryRef));

builder.Services.AddScoped<IKntService, KntService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = false,
         ValidateAudience = false,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.Secret)),
         ClockSkew = TimeSpan.Zero
     });

builder.Services.AddCors(p => p.AddPolicy("KntPolicy", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddScoped<IFileStore, LocalFileStore>();
builder.Services.AddHttpContextAccessor();

//builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
//   options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddSingleton<KntMessageBroker>();

/////////////////////////////////////////////////////////////////
/// Build App and configure the HTTP request pipeline.
/////////////////////////////////////////////////////////////////

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapFallbackToFile("index.html");

// Experimental ------------------------------------------------------------------------
//
// To use the message broker force an instance of the service layer at the application
// level with the use of the enable activeMessageBroker parameter in its constructor.
//
//var eventBus = app.Services.GetRequiredService<KntService>();
//eventBus.xxx
//
KntService kntService;
if (appSettings.ActivateMessageBroker)
{
    if (connectionStrings.ORM == "Dapper")
        kntService = new KntService(new DP.KntRepository(repositoryRef), true);
    else if (connectionStrings.ORM == "EntityFramework")
        kntService = new KntService(new EF.KntRepository(repositoryRef), true);
}
// -------------------------------------------------------------------------------------

app.Run();
