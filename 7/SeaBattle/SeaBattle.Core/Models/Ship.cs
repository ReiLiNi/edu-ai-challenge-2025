using SeaBattle.Core.Models;

namespace SeaBattle.Core.Models;

public class Ship
{
    private readonly HashSet<Coordinate> _hitPositions;
    
    public IReadOnlyList<Coordinate> Positions { get; }
    
    public bool IsSunk => _hitPositions.Count == Positions.Count;

    public Ship(IList<Coordinate> positions)
    {
        if (positions == null || positions.Count == 0)
            throw new ArgumentException("Ship must have at least one position", nameof(positions));
        
        Positions = positions.ToArray();
        _hitPositions = new HashSet<Coordinate>();
    }

    public bool Hit(Coordinate coordinate)
    {
        if (!Positions.Contains(coordinate))
            return false;
        
        _hitPositions.Add(coordinate);
        return true;
    }

    public bool IsHitAt(Coordinate coordinate) => _hitPositions.Contains(coordinate);

    public bool ContainsPosition(Coordinate coordinate) => Positions.Contains(coordinate);
} 