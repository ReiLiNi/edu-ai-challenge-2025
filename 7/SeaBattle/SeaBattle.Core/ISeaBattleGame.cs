using SeaBattle.Core.Enums;
using SeaBattle.Core.Models;

namespace SeaBattle.Core;

public interface ISeaBattleGame
{
    GameConfiguration GetGameConfiguration();
    void InitializeGame();
    Board GetPlayerBoard();
    Board GetOpponentBoard();
    bool IsValidCoordinate(Coordinate coordinate);
    bool IsValidAndNewGuess(Coordinate coordinate, List<Coordinate> previousGuesses);
    bool CanPlaceShip(Coordinate startPosition, ShipOrientation orientation, int shipLength);
    Ship PlacePlayerShip(Coordinate startPosition, ShipOrientation orientation);
    Ship PlaceOpponentShip(Coordinate startPosition, ShipOrientation orientation);
    GuessResult ProcessPlayerGuess(string input);
    GuessResult ProcessCpuGuess(Coordinate coordinate);
    GameState GetGameState();
    void AutoPlacePlayerShips();
    void AutoPlaceOpponentShips();
    Coordinate GenerateCpuGuess();
} 