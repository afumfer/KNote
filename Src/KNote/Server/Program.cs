using System.Text;
using System;
using System.Linq;
using System.Text.Json.Serialization;
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

using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Hosting;

/////////////////////////////////////////////////////////////////
/// Create HostBuilder
/////////////////////////////////////////////////////////////////

var builder = WebApplication.CreateBuilder(args);


/////////////////////////////////////////////////////////////////
/// Configure the application and add services to the container.
/////////////////////////////////////////////////////////////////

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
var repositoryRefSection = builder.Configuration.GetSection("RepositoryRef");

builder.Services.Configure<AppSettings>(appSettingsSection);
builder.Services.Configure<RepositoryRef>(repositoryRefSection);

var appSettings = appSettingsSection.Get<AppSettings>();
var repositoryRef = repositoryRefSection.Get<RepositoryRef>();

if (repositoryRef.Orm == "Dapper")
    builder.Services.AddScoped<IKntRepository>(provider => new DP.KntRepository(repositoryRef));
else if (repositoryRef.Orm == "EntityFramework")
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

// Experimental ------------------------------------------------------------------------
//
//  TODO: Convert in extension method
//
// To use the message broker force an instance of the service layer at the application
// level with the use of the enable activeMessageBroker parameter in its constructor.
// (No use: "var kntServiceForMessageBroker = app.Services.GetRequiredService<KntService>();")
//
KntService kntServiceForMessageBroker;
if (appSettings.ActivateMessageBroker)
{
    if (repositoryRef.Orm == "Dapper")
        kntServiceForMessageBroker = new KntService(new DP.KntRepository(repositoryRef), true);
    else if (repositoryRef.Orm == "EntityFramework")
        kntServiceForMessageBroker = new KntService(new EF.KntRepository(repositoryRef), true);
}
// -------------------------------------------------------------------------------------


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

// Middleware to serve files from ResourcesContainerRootUrl on ResourcesContainerRootPath
// Personalize in appsetting
if (appSettings.MountResourceContainerOnStartup)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            repositoryRef.ResourcesContainerRootPath),
        RequestPath = repositoryRef.ResourcesContainerRootUrl
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("KntPolicy");

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapFallbackToFile("index.html");

app.Run();
