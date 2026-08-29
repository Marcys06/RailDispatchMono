namespace RailDispatchMono.Core.Game.Building;

public enum JunctionType
{
    // --- WJAZD Z POŁUDNIA ---
    South_NorthEast, // Wjazd z Południa -> Prosto na Północ | Skręt na Wschód
    South_NorthWest, // Wjazd z Południa -> Prosto na Północ | Skręt na Zachód

    // --- WJAZD Z ZACHODU ---
    West_EastSouth,  // Wjazd z Zachodu  -> Prosto na Wschód  | Skręt na Południe
    West_EastNorth,   // Wjazd z Zachodu  -> Prosto na Wschód  | Skręt na Północ

    // --- WJAZD Z PÓŁNOCY ---
    North_SouthEast, // Wjazd z Północy  -> Prosto na Południe | Skręt na Wschód
    North_SouthWest, // Wjazd z Północy  -> Prosto na Południe | Skręt na Zachód

    // --- WJAZD ZE WSCHODU ---
    East_WestSouth,  // Wjazd ze Wschodu -> Prosto na Zachód  | Skręt na Południe
    East_WestNorth   // Wjazd ze Wschodu -> Prosto na Zachód  | Skręt na Północ
}