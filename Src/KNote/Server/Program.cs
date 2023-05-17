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
using Microsoft.AspNetCore.Hosting;
using KNote.Model;
using KNote.Server.Helpers;
using KNote.Server.Hubs;
using KNote.MessageBroker.RabbitMQ;


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

builder.Services.KntAddServices(appSettings, repositoryRef);

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
app.KntAddResourcesStaticFiles(appSettings, repositoryRef);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("KntPolicy");

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapFallbackToFile("index.html");

app.KntConfigureMessageBroker(appSettings, repositoryRef);

app.Run();
