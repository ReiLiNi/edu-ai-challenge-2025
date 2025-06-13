namespace SeaBattle.Core.Models;

public record GameConfiguration(int BoardSize, int NumberOfShips, int ShipLength)
{
    public static GameConfiguration Default => new(10, 3, 3);
} 