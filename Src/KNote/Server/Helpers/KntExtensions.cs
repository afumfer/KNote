using KNote.Model;
using KNote.Repository;
using KNote.Service.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace KNote.Server.Helpers;

public static class KntExtensions
{
    private static KntService _kntServiceForMessageBroker;

    public static IServiceCollection KntAddServices(this IServiceCollection services, AppSettings appSettings, RepositoryRef repositoryRef)
    {        
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddScoped<IKntRepository>(provider => KntRepositoryFactory.Create(repositoryRef));

        services.AddScoped<IKntService, KntService>();

        return services;
    }

    public static IApplicationBuilder KntAddResourcesStaticFiles(this IApplicationBuilder app, AppSettings appSettings, RepositoryRef repositoryRef)
    {        
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }
        if (appSettings == null)
        {
            throw new ArgumentNullException(nameof(appSettings));
        }
        if (repositoryRef == null)
        {
            throw new ArgumentNullException(nameof(repositoryRef));
        }

        StaticFileOptions options = null!;

        if (appSettings.MountResourceContainerOnStartup)
        {
            options = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    repositoryRef.ResourcesContainerRootPath),
                RequestPath = repositoryRef.ResourcesContainerRootUrl
            };
            return app.UseMiddleware<StaticFileMiddleware>(Options.Create(options));
        }
        else
        {
            return app;
        }
    }
    
    public static void KntConfigureMessageBroker(this IApplicationBuilder app, AppSettings appSettings, RepositoryRef repositoryRef)
    {
        if (appSettings == null)
        {
            throw new ArgumentNullException(nameof(appSettings));
        }
        if (repositoryRef == null)
        {
            throw new ArgumentNullException(nameof(repositoryRef));
        }

        // Experimental ------------------------------------------------------------------------
        //
        // To use the message broker force an instance of the service layer at the application
        // level with the use of the enable activeMessageBroker parameter in its constructor.
        // (No use: "var kntServiceForMessageBroker = app.Services.GetRequiredService<KntService>();")
        //
        
        if (appSettings.ActivateMessageBroker)
        {
            _kntServiceForMessageBroker = new KntService(KntRepositoryFactory.Create(repositoryRef), true);
        }
        // -------------------------------------------------------------------------------------
    }

    public static IKntService GetMessageBroker(this IApplicationBuilder app)
    {
        return _kntServiceForMessageBroker;
    }
}
