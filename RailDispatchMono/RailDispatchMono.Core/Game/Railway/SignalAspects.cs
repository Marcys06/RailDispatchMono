// ============================================================
// SIGNALASPECTS.CS - DEFINICJE ASPEKTÓW
// ============================================================

using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Railway
{
    public enum SignalAspect
    {
        Stop,           // S1a - Stój
        StopStation,    // S1b - Stój (stacja)
        Clear,          // S2 - Jazda z Vmax
        Warning,        // S5 - Następny semafor stój
        Speed100,       // S6 - Jazda ≤ 100 km/h
        Speed40,        // S10 - Jazda ≤ 40 km/h
        Reserve1,       // S12 - Rezerwa 1
        Reserve2,       // S13 - Rezerwa 2
        Reserve3,       // S14 - Rezerwa 3
        Reserve4        // S15 - Rezerwa 4
    }

    public static class SignalAspectInfo
    {
        public class AspectInfo
        {
            public string Name { get; }
            public string Description { get; }
            public float SpeedLimitKmh { get; }
            public string Color { get; }

            public AspectInfo(string name, string description, float speedLimitKmh, string color)
            {
                Name = name;
                Description = description;
                SpeedLimitKmh = speedLimitKmh;
                Color = color;
            }
        }

        public static readonly Dictionary<SignalAspect, AspectInfo> Aspects = new()
        {
            { SignalAspect.Stop, new AspectInfo("S1a", "Stój - przejazd zabroniony", 0, "Czerwony") },
            { SignalAspect.StopStation, new AspectInfo("S1b", "Stój (stacja) - przejazd zabroniony", 0, "Czerwony + Biały") },
            { SignalAspect.Clear, new AspectInfo("S2", "Jazda z Vmax - droga wolna", float.MaxValue, "Zielony") },
            { SignalAspect.Warning, new AspectInfo("S5", "Ostrzeżenie - następny semafor stój", 5, "Żółty") },
            { SignalAspect.Speed100, new AspectInfo("S6", "Jazda ≤ 100 km/h", 100, "Zielony + Żółty") },
            { SignalAspect.Speed40, new AspectInfo("S10", "Jazda ≤ 40 km/h", 40, "Żółty Migający") },
            { SignalAspect.Reserve1, new AspectInfo("S12", "Rezerwa 1", 120, "Rezerwowy 1") },
            { SignalAspect.Reserve2, new AspectInfo("S13", "Rezerwa 2", 80, "Rezerwowy 2") },
            { SignalAspect.Reserve3, new AspectInfo("S14", "Rezerwa 3", 60, "Rezerwowy 3") },
            { SignalAspect.Reserve4, new AspectInfo("S15", "Rezerwa 4", 30, "Rezerwowy 4") }
        };
    }

    // ============================================================
    // METODY ROZSZERZAJĄCE DLA SignalAspect
    // ============================================================

    public static class SignalAspectExtensions
    {
        public static string GetName(this SignalAspect aspect)
        {
            return SignalAspectInfo.Aspects.TryGetValue(aspect, out var info) 
                ? info.Name 
                : "Unknown";
        }

        public static string GetDescription(this SignalAspect aspect)
        {
            return SignalAspectInfo.Aspects.TryGetValue(aspect, out var info) 
                ? info.Description 
                : "Nieznany";
        }

        public static float GetSpeedLimit(this SignalAspect aspect)
        {
            return SignalAspectInfo.Aspects.TryGetValue(aspect, out var info) 
                ? info.SpeedLimitKmh 
                : 0f;
        }
    }
}
