using System.Text.RegularExpressions;
using Helious.Game;
using Helious.Utils;
using Helious.Web;
using HID_API;

namespace Helious;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine();

        var alligator = Figgle.FiggleFonts.Alligator3.Render("H E L I O U S");
        var figgleLines = Regex.Split(alligator, "\r\n|\r|\n");
        for (int i = 0; i < figgleLines.Length - 1; i++)
        {
            ConsoleUtils.WriteCentered(figgleLines[i]);
        }

        var dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local);
        var formattedTime = dateTime.ToString("MM/dd/yy HH:mm:ss");
        ConsoleUtils.WriteCentered($"> {formattedTime} <\n");

        // TODO: Temporary configuration
        ConsoleUtils.WriteLine("Inputs, (Horizontal Recoil, Vertical Recoil, Initial Recoil, Rpm, Bullet Count, Smoothness)");
        ConsoleUtils.WriteOnLine();
        var readLine = Console.ReadLine();
        if (readLine == null)
        {
            ConsoleUtils.WriteLine("No inputs found.");
            return;
        }

        var values = readLine.Split(", ");
        if (values.Length < 6)
        {
            ConsoleUtils.WriteLine($"Not enough inputs ({values.Length})");
            return;
        }

        var horizontalRecoil = double.Parse(values[0]);
        var verticalRecoil = 20 * double.Parse(values[1]);
        var initialRecoil = double.Parse(values[2]);

        var rpm = int.Parse(values[3]);
        var bullets = int.Parse(values[4]);
        var smoothness = int.Parse(values[5]);
        //

        var hidHandler = new HidHandler(new[]
            {
                "/dev/input/mice"
            },
            new[]
            {
                "/dev/input/by-id/usb-Keychron_K4_Keychron_K4-event-kbd",
                "/dev/input/by-id/usb-Logitech_G502_HERO_Gaming_Mouse_0E6D395D3333-if01-event-kbd"
            },
            "/dev/hidg0");

        var recoilThread = new Thread(() =>
        {
            new RecoilHandler(hidHandler)
            {
                HorizontalRecoil = horizontalRecoil,
                VerticalRecoil = verticalRecoil,
                InitialRecoil = initialRecoil,
                Rpm = rpm,
                TotalBullets = bullets,
                Smoothness = smoothness
            };
        })
        {
            IsBackground = true
        };
        recoilThread.Start();

        var frontendThread = new Thread(Frontend.StartFrontend)
        {
            IsBackground = true
        };
        frontendThread.Start();

        Console.CancelKeyPress += (_, _) =>
        {
            ConsoleUtils.WriteLine("Shutting down...");
            hidHandler.Stop();
            Environment.Exit(0);
        };

        ConsoleUtils.WriteLine("Press any key to shutdown!");
        Console.ReadKey();
    }
}