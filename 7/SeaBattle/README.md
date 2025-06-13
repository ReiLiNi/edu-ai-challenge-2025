# Sea Battle Game - JavaScript to Modern C# Conversion

This project demonstrates a complete rewrite of a JavaScript sea battle game into modern C# using test-driven development principles. The solution maintains all original functionality while implementing modern software engineering practices.

## Project Structure

```
SeaBattle/
├── SeaBattle.sln                 # Visual Studio solution file
├── SeaBattle.Core/               # Core game logic library
│   ├── Models/                   # Data models
│   │   ├── Board.cs             # Game board management
│   │   ├── Ship.cs              # Ship representation
│   │   ├── Coordinate.cs        # Board coordinates
│   │   ├── GameConfiguration.cs # Game settings
│   │   └── GuessResult.cs       # Result objects
│   ├── Enums/                   # Enumerations
│   │   ├── CellState.cs         # Board cell states
│   │   ├── GameState.cs         # Game status
│   │   └── ShipOrientation.cs   # Ship placement orientation
│   ├── Services/                # Business logic services
│   │   ├── IShipPlacementService.cs
│   │   ├── ShipPlacementService.cs
│   │   ├── ICpuPlayerService.cs
│   │   └── CpuPlayerService.cs
│   ├── ISeaBattleGame.cs        # Main game interface
│   └── SeaBattleGame.cs         # Main game implementation
├── SeaBattle.Console/           # Console application
│   └── Program.cs               # User interface
├── SeaBattle.Tests/             # Unit tests
│   └── SeaBattleGameTests.cs    # Comprehensive test suite
└── seabattle.js                 # Original JavaScript implementation
```

## Key Features

### Modern C# Practices

1. **Separation of Concerns**: Clear separation between game logic, services, and presentation
2. **Dependency Injection**: Services are injected into the main game class
3. **Encapsulation**: No global variables; all state is properly encapsulated
4. **SOLID Principles**: Each class has a single responsibility
5. **Clean Architecture**: Domain models, services, and interfaces are well-structured

### Game Functionality

- **10x10 Game Board**: Standard battleship game dimensions
- **3 Ships per Player**: Each ship is 3 cells long
- **Intelligent CPU AI**: Implements hunt/target strategy like the original
- **Input Validation**: Comprehensive validation for player inputs
- **Game State Management**: Proper tracking of game progress and win conditions

### Code Quality

- **Consistent Naming**: Uses C# naming conventions throughout
- **Type Safety**: Full use of C#'s type system with nullable reference types
- **Error Handling**: Proper exception handling and validation
- **Documentation**: Clear, self-documenting code with meaningful names

## Test-Driven Development Approach

The conversion followed TDD principles:

1. **JavaScript Tests**: First, comprehensive tests were written for the original JavaScript functionality
2. **C# Test Translation**: The JavaScript tests were translated to equivalent C# xUnit tests
3. **Implementation**: The C# implementation was written to pass all tests
4. **Validation**: All tests pass, ensuring feature parity with the original

## Building and Running

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension

### Build
```bash
cd SeaBattle
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Run Game
```bash
cd SeaBattle.Console
dotnet run
```

## Game Rules

1. **Setup**: Both player and CPU have 3 ships (3 cells each) placed randomly
2. **Gameplay**: 
   - Player guesses coordinates (e.g., "34" for row 3, column 4)
   - CPU uses intelligent targeting after scoring hits
   - Game alternates between player and CPU turns
3. **Victory**: First to sink all opponent ships wins

## Architecture Highlights

### Dependency Injection
```csharp
public SeaBattleGame(IShipPlacementService shipPlacementService, ICpuPlayerService cpuPlayerService)
{
    _shipPlacementService = shipPlacementService ?? throw new ArgumentNullException(nameof(shipPlacementService));
    _cpuPlayerService = cpuPlayerService ?? throw new ArgumentNullException(nameof(cpuPlayerService));
}
```

### Immutable Value Objects
```csharp
public readonly record struct Coordinate(int Row, int Column)
{
    public bool IsValid(int boardSize) => 
        Row >= 0 && Row < boardSize && Column >= 0 && Column < boardSize;
}
```

### Service Pattern
```csharp
public interface IShipPlacementService
{
    bool CanPlaceShip(Board board, Coordinate startPosition, ShipOrientation orientation, int shipLength);
    Ship PlaceShip(Board board, Coordinate startPosition, ShipOrientation orientation, int shipLength);
    void PlaceShipsRandomly(Board board, int numberOfShips, int shipLength, Random random);
}
```

## Testing Strategy

The test suite covers:
- **Board Creation and Management**
- **Coordinate Validation**
- **Ship Placement Logic**
- **Guess Processing**
- **Game State Management**
- **Win Condition Detection**
- **Input Validation**
- **CPU AI Logic**

Example test:
```csharp
[Fact]
public void ProcessPlayerGuess_Should_Process_Hit_Correctly()
{
    // Arrange
    _game.InitializeGame();
    _game.PlaceOpponentShip(new Coordinate(0, 0), ShipOrientation.Horizontal);

    // Act
    var result = _game.ProcessPlayerGuess("00");
    var opponentBoard = _game.GetOpponentBoard();

    // Assert
    Assert.True(result.IsValid);
    Assert.True(result.IsHit);
    Assert.False(result.IsSunk);
    Assert.Equal(CellState.Hit, opponentBoard.GetCellState(new Coordinate(0, 0)));
}
```

## Comparison: JavaScript vs C#

| Aspect | JavaScript | Modern C# |
|--------|------------|-----------|
| **Global State** | Multiple global variables | Encapsulated in classes |
| **Type Safety** | Dynamic typing | Strong static typing |
| **Error Handling** | Basic validation | Comprehensive validation with exceptions |
| **Architecture** | Procedural | Object-oriented with services |
| **Testing** | Manual testing | Comprehensive unit test suite |
| **Maintainability** | Harder to extend | Easy to extend and modify |
| **Code Organization** | Single file | Multiple files with clear separation |

## Conclusion

This conversion demonstrates how modern C# practices can transform a simple JavaScript game into a well-structured, maintainable, and extensible application. The use of TDD ensures that all original functionality is preserved while significantly improving code quality and architecture.

The resulting C# implementation is:
- **More Maintainable**: Clear separation of concerns and SOLID principles
- **More Testable**: Comprehensive test coverage with dependency injection
- **More Extensible**: Easy to add new features or modify existing ones
- **More Robust**: Strong typing and comprehensive error handling
- **Production Ready**: Follows enterprise-level coding standards 