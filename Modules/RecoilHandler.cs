﻿using System.Diagnostics;
using Helious.Utils;
using HID_API;

namespace Helious.Modules;

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
    public static double Sens = 100;

    private readonly Stopwatch _recoilReset = new();

    private readonly Stopwatch _bulletTiming = new();

    private readonly Stopwatch _benchmarkTiming = new();

    public RecoilHandler(HidHandler hidHandler)
    {
        decimal globalOverflowY = 0;
        var globalTimeOverflow = 0.0;
        var currentBullet = 0;

        var scopeMultiplier = Scope == 1.0 ? 1 : Scope * 1;

        var multiplier = (decimal) (Fov * (12 / 60.0) / (Sens / 100));

        var delay = 60000.0 / Rpm;
        var bestSmoothness = Smoothness != 0 ? Smoothness : (int) Math.Round(Math.Sqrt((VerticalRecoil * scopeMultiplier) * (double) multiplier));
        var smoothedDelay = delay / bestSmoothness;

        if (Smoothness != 0)
        {
            ConsoleUtils.WriteLine($"Using user-defined smoothness value ({Smoothness})");
        }

        ConsoleUtils.WriteLine($"High Resolution Clocking: {Stopwatch.IsHighResolution}");

        var hidStream = hidHandler.CreateHidStream("/dev/hidg1");

        while (true)
        {
            _benchmarkTiming.Restart();
            if (hidHandler.HidMouseHandlers.Count <= 0)
            {
                ConsoleUtils.WriteLine("Failed to find any mouses connected");
                Thread.Sleep(5000);
                continue;
            }

            var mouseState = hidHandler.HidMouseHandlers[0].GetMouseState();
            if (mouseState.LeftButton && mouseState.RightButton)
            {
                _recoilReset.Reset();

                if (currentBullet < TotalBullets && VerticalRecoil != 0)
                {
                    decimal localY = (decimal) (VerticalRecoil * scopeMultiplier);

                    if (currentBullet == 0)
                    {
                        localY = (decimal) Math.Pow((double)localY, InitialRecoil);
                    }

                    localY *= multiplier;

                    decimal overflowY = 0;

                    if (LocalOverflowCorrection && GlobalOverflowCorrection)
                    {
                        if ((int) globalOverflowY != 0)
                        {
                            var truncatedGlobalOverflowY = (int) Math.Ceiling(globalOverflowY);
                            globalOverflowY -= truncatedGlobalOverflowY;

                            localY += truncatedGlobalOverflowY;
                        }
                    }

                    _benchmarkTiming.Stop();
                    ConsoleUtils.WriteLine(
                        $"Bullet {currentBullet} \\ {TotalBullets} | (Y: {localY}, BSM: {bestSmoothness}, RPM: {Rpm}) | (Computation: {_benchmarkTiming.ElapsedTicks / 1000000.0}ms)");

                    var timeOverflow = globalTimeOverflow;
                    globalTimeOverflow = 0;

                    for (int i = 0; i < bestSmoothness; i++)
                    {
                        decimal smoothedY = localY / bestSmoothness;

                        var smoothedIntY = (int) Math.Round(smoothedY, 0, MidpointRounding.ToZero);

                        if (LocalOverflowCorrection)
                        {
                            overflowY += smoothedY - smoothedIntY;

                            if ((int) overflowY != 0)
                            {
                                var truncatedOverflowY = (int) Math.Ceiling(overflowY);
                                overflowY -= truncatedOverflowY;

                                smoothedIntY += truncatedOverflowY;
                            }
                        }

                        hidHandler.WriteMouseReport(mouseState with
                        {
                            X = 0,
                            Y = (short)smoothedIntY,
                            Wheel = 0
                        }, hidStream);

                        _bulletTiming.Restart();

                        while (true)
                        {
                            if (_bulletTiming.ElapsedTicks * 1000000.0 / Stopwatch.Frequency >= (smoothedDelay * 1000) - timeOverflow)
                            {
                                timeOverflow = _bulletTiming.ElapsedTicks * 1000000.0 / Stopwatch.Frequency - smoothedDelay * 1000;
                                break;
                            }
                        }
                    }

                    globalTimeOverflow += timeOverflow;
                    globalOverflowY += overflowY;
                    currentBullet++;
                }

                continue;
            }

            _recoilReset.Start();

            if (_recoilReset.ElapsedMilliseconds >= 250)
            {
                currentBullet = 0;
                globalOverflowY = 0;
                globalTimeOverflow = 0;
            }
        }
    }
}