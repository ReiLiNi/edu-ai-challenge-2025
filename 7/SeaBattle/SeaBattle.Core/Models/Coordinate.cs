namespace SeaBattle.Core.Models;

public readonly record struct Coordinate(int Row, int Column)
{
    public override string ToString() => $"{Row}{Column}";
    
    public static Coordinate FromString(string coordinate)
    {
        if (string.IsNullOrEmpty(coordinate) || coordinate.Length != 2)
            throw new ArgumentException("Coordinate must be exactly 2 characters", nameof(coordinate));
        
        if (!int.TryParse(coordinate[0].ToString(), out int row) || 
            !int.TryParse(coordinate[1].ToString(), out int col))
            throw new ArgumentException("Coordinate must contain only digits", nameof(coordinate));
        
        return new Coordinate(row, col);
    }
    
    public bool IsValid(int boardSize) => 
        Row >= 0 && Row < boardSize && Column >= 0 && Column < boardSize;
} 