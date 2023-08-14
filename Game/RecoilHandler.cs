using System.Diagnostics;
using Helious.Utils;
using HID_API;

namespace Helious.Game;

public class RecoilHandler
{
    public static double VerticalRecoil = 0.0;
    public static double HorizontalRecoil = 0.0;
    public static double InitialRecoil = 1.0;

    public static int Rpm = 0;
    public static int TotalBullets = 0;
    public static int Smoothness = 1;

    public static bool GlobalOverflowCorrection = true;
    public static bool LocalOverflowCorrection = true;

    public static double Fov = 100;

    public RecoilHandler(HidHandler hidHandler)
    {
        decimal globalOverflowX = 0;
        decimal globalOverflowY = 0;

        var currentBullet = 0;
        
        ConsoleUtils.WriteLine($"High Resolution Clocking: {Stopwatch.IsHighResolution}");

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
                if (currentBullet <= TotalBullets && (VerticalRecoil != 0 || HorizontalRecoil != 0))
                {
                    decimal localX = (decimal)HorizontalRecoil;
                    decimal localY = (decimal)VerticalRecoil;
                    
                    if (currentBullet == 0)
                    {
                        localX *= (decimal)InitialRecoil;
                        localY *= (decimal)InitialRecoil;
                    }
                    
                    var multiplier = (decimal)(Fov * (12 / 60.0));
                    var bestSmoothness = Smoothness == -1 ? 1 : SimulateBestSmoothness(localY, multiplier, TotalBullets, Smoothness);
                    localY *= multiplier;

                    decimal overflowX = 0;
                    decimal overflowY = 0;

                    var smoothedDelay = delay / bestSmoothness;

                    if (LocalOverflowCorrection && GlobalOverflowCorrection)
                    {
                        if (globalOverflowX >= 1 || globalOverflowX <= -1)
                        {
                            var truncatedGlobalOverflowX = (int) Math.Truncate(globalOverflowX);
                            globalOverflowX -= truncatedGlobalOverflowX;
                            
                            localX += truncatedGlobalOverflowX;
                        }

                        if (globalOverflowY >= 1 || globalOverflowY <= -1)
                        {
                            var truncatedGlobalOverflowY = (int) Math.Truncate(globalOverflowY);
                            globalOverflowY -= truncatedGlobalOverflowY;

                            localY += truncatedGlobalOverflowY;
                        }
                    }

                    ConsoleUtils.WriteLine($"Bullet {currentBullet} \\ {TotalBullets} | (X: {localX}, Y: {localY}, BSM: {bestSmoothness}, RPM: {Rpm})");
                    // Console.WriteLine($"Smoothing Global (X: {globalOverflowX}, Y: {globalOverflowY})");
                    
                    for (int i = 0; i < bestSmoothness; i++)
                    {
                        decimal smoothedX = localX / bestSmoothness;
                        decimal smoothedY = localY / bestSmoothness;

                        var smoothedIntX = (int) Math.Floor(smoothedX);
                        var smoothedIntY = (int) Math.Floor(smoothedY);

                        if (LocalOverflowCorrection)
                        {
                            overflowX += smoothedX - smoothedIntX;
                            overflowY += smoothedY - smoothedIntY;

                            if (overflowX >= 1 || overflowX <= -1)
                            {
                                var truncatedOverflowX = (int) Math.Truncate(overflowX);
                                overflowX -= truncatedOverflowX;

                                smoothedIntX += truncatedOverflowX;
                            }

                            if (overflowY >= 1 || overflowY <= -1)
                            {
                                var truncatedOverflowY = (int) Math.Truncate(overflowY);
                                overflowY -= truncatedOverflowY;

                                smoothedIntY += truncatedOverflowY;
                            }
                        }

                        hidHandler.WriteMouseReport(hidHandler.HidMouseHandlers[0].Mouse with
                        {
                            X = smoothedIntX,
                            Y = smoothedIntY,
                            Wheel = 0
                        });

                        var stopwatch = Stopwatch.StartNew();
                        while (stopwatch.ElapsedTicks * 1000000.0 / Stopwatch.Frequency <= smoothedDelay * 1000);
                    }

                    globalOverflowX += overflowX;
                    globalOverflowY += overflowY;

                    currentBullet++;
                }

                continue;
            }

            currentBullet = 0;
            Thread.Sleep(1);
        }
    }

    private int SimulateBestSmoothness(decimal yRecoil, decimal multiplier, int bulletCount, int minSmoothness = 4)
    {
        decimal? best = null;
        int bestIndex = -1;
        for (int i = minSmoothness; i < 128; i++)
        {
            var yAdjusted = yRecoil * multiplier;
            var divider = yAdjusted / i;
            
            var onlyDecimals = Math.Floor(divider) - divider;
        
            var simBullets = Math.Abs(onlyDecimals * bulletCount);
            best ??= simBullets;
            
            if (simBullets < best)
            {
                best = simBullets;
                bestIndex = i;
            }
        }

        if (bestIndex == -1)
        {
            return minSmoothness;
        }
        return bestIndex;
    }
}