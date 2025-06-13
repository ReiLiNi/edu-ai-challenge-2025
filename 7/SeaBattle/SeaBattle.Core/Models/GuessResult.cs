namespace SeaBattle.Core.Models;

public record GuessResult(bool IsValid, bool IsHit, bool IsSunk, string Message)
{
    public static GuessResult Success(bool isHit, bool isSunk) =>
        new(true, isHit, isSunk, isHit ? (isSunk ? "Hit and sunk!" : "Hit!") : "Miss");
    
    public static GuessResult Failure(string message) =>
        new(false, false, false, message);
} 