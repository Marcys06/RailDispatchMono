namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Infrastructure class prepared for later speed, maintenance and operating rules.</summary>
public enum LineClass
{
    Local,
    Regional,
    Mainline,
    Magistral
}

public static class LineClassProfile
{
    public static float MaxSpeedKmh(LineClass lineClass) => lineClass switch
    {
        LineClass.Local => 80f,
        LineClass.Regional => 120f,
        LineClass.Mainline => 160f,
        LineClass.Magistral => 200f,
        _ => 80f
    };

    public static float MaxAxleLoadTons(LineClass lineClass) => lineClass switch
    {
        LineClass.Local => 20f,
        LineClass.Regional => 22.5f,
        LineClass.Mainline => 22.5f,
        LineClass.Magistral => 25f,
        _ => 20f
    };
}
