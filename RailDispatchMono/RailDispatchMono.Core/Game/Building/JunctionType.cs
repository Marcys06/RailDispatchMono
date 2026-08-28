namespace RailDispatchMono.Core.Game.Building;

public enum JunctionType
{
    South_NorthEast, // Wjazd z Południa -> Prosto na Północ | Skręt na Wschód
    South_NorthWest, // Wjazd z Południa -> Prosto na Północ | Skręt na Zachód
    West_EastSouth,  // Wjazd z Zachodu  -> Prosto na Wschód | Skręt na Południe
    West_EastNorth   // Wjazd z Zachodu  -> Prosto na Wschód | Skręt na Północ
}