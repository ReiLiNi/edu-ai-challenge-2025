using SeaBattle.Core.Enums;

namespace SeaBattle.Core.Models;

public class Board
{
    private readonly CellState[,] _cells;
    private readonly List<Ship> _ships;

    public int Size { get; }

    public IReadOnlyList<Ship> Ships => _ships.AsReadOnly();

    public Board(int size)
    {
        if (size <= 0)
            throw new ArgumentException("Board size must be positive", nameof(size));

        Size = size;
        _cells = new CellState[size, size];
        _ships = new List<Ship>();
        
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                _cells[row, col] = CellState.Water;
            }
        }
    }

    public CellState GetCellState(Coordinate coordinate)
    {
        if (!IsValidCoordinate(coordinate))
            throw new ArgumentException("Invalid coordinate", nameof(coordinate));

        return _cells[coordinate.Row, coordinate.Column];
    }

    public void SetCellState(Coordinate coordinate, CellState state)
    {
        if (!IsValidCoordinate(coordinate))
            throw new ArgumentException("Invalid coordinate", nameof(coordinate));

        _cells[coordinate.Row, coordinate.Column] = state;
    }

    public void AddShip(Ship ship)
    {
        if (ship == null)
            throw new ArgumentNullException(nameof(ship));

        _ships.Add(ship);
        
        // Mark ship positions on the board
        foreach (var position in ship.Positions)
        {
            SetCellState(position, CellState.Ship);
        }
    }

    public bool CanPlaceShip(IList<Coordinate> positions)
    {
        if (positions == null || positions.Count == 0)
            return false;

        // Check if all ship positions are valid and empty
        if (!positions.All(pos => IsValidCoordinate(pos) && GetCellState(pos) == CellState.Water))
            return false;

        // Check adjacency rule - no ships should be adjacent (including diagonally)
        return !HasAdjacentShips(positions);
    }

    private bool HasAdjacentShips(IList<Coordinate> positions)
    {
        foreach (var position in positions)
        {
            // Check all 8 adjacent cells (including diagonals)
            for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (int colOffset = -1; colOffset <= 1; colOffset++)
                {
                    if (rowOffset == 0 && colOffset == 0)
                        continue; // Skip the position itself

                    var adjacentCoordinate = new Coordinate(
                        position.Row + rowOffset, 
                        position.Column + colOffset
                    );

                    // If the adjacent cell is valid and has a ship, and it's not part of the current ship being placed
                    if (IsValidCoordinate(adjacentCoordinate) && 
                        GetCellState(adjacentCoordinate) == CellState.Ship &&
                        !positions.Contains(adjacentCoordinate))
                    {
                        return true; // Found an adjacent ship
                    }
                }
            }
        }
        return false; // No adjacent ships found
    }

    public Ship? GetShipAt(Coordinate coordinate)
    {
        return _ships.FirstOrDefault(ship => ship.ContainsPosition(coordinate));
    }

    public bool IsValidCoordinate(Coordinate coordinate) => coordinate.IsValid(Size);

    public int GetRemainingShipCount() => _ships.Count(ship => !ship.IsSunk);
} 