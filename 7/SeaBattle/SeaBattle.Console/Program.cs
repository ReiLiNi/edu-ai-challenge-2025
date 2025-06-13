using SeaBattle.Core;
using SeaBattle.Core.Enums;
using SeaBattle.Core.Models;

namespace SeaBattle.Console;

class Program
{
    private static readonly ISeaBattleGame _game;
    
    static Program()
    {
        var shipPlacementService = new ShipPlacementService();
        var cpuPlayerService = new CpuPlayerService();
        _game = new SeaBattleGame(shipPlacementService, cpuPlayerService);
    }

    static void Main(string[] args)
    {
        System.Console.WriteLine("=== Modern C# Sea Battle Game ===");
        System.Console.WriteLine();

        _game.InitializeGame();
        _game.AutoPlacePlayerShips();
        _game.AutoPlaceOpponentShips();

        var config = _game.GetGameConfiguration();
        System.Console.WriteLine($"Let's play Sea Battle!");
        System.Console.WriteLine($"Try to sink the {config.NumberOfShips} enemy ships.");
        System.Console.WriteLine();

        GameLoop();
    }

    private static void GameLoop()
    {
        while (true)
        {
            var gameState = _game.GetGameState();
            
            if (gameState == GameState.PlayerWon)
            {
                System.Console.WriteLine("\n*** CONGRATULATIONS! You sunk all enemy battleships! ***");
                PrintBoards(revealOpponentShips: true);
                break;
            }
            
            if (gameState == GameState.CpuWon)
            {
                System.Console.WriteLine("\n*** GAME OVER! The CPU sunk all your battleships! ***");
                PrintBoards(revealOpponentShips: true);
                break;
            }

            PrintBoards();
            
            // Player turn
            var playerResult = ProcessPlayerTurn();
            if (!playerResult.IsValid)
                continue;

            // Check for player victory after player's turn
            if (_game.GetGameState() == GameState.PlayerWon)
                continue;

            // CPU turn
            ProcessCpuTurn();

            // Check for CPU victory after CPU's turn
            if (_game.GetGameState() == GameState.CpuWon)
                continue;
        }
    }

    private static GuessResult ProcessPlayerTurn()
    {
        System.Console.Write("Enter your guess (e.g., 00): ");
        var input = System.Console.ReadLine() ?? string.Empty;
        
        var result = _game.ProcessPlayerGuess(input);
        
        if (!result.IsValid)
        {
            System.Console.WriteLine($"Oops, {result.Message.ToLower()}");
            return result;
        }

        if (result.IsHit)
        {
            System.Console.WriteLine("PLAYER HIT!");
            if (result.IsSunk)
            {
                System.Console.WriteLine("You sunk an enemy battleship!");
            }
        }
        else
        {
            System.Console.WriteLine("PLAYER MISS.");
        }

        return result;
    }

    private static void ProcessCpuTurn()
    {
        System.Console.WriteLine("\n--- CPU's Turn ---");
        
        var cpuGuess = _game.GenerateCpuGuess();
        var result = _game.ProcessCpuGuess(cpuGuess);
        
        if (result.IsHit)
        {
            System.Console.WriteLine($"CPU HIT at {cpuGuess}!");
            if (result.IsSunk)
            {
                System.Console.WriteLine("CPU sunk your battleship!");
            }
        }
        else
        {
            System.Console.WriteLine($"CPU MISS at {cpuGuess}.");
        }
    }

    private static void PrintBoards(bool revealOpponentShips = false)
    {
        var playerBoard = _game.GetPlayerBoard();
        var opponentBoard = _game.GetOpponentBoard();
        
        System.Console.WriteLine("\n   --- OPPONENT BOARD ---          --- YOUR BOARD ---");
        
        var header = "  ";
        for (var h = 0; h < playerBoard.Size; h++) 
            header += h + " ";
        System.Console.WriteLine(header + "     " + header);

        for (var i = 0; i < playerBoard.Size; i++)
        {
            var rowStr = i + " ";

            // Opponent board
            for (var j = 0; j < opponentBoard.Size; j++)
            {
                var coordinate = new Coordinate(i, j);
                var cellState = opponentBoard.GetCellState(coordinate);
                var symbol = GetCellSymbol(cellState, false, revealOpponentShips);
                rowStr += symbol + " ";
            }
            
            rowStr += "    " + i + " ";

            // Player board
            for (var j = 0; j < playerBoard.Size; j++)
            {
                var coordinate = new Coordinate(i, j);
                var cellState = playerBoard.GetCellState(coordinate);
                var symbol = GetCellSymbol(cellState, true, revealOpponentShips);
                rowStr += symbol + " ";
            }
            
            System.Console.WriteLine(rowStr);
        }
        
        System.Console.WriteLine();
    }

    private static char GetCellSymbol(CellState cellState, bool isPlayerBoard, bool revealOpponentShips = false)
    {
        return cellState switch
        {
            CellState.Water => '~',
            CellState.Ship => (isPlayerBoard || revealOpponentShips) ? 'S' : '~', // Show opponent ships when game is over
            CellState.Hit => 'X',
            CellState.Miss => 'O',
            _ => '?'
        };
    }
} 