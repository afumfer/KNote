using KNote.Model;
using KNote.Repository;
using KNote.Server.Ai;
using KNote.Service.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace KNote.Server.Helpers;

public static class KntExtensions
{
    private static KntService _kntServiceForMessageBroker;

    // The service of each request is the one of the session's repository (the knt_repo claim of its token), or of the
    // default repository without a user. The controllers set its UserIdentityName.
    public static IServiceCollection KntAddServices(this IServiceCollection services, KntRepositories repositories)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton(repositories);
        services.AddScoped<IKntService>(provider =>
        {
            var httpContext = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            return repositories.CreateService(repositories.ForUser(httpContext?.User), null, httpContext?.Request);
        });

        return services;
    }

    // The Web AI assistant (AiAssistantController): the providers of the "ai" configuration section, bound when
    // first used (so the in-process tests can still override them), and how their chat clients are built.
    public static IServiceCollection KntAddAiAssistant(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Configure<AiConfig>(configuration.GetSection("ai"));
        services.AddSingleton<AiProvidersCatalog>();
        services.AddSingleton<IAiChatClientProvider, AiChatClientProvider>();

        return services;
    }

    // "Forgot your password?": the links sent by email (the "PasswordReset" section) through the Server's SMTP account
    // (the "Email" section), bound when first used like the "ai" section.
    public static IServiceCollection KntAddPasswordReset(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Configure<PasswordResetConfig>(configuration.GetSection("PasswordReset"));
        services.Configure<EmailConfig>(configuration.GetSection("Email"));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<KntPasswordResetMail>();

        return services;
    }

    // KNote Local (AppSettings:ServeRepositoryResources): the resources of each repository from its folder at
    // /KNote/Resources/{RepositoryId}/... (its ResourcesContainerRootUrl, see KntRepositories). The repository is looked up
    // in each request, so one linked or unlinked since the server started is served (or not) at once. Anonymous, as the
    // resources served by IIS or by MountResourceContainerOnStartup; nothing outside the repository's folder.
    public static WebApplication MapKntRepositoryResources(this WebApplication app, AppSettings appSettings)
    {
        if (!appSettings.ServeRepositoryResources)
            return app;

        var contentTypes = new FileExtensionContentTypeProvider();
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        app.MapGet(KntRepositories.ResourcesPath + "/{repositoryId:guid}/{**path}", (Guid repositoryId, string path, KntRepositories repositories) =>
        {
            var root = repositories.Find(repositoryId)?.RepositoryRef.ResourcesContainerRootPath;
            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(path))
                return Results.NotFound();

            var rootFolder = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var file = Path.GetFullPath(Path.Combine(rootFolder, path));
            if (!file.StartsWith(rootFolder, comparison) || !File.Exists(file))
                return Results.NotFound();

            if (!contentTypes.TryGetContentType(file, out var contentType))
                contentType = "application/octet-stream";
            return Results.File(file, contentType, enableRangeProcessing: true);
        });
        return app;
    }

    // With MountResourceContainerOnStartup, the resources folder of each repository is served from its url (a path of
    // this host, e.g. /KNoteResources). Without it, they are served by the web server (IIS virtual directory) or, when
    // they are in this host's web root, by UseStaticFiles.
    public static IApplicationBuilder KntAddResourcesStaticFiles(this IApplicationBuilder app, AppSettings appSettings, KntRepositories repositories)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }
        if (appSettings == null)
        {
            throw new ArgumentNullException(nameof(appSettings));
        }
        if (repositories == null)
        {
            throw new ArgumentNullException(nameof(repositories));
        }

        if (!appSettings.MountResourceContainerOnStartup)
            return app;

        foreach (var repositoryRef in repositories.All.Select(r => r.RepositoryRef))
        {
            if (string.IsNullOrEmpty(repositoryRef.ResourcesContainerRootPath) || repositoryRef.ResourcesContainerRootUrl?.StartsWith('/') != true)
                continue;

            var options = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(repositoryRef.ResourcesContainerRootPath),
                RequestPath = repositoryRef.ResourcesContainerRootUrl.TrimEnd('/')
            };
            app.UseMiddleware<StaticFileMiddleware>(Options.Create(options));
        }

        return app;
    }
    
    public static async Task KntConfigureMessageBroker(this IApplicationBuilder app, AppSettings appSettings, RepositoryRef repositoryRef)
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
            await _kntServiceForMessageBroker.InitMessageBrokerAsync();
        }
        // -------------------------------------------------------------------------------------
    }

    public static IKntService GetMessageBroker(this IApplicationBuilder app)
    {
        return _kntServiceForMessageBroker;
    }
}
