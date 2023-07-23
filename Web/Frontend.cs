using Helious.Utils;
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

        builder.WebHost.UseKestrel(options =>
        {
            options.ListenAnyIP(9834);
            options.ListenAnyIP(8734, configure => configure.UseHttps());
        });

        builder.Services.AddRazorPages(options => { options.RootDirectory = "/Web/Pages"; });

        // builder.Services.AddLogging(c => c.ClearProviders());

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        IServer server = app.Services.GetRequiredService<IServer>();
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            if (addressFeature != null)
            {
                ConsoleUtils.WriteLine($"Listening on Ports ({String.Join(", ", addressFeature.Addresses)})");
            }
        });


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