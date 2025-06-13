using SeaBattle.Core.Enums;
using SeaBattle.Core.Models;

namespace SeaBattle.Core;

public interface IShipPlacementService
{
    List<Coordinate> GenerateShipPositions(Coordinate startPosition, ShipOrientation orientation, int shipLength);
    bool CanPlaceShip(Board board, Coordinate startPosition, ShipOrientation orientation, int shipLength);
    Ship PlaceShip(Board board, Coordinate startPosition, ShipOrientation orientation, int shipLength);
    void PlaceShipsRandomly(Board board, int numberOfShips, int shipLength, Random random);
} 