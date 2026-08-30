namespace RailDispatchMono.Core.Game.Building;

public enum JunctionType
{
    // --- WJAZD Z PO£UDNIA ---
    South_NorthEast, // Wjazd z Po³udnia -> Prosto na Pó³noc | Skrêt na Wschód
    South_NorthWest, // Wjazd z Po³udnia -> Prosto na Pó³noc | Skrêt na Zachód

    // --- WJAZD Z ZACHODU ---
    West_EastSouth,  // Wjazd z Zachodu  -> Prosto na Wschód  | Skrêt na Po³udnie
    West_EastNorth,   // Wjazd z Zachodu  -> Prosto na Wschód  | Skrêt na Pó³noc

    // --- WJAZD Z PÓ£NOCY ---
    North_SouthEast, // Wjazd z Pó³nocy  -> Prosto na Po³udnie | Skrêt na Wschód
    North_SouthWest, // Wjazd z Pó³nocy  -> Prosto na Po³udnie | Skrêt na Zachód

    // --- WJAZD ZE WSCHODU ---
    East_WestSouth,  // Wjazd ze Wschodu -> Prosto na Zachód  | Skrêt na Po³udnie
    East_WestNorth   // Wjazd ze Wschodu -> Prosto na Zachód  | Skrêt na Pó³noc
}