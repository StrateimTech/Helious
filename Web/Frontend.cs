﻿using Helious.Utils;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;

namespace Helious.Web;

public static class Frontend
{
    public static void StartFrontend()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production,
            ContentRootPath = Path.GetFullPath($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Web{Path.DirectorySeparatorChar}"),
            WebRootPath = "assets"
        });
        
        builder.Services.AddControllers();
        
        builder.Services.AddRazorPages(options => { options.RootDirectory = "/Web/Pages"; });

        builder.Services.AddLogging(c => c.ClearProviders());
        
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        
        IServer server = app.Services.GetRequiredService<IServer>();
        app.Lifetime.ApplicationStarted.Register(() => PortLogger.logAddresses(server.Features));
        
        app.UseStaticFiles();
        
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "Assets")),
            RequestPath = "/Web/Assets"
        });

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        app.Run();
        
    }
}