using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Helious.Utils;

public class PortLogger
{
    public static void logAddresses(IFeatureCollection features)
    {
        var addressFeature = features.Get<IServerAddressesFeature>();
        if (addressFeature != null)
        {
            ConsoleUtils.WriteLine($"Listening on Ports ({String.Join(", ", addressFeature.Addresses)})");
        }
    }
}