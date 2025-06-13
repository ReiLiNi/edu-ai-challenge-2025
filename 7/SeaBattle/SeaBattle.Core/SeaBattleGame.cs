using SeaBattle.Core.Enums;
using SeaBattle.Core.Models;

namespace SeaBattle.Core;

public class SeaBattleGame : ISeaBattleGame
{
    private readonly IShipPlacementService _shipPlacementService;
    private readonly ICpuPlayerService _cpuPlayerService;
    private readonly GameConfiguration _configuration;
    private readonly Random _random;
    private readonly List<Coordinate> _playerGuesses;
    private readonly List<Coordinate> _cpuGuesses;

    private Board? _playerBoard;
    private Board? _opponentBoard;

    public SeaBattleGame(IShipPlacementService shipPlacementService, ICpuPlayerService cpuPlayerService)
    {
        _shipPlacementService = shipPlacementService ?? throw new ArgumentNullException(nameof(shipPlacementService));
        _cpuPlayerService = cpuPlayerService ?? throw new ArgumentNullException(nameof(cpuPlayerService));
        _configuration = GameConfiguration.Default;
        _random = new Random();
        _playerGuesses = new List<Coordinate>();
        _cpuGuesses = new List<Coordinate>();
    }

    public GameConfiguration GetGameConfiguration() => _configuration;

    public void InitializeGame()
    {
        _playerBoard = new Board(_configuration.BoardSize);
        _opponentBoard = new Board(_configuration.BoardSize);
        _playerGuesses.Clear();
        _cpuGuesses.Clear();
    }

    public Board GetPlayerBoard()
    {
        EnsureGameInitialized();
        return _playerBoard!;
    }

    public Board GetOpponentBoard()
    {
        EnsureGameInitialized();
        return _opponentBoard!;
    }

    public bool IsValidCoordinate(Coordinate coordinate)
    {
        return coordinate.IsValid(_configuration.BoardSize);
    }

    public bool IsValidAndNewGuess(Coordinate coordinate, List<Coordinate> previousGuesses)
    {
        return IsValidCoordinate(coordinate) && !previousGuesses.Contains(coordinate);
    }

    public bool CanPlaceShip(Coordinate startPosition, ShipOrientation orientation, int shipLength)
    {
        EnsureGameInitialized();
        return _shipPlacementService.CanPlaceShip(_playerBoard!, startPosition, orientation, shipLength);
    }

    public Ship PlacePlayerShip(Coordinate startPosition, ShipOrientation orientation)
    {
        EnsureGameInitialized();
        return _shipPlacementService.PlaceShip(_playerBoard!, startPosition, orientation, _configuration.ShipLength);
    }

    public Ship PlaceOpponentShip(Coordinate startPosition, ShipOrientation orientation)
    {
        EnsureGameInitialized();
        var ship = _shipPlacementService.PlaceShip(_opponentBoard!, startPosition, orientation, _configuration.ShipLength);
        
        // Ships are kept in their actual positions - hiding is handled in the display layer
        return ship;
    }

    public GuessResult ProcessPlayerGuess(string input)
    {
        EnsureGameInitialized();

        if (string.IsNullOrEmpty(input) || input.Length != 2)
            return GuessResult.Failure("Invalid format");

        Coordinate coordinate;
        try
        {
            coordinate = Coordinate.FromString(input);
        }
        catch
        {
            return GuessResult.Failure("Invalid coordinates");
        }

        if (!IsValidCoordinate(coordinate))
            return GuessResult.Failure("Invalid coordinates");

        if (_playerGuesses.Contains(coordinate))
            return GuessResult.Failure("Already guessed");

        _playerGuesses.Add(coordinate);

        var ship = _opponentBoard!.GetShipAt(coordinate);
        bool isHit = ship != null;
        bool isSunk = false;

        if (isHit)
        {
            ship!.Hit(coordinate);
            _opponentBoard.SetCellState(coordinate, CellState.Hit);
            isSunk = ship.IsSunk;
        }
        else
        {
            _opponentBoard.SetCellState(coordinate, CellState.Miss);
        }

        return GuessResult.Success(isHit, isSunk);
    }

    public GuessResult ProcessCpuGuess(Coordinate coordinate)
    {
        EnsureGameInitialized();

        if (_cpuGuesses.Contains(coordinate))
            return GuessResult.Failure("Already guessed");

        _cpuGuesses.Add(coordinate);

        var ship = _playerBoard!.GetShipAt(coordinate);
        bool isHit = ship != null;
        bool isSunk = false;

        if (isHit)
        {
            ship!.Hit(coordinate);
            _playerBoard.SetCellState(coordinate, CellState.Hit);
            isSunk = ship.IsSunk;
        }
        else
        {
            _playerBoard.SetCellState(coordinate, CellState.Miss);
        }

        _cpuPlayerService.ProcessHitResult(coordinate, isHit, isSunk);

        return GuessResult.Success(isHit, isSunk);
    }

    public GameState GetGameState()
    {
        EnsureGameInitialized();

        // Only check for win conditions if ships have been placed
        bool hasOpponentShips = _opponentBoard!.Ships.Count > 0;
        bool hasPlayerShips = _playerBoard!.Ships.Count > 0;

        if (hasOpponentShips && _opponentBoard.GetRemainingShipCount() == 0)
            return GameState.PlayerWon;

        if (hasPlayerShips && _playerBoard.GetRemainingShipCount() == 0)
            return GameState.CpuWon;

        return GameState.InProgress;
    }

    public void AutoPlacePlayerShips()
    {
        EnsureGameInitialized();
        _shipPlacementService.PlaceShipsRandomly(_playerBoard!, _configuration.NumberOfShips, _configuration.ShipLength, _random);
    }

    public void AutoPlaceOpponentShips()
    {
        EnsureGameInitialized();
        _shipPlacementService.PlaceShipsRandomly(_opponentBoard!, _configuration.NumberOfShips, _configuration.ShipLength, _random);
        
        // Ships are kept in their actual positions - hiding is handled in the display layer
    }

    public Coordinate GenerateCpuGuess()
    {
        EnsureGameInitialized();
        return _cpuPlayerService.GenerateGuess(_playerBoard!, _cpuGuesses, _random);
    }

    private void EnsureGameInitialized()
    {
        if (_playerBoard == null || _opponentBoard == null)
            throw new InvalidOperationException("Game must be initialized before use");
    }
} 