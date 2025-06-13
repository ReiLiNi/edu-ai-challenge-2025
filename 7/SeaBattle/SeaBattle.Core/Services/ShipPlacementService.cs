using SeaBattle.Core.Enums;
using SeaBattle.Core.Models;

namespace SeaBattle.Core;

public class ShipPlacementService : IShipPlacementService
{
    public List<Coordinate> GenerateShipPositions(Coordinate startPosition, ShipOrientation orientation, int shipLength)
    {
        var positions = new List<Coordinate>();
        
        for (int i = 0; i < shipLength; i++)
        {
            var position = orientation switch
            {
                ShipOrientation.Horizontal => new Coordinate(startPosition.Row, startPosition.Column + i),
                ShipOrientation.Vertical => new Coordinate(startPosition.Row + i, startPosition.Column),
                _ => throw new ArgumentException("Invalid orientation", nameof(orientation))
            };
            
            positions.Add(position);
        }
        
        return positions;
    }

    public bool CanPlaceShip(Board board, Coordinate startPosition, ShipOrientation orientation, int shipLength)
    {
        var positions = GenerateShipPositions(startPosition, orientation, shipLength);
        return board.CanPlaceShip(positions);
    }

    public Ship PlaceShip(Board board, Coordinate startPosition, ShipOrientation orientation, int shipLength)
    {
        if (!CanPlaceShip(board, startPosition, orientation, shipLength))
            throw new InvalidOperationException("Cannot place ship at the specified position");

        var positions = GenerateShipPositions(startPosition, orientation, shipLength);
        var ship = new Ship(positions);
        
        board.AddShip(ship);
        
        return ship;
    }

    public void PlaceShipsRandomly(Board board, int numberOfShips, int shipLength, Random random)
    {
        int placedShips = 0;
        int maxAttempts = 1000; // Prevent infinite loops
        int attempts = 0;

        while (placedShips < numberOfShips && attempts < maxAttempts)
        {
            attempts++;
            
            var orientation = random.Next(2) == 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical;
            
            Coordinate startPosition;
            if (orientation == ShipOrientation.Horizontal)
            {
                startPosition = new Coordinate(
                    random.Next(board.Size),
                    random.Next(board.Size - shipLength + 1)
                );
            }
            else
            {
                startPosition = new Coordinate(
                    random.Next(board.Size - shipLength + 1),
                    random.Next(board.Size)
                );
            }

            if (CanPlaceShip(board, startPosition, orientation, shipLength))
            {
                PlaceShip(board, startPosition, orientation, shipLength);
                placedShips++;
            }
        }

        if (placedShips < numberOfShips)
            throw new InvalidOperationException($"Could not place all {numberOfShips} ships after {maxAttempts} attempts");
    }
} 