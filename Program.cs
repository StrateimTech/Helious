using System.Text.RegularExpressions;
using Helious.Game;
using Helious.Utils;
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
            Console.Write("\x1b[38;2;" + (i == 0 ? 255 : (255 - (5 * i))) + ";" + 64 + ";" + 64 + "m");
            ConsoleUtils.WriteCentered(figgleLines[i]);
        }

        Console.ResetColor();

        var dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local);
        var formattedTime = dateTime.ToString("MM/dd/yy HH:mm:ss");
        ConsoleUtils.WriteCentered($"> {formattedTime} <\n");

        HidHandler? hidHandler = null;
        var hidThread = new Thread(() => hidHandler = new HidHandler(new[]
            {
                "/dev/input/mice"
            },
            null!,
            "/dev/hidg0")
        )
        {
            IsBackground = true
        };
        hidThread.Start();

        ConsoleUtils.WriteLine("Inputs, (Vertical, Initial, Rpm, Magazine Size, Fov)");
        ConsoleUtils.WriteLine("Optional, (Smoothness, Scope (2x, 4x), Global Overflow, Local Overflow)");
        ConsoleUtils.WriteOnLine();
        var readLine = Console.ReadLine();
        if (readLine == null)
        {
            ConsoleUtils.WriteLine("No inputs found.");
            return;
        }

        var values = readLine.Split(", ");
        if (values.Length < 5)
        {
            ConsoleUtils.WriteLine($"Not enough inputs ({values.Length}, Minimum 6)");
            return;
        }
        
        RecoilHandler.VerticalRecoil = double.Parse(values[0]);
        RecoilHandler.InitialRecoil = double.Parse(values[1]);
        RecoilHandler.Rpm = int.Parse(values[2]);
        RecoilHandler.TotalBullets = int.Parse(values[3]);
        RecoilHandler.Fov = int.Parse(values[4]);

        switch (values.Length)
        {
            case 6:
                RecoilHandler.Smoothness = int.Parse(values[5]);
                break;
            case 7:
                RecoilHandler.Smoothness = int.Parse(values[5]);
                RecoilHandler.Scope = double.Parse(values[6].Replace("x", ""));
                break;
            case 8:
                RecoilHandler.Smoothness = int.Parse(values[5]);
                RecoilHandler.Scope = double.Parse(values[6].Replace("x", ""));
                RecoilHandler.GlobalOverflowCorrection = bool.Parse(values[7]);
                break;
            case 9:
                RecoilHandler.Smoothness = int.Parse(values[5]);
                RecoilHandler.Scope = double.Parse(values[6].Replace("x", ""));
                RecoilHandler.GlobalOverflowCorrection = bool.Parse(values[7]);
                RecoilHandler.LocalOverflowCorrection = bool.Parse(values[8]);
                break;
        }

        var recoilThread = new Thread(() => new RecoilHandler(hidHandler!))
        {
            IsBackground = true
        };
        recoilThread.Start();

        Console.CancelKeyPress += (_, _) =>
        {
            ConsoleUtils.WriteLine("Shutting down...");
            if (hidHandler != null)
            {
                hidHandler.Stop();
            }

            Environment.Exit(0);
        };

        ConsoleUtils.WriteLine("Press any key to shutdown!");
        Console.ReadKey();
    }
}