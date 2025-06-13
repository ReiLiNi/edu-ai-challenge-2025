using SeaBattle.Core.Models;

namespace SeaBattle.Core;

public interface ICpuPlayerService
{
    Coordinate GenerateGuess(Board targetBoard, List<Coordinate> previousGuesses, Random random);
    void ProcessHitResult(Coordinate lastGuess, bool wasHit, bool wasSunk);
} 