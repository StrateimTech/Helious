using System.Diagnostics;
using Helious.Utils;
using HID_API;

namespace Helious.Game;

public class RecoilHandler
{
    public static double VerticalRecoil = 0.0;
    public static double InitialRecoil = 1.0;

    public static int Rpm = 0;
    public static int TotalBullets = 0;
    public static int Smoothness = 1;

    public static int Scope = 1; 

    public static bool GlobalOverflowCorrection = true;
    public static bool LocalOverflowCorrection = true;

    public static double Fov = 100;

    public RecoilHandler(HidHandler hidHandler)
    {
        decimal globalOverflowY = 0;

        var currentBullet = 0;
        
        ConsoleUtils.WriteLine($"High Resolution Clocking: {Stopwatch.IsHighResolution}");

        var scopeMultiplier = Scope == 1 ? 1 : Scope * 1.125;
        while (true)
        {
            if (hidHandler.HidMouseHandlers.Count <= 0)
            {
                ConsoleUtils.WriteLine("Failed to find any mouses connected");
                Thread.Sleep(5000);
                continue;
            }

            var delay = 60000.0 / Rpm;

            if (hidHandler.HidMouseHandlers[0].Mouse.LeftButton && hidHandler.HidMouseHandlers[0].Mouse.RightButton)
            {
                if (currentBullet <= TotalBullets && VerticalRecoil != 0)
                {
                    decimal localY = (decimal)(VerticalRecoil * scopeMultiplier);
                    
                    if (currentBullet == 0)
                    {
                        localY *= (decimal)InitialRecoil;
                    }
                    
                    var multiplier = (decimal)(Fov * (12 / 60.0));
                    var bestSmoothness = Smoothness == -1 ? 1 : GetBestSmoothness(localY, multiplier, Smoothness);
                    localY *= multiplier;

                    decimal overflowY = 0;

                    var smoothedDelay = delay / bestSmoothness;

                    if (LocalOverflowCorrection && GlobalOverflowCorrection)
                    {
                        if (globalOverflowY >= 1 || globalOverflowY <= -1)
                        {
                            var truncatedGlobalOverflowY = (int) Math.Truncate(globalOverflowY);
                            globalOverflowY -= truncatedGlobalOverflowY;

                            localY += truncatedGlobalOverflowY;
                        }
                    }

                    ConsoleUtils.WriteLine($"Bullet {currentBullet} \\ {TotalBullets} | (Y: {localY}, BSM: {bestSmoothness}, RPM: {Rpm})");
                    
                    for (int i = 0; i < bestSmoothness; i++)
                    {
                        decimal smoothedY = localY / bestSmoothness;

                        var smoothedIntY = (int) Math.Floor(smoothedY);

                        if (LocalOverflowCorrection)
                        {
                            overflowY += smoothedY - smoothedIntY;

                            if (overflowY >= 1 || overflowY <= -1)
                            {
                                var truncatedOverflowY = (int) Math.Truncate(overflowY);
                                overflowY -= truncatedOverflowY;

                                smoothedIntY += truncatedOverflowY;
                            }
                        }

                        hidHandler.WriteMouseReport(hidHandler.HidMouseHandlers[0].Mouse with
                        {
                            Y = smoothedIntY,
                            Wheel = 0
                        });

                        var stopwatch = Stopwatch.StartNew();
                        while (stopwatch.ElapsedTicks * 1000000.0 / Stopwatch.Frequency <= smoothedDelay * 1000);
                    }

                    globalOverflowY += overflowY;

                    currentBullet++;
                }

                continue;
            }

            currentBullet = 0;
            Thread.Sleep(1);
        }
    }

    private int GetBestSmoothness(decimal yRecoil, decimal multiplier, int minSmoothness = 4)
    {
        var multipliedRecoil = yRecoil * multiplier;
        var leastLossySmoothness = Math.Truncate(multipliedRecoil);

        if (leastLossySmoothness < minSmoothness)
        {
            return minSmoothness;
        }
        
        return (int) leastLossySmoothness;
    }
}