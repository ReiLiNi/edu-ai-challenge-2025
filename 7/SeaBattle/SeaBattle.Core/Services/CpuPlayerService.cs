using SeaBattle.Core.Models;

namespace SeaBattle.Core;

public class CpuPlayerService : ICpuPlayerService
{
    private readonly Queue<Coordinate> _targetQueue;
    private CpuMode _mode;

    public CpuPlayerService()
    {
        _targetQueue = new Queue<Coordinate>();
        _mode = CpuMode.Hunt;
    }

    public Coordinate GenerateGuess(Board targetBoard, List<Coordinate> previousGuesses, Random random)
    {
        if (_mode == CpuMode.Target && _targetQueue.Count > 0)
        {
            // Target mode: attack adjacent cells
            while (_targetQueue.Count > 0)
            {
                var guess = _targetQueue.Dequeue();
                if (!previousGuesses.Contains(guess))
                {
                    return guess;
                }
            }
            
            // If we get here, queue is empty, switch to hunt mode
            _mode = CpuMode.Hunt;
        }

        // Hunt mode: random guessing
        Coordinate huntGuess;
        do
        {
            huntGuess = new Coordinate(random.Next(targetBoard.Size), random.Next(targetBoard.Size));
        } while (previousGuesses.Contains(huntGuess));

        return huntGuess;
    }

    public void ProcessHitResult(Coordinate lastGuess, bool wasHit, bool wasSunk)
    {
        if (wasHit)
        {
            if (wasSunk)
            {
                // Ship sunk, go back to hunt mode
                _mode = CpuMode.Hunt;
                _targetQueue.Clear();
            }
            else
            {
                // Hit but not sunk, switch to target mode and add adjacent cells
                _mode = CpuMode.Target;
                AddAdjacentTargets(lastGuess);
            }
        }
        else if (_mode == CpuMode.Target && _targetQueue.Count == 0)
        {
            // Miss in target mode with no more targets, switch to hunt mode
            _mode = CpuMode.Hunt;
        }
    }

    private void AddAdjacentTargets(Coordinate hitPosition)
    {
        var adjacentPositions = new[]
        {
            new Coordinate(hitPosition.Row - 1, hitPosition.Column),
            new Coordinate(hitPosition.Row + 1, hitPosition.Column),
            new Coordinate(hitPosition.Row, hitPosition.Column - 1),
            new Coordinate(hitPosition.Row, hitPosition.Column + 1)
        };

        foreach (var position in adjacentPositions)
        {
            if (position.Row >= 0 && position.Row < 10 && 
                position.Column >= 0 && position.Column < 10 &&
                !_targetQueue.Contains(position))
            {
                _targetQueue.Enqueue(position);
            }
        }
    }

    private enum CpuMode
    {
        Hunt,
        Target
    }
} 