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
    public static int Smoothness = 0;

    public static double Scope = 1.0;

    public static bool GlobalOverflowCorrection = true;
    public static bool LocalOverflowCorrection = true;

    public static double Fov = 100;

    private readonly Stopwatch _recoilReset = new();

    public RecoilHandler(HidHandler hidHandler)
    {
        decimal globalOverflowY = 0;
        var currentBullet = 0;

        var scopeMultiplier = Scope == 1.0 ? 1 : Scope * 1;

        if (Smoothness != 0)
        {
            ConsoleUtils.WriteLine($"Using user-defined smoothness value ({Smoothness})");
        }

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

            bool left;
            bool right;
            hidHandler.HidMouseHandlers[0].mouseLock.EnterReadLock();
            try
            {
                left = hidHandler.HidMouseHandlers[0].Mouse.LeftButton;
                right = hidHandler.HidMouseHandlers[0].Mouse.RightButton;
            }
            finally
            {
                hidHandler.HidMouseHandlers[0].mouseLock.ExitReadLock();
            }

            if (left && right)
            {
                _recoilReset.Reset();

                if (currentBullet < TotalBullets && VerticalRecoil != 0)
                {
                    decimal localY = (decimal) (VerticalRecoil * scopeMultiplier);

                    if (currentBullet == 0)
                    {
                        localY += (decimal) InitialRecoil;
                    }

                    var multiplier = (decimal) (Fov * (12 / 60.0));
                    localY *= multiplier;
                    var bestSmoothness = Smoothness != 0 ? Smoothness : (int) Math.Round(Math.Sqrt((double) localY));

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

                        hidHandler.HidMouseHandlers[0].mouseLock.EnterReadLock();
                        try
                        {
                            hidHandler.WriteMouseReport(hidHandler.HidMouseHandlers[0].Mouse with
                            {
                                X = 0,
                                Y = smoothedIntY,
                                Wheel = 0
                            });
                        }
                        finally
                        {
                            hidHandler.HidMouseHandlers[0].mouseLock.ExitReadLock();
                        }

                        var stopwatch = Stopwatch.StartNew();
                        while (stopwatch.ElapsedTicks * 1000000.0 / Stopwatch.Frequency <= smoothedDelay * 1000) ;
                    }

                    globalOverflowY += overflowY;

                    currentBullet++;
                }

                continue;
            }

            _recoilReset.Start();

            if (_recoilReset.ElapsedMilliseconds > 500)
            {
                currentBullet = 0;
                globalOverflowY = 0;
            }
        }
    }
}