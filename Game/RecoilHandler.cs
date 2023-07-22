using System.Diagnostics;
using Helious.Utils;
using HID_API;

namespace Helious.Game;

public class RecoilHandler
{
    public double VerticalRecoil = 0;
    public double HorizontalRecoil = 0;
    public double InitialRecoil = 1.0;

    public int Rpm = 0;
    public int TotalBullets = 0;
    public int Smoothness = 1;

    public bool GlobalOverflowCorrection = true;
    public bool LocalOverflowCorrection = true;

    public RecoilHandler(HidHandler hidHandler)
    {
        var globalOverflowX = 0.0;
        var globalOverflowY = 0.0;

        var currentBullet = 0;

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
                if (currentBullet <= TotalBullets)
                {
                    var localX = HorizontalRecoil;
                    var localY = VerticalRecoil;

                    if (currentBullet == 0)
                    {
                        localX *= InitialRecoil;
                        localY *= InitialRecoil;
                    }

                    // // false == odd
                    // // true == even
                    // if (currentBullet % 2 == 0 && localX != 0)
                    // {
                    //     localX *= -1;
                    // }

                    var overflowX = 0.0;
                    var overflowY = 0.0;

                    var smoothedDelay = delay / Smoothness;

                    if (LocalOverflowCorrection && GlobalOverflowCorrection)
                    {
                        if (globalOverflowX >= 1 || globalOverflowX <= -1)
                        {
                            var flooredGlobalOverflowX = (int) Math.Truncate(globalOverflowX);
                            globalOverflowX -= flooredGlobalOverflowX;

                            localX += flooredGlobalOverflowX;
                        }

                        if (globalOverflowY >= 1 || globalOverflowY <= -1)
                        {
                            var flooredGlobalOverflowY = (int) Math.Truncate(globalOverflowY);
                            globalOverflowY -= flooredGlobalOverflowY;

                            localY += flooredGlobalOverflowY;
                        }
                    }

                    Console.WriteLine($"Bullet {currentBullet} \\ {TotalBullets} | (X: {localX}, Y: {localY}, SM: {Smoothness}, RPM: {Rpm})");

                    for (int i = 0; i < Smoothness; i++)
                    {
                        var smoothedX = localX / Smoothness;
                        var smoothedY = localY / Smoothness;

                        var smoothedIntX = (int) Math.Floor(smoothedX);
                        var smoothedIntY = (int) Math.Floor(smoothedY);

                        if (LocalOverflowCorrection)
                        {
                            overflowX += smoothedX - smoothedIntX;
                            overflowY += smoothedY - smoothedIntY;

                            if (overflowX >= 1 || overflowX <= -1)
                            {
                                var flooredOverflowX = (int) Math.Truncate(overflowX);
                                overflowX -= flooredOverflowX;

                                smoothedIntX += flooredOverflowX;
                            }

                            if (overflowY >= 1 || overflowY <= -1)
                            {
                                var flooredOverflowY = (int) Math.Truncate(overflowY);
                                overflowY -= flooredOverflowY;

                                smoothedIntY += flooredOverflowY;
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
}