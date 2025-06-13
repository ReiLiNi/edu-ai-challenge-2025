using SeaBattle.Core;
using SeaBattle.Core.Models;
using SeaBattle.Core.Enums;
using Xunit;

namespace SeaBattle.Tests;

public class SeaBattleGameTests
{
    private ISeaBattleGame _game;
    private readonly IShipPlacementService _shipPlacementService;
    private readonly ICpuPlayerService _cpuPlayerService;

    public SeaBattleGameTests()
    {
        _shipPlacementService = new ShipPlacementService();
        _cpuPlayerService = new CpuPlayerService();
        _game = new SeaBattleGame(_shipPlacementService, _cpuPlayerService);
    }

    [Fact]
    public void Game_Should_Have_Correct_Initial_Parameters()
    {
        // Arrange & Act
        var config = _game.GetGameConfiguration();

        // Assert
        Assert.Equal(10, config.BoardSize);
        Assert.Equal(3, config.NumberOfShips);
        Assert.Equal(3, config.ShipLength);
    }

    [Fact]
    public void CreateBoard_Should_Initialize_10x10_Boards_With_Water()
    {
        // Arrange & Act
        _game.InitializeGame();
        var playerBoard = _game.GetPlayerBoard();
        var opponentBoard = _game.GetOpponentBoard();

        // Assert
        Assert.Equal(10, playerBoard.Size);
        Assert.Equal(10, opponentBoard.Size);

        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                var playerCell = playerBoard.GetCellState(new Coordinate(row, col));
                var opponentCell = opponentBoard.GetCellState(new Coordinate(row, col));
                
                Assert.Equal(CellState.Water, playerCell);
                Assert.Equal(CellState.Water, opponentCell);
            }
        }
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(5, 5, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(10, 0, false)]
    [InlineData(0, 10, false)]
    public void IsValidCoordinate_Should_Validate_Coordinates_Within_Board_Bounds(int row, int col, bool expected)
    {
        // Arrange
        var coordinate = new Coordinate(row, col);

        // Act
        var result = _game.IsValidCoordinate(coordinate);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidAndNewGuess_Should_Check_For_New_Guesses_Correctly()
    {
        // Arrange
        _game.InitializeGame();
        var alreadyGuessed = new List<Coordinate>
        {
            new(0, 0),
            new(2, 3),
            new(4, 5)
        };

        // Act & Assert
        Assert.False(_game.IsValidAndNewGuess(new Coordinate(0, 0), alreadyGuessed));
        Assert.False(_game.IsValidAndNewGuess(new Coordinate(2, 3), alreadyGuessed));
        Assert.True(_game.IsValidAndNewGuess(new Coordinate(1, 1), alreadyGuessed));
        Assert.False(_game.IsValidAndNewGuess(new Coordinate(-1, 0), alreadyGuessed));
    }

    [Theory]
    [InlineData(0, 0, ShipOrientation.Horizontal, true)]
    [InlineData(0, 7, ShipOrientation.Horizontal, true)]
    [InlineData(0, 8, ShipOrientation.Horizontal, false)] // Would extend beyond board
    [InlineData(9, 0, ShipOrientation.Horizontal, true)]
    public void CanPlaceShip_Should_Validate_Horizontal_Ship_Placement(int row, int col, ShipOrientation orientation, bool expected)
    {
        // Arrange
        _game.InitializeGame();
        var startPosition = new Coordinate(row, col);

        // Act
        var result = _game.CanPlaceShip(startPosition, orientation, 3);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0, ShipOrientation.Vertical, true)]
    [InlineData(7, 0, ShipOrientation.Vertical, true)]
    [InlineData(8, 0, ShipOrientation.Vertical, false)] // Would extend beyond board
    [InlineData(0, 9, ShipOrientation.Vertical, true)]
    public void CanPlaceShip_Should_Validate_Vertical_Ship_Placement(int row, int col, ShipOrientation orientation, bool expected)
    {
        // Arrange
        _game.InitializeGame();
        var startPosition = new Coordinate(row, col);

        // Act
        var result = _game.CanPlaceShip(startPosition, orientation, 3);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PlaceShip_Should_Place_Ship_Correctly_And_Mark_Player_Board()
    {
        // Arrange
        _game.InitializeGame();
        var startPosition = new Coordinate(0, 0);
        var orientation = ShipOrientation.Horizontal;

        // Act
        var ship = _game.PlacePlayerShip(startPosition, orientation);
        var playerBoard = _game.GetPlayerBoard();

        // Assert
        Assert.Equal(3, ship.Positions.Count);
        Assert.Contains(new Coordinate(0, 0), ship.Positions);
        Assert.Contains(new Coordinate(0, 1), ship.Positions);
        Assert.Contains(new Coordinate(0, 2), ship.Positions);

        Assert.Equal(CellState.Ship, playerBoard.GetCellState(new Coordinate(0, 0)));
        Assert.Equal(CellState.Ship, playerBoard.GetCellState(new Coordinate(0, 1)));
        Assert.Equal(CellState.Ship, playerBoard.GetCellState(new Coordinate(0, 2)));
    }

    [Fact]
    public void CanPlaceShip_Should_Detect_Collision_With_Existing_Ships()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlacePlayerShip(new Coordinate(0, 0), ShipOrientation.Horizontal);

        // Act
        var result = _game.CanPlaceShip(new Coordinate(0, 1), ShipOrientation.Vertical, 3);

        // Assert
        Assert.False(result); // Would overlap with existing ship
    }

    [Fact]
    public void Ship_Should_Detect_When_Not_Sunk()
    {
        // Arrange
        var positions = new List<Coordinate>
        {
            new(0, 0),
            new(0, 1),
            new(0, 2)
        };
        var ship = new Ship(positions);
        ship.Hit(new Coordinate(0, 1)); // Hit middle position only

        // Act
        var result = ship.IsSunk;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Ship_Should_Detect_When_Sunk()
    {
        // Arrange
        var positions = new List<Coordinate>
        {
            new(0, 0),
            new(0, 1),
            new(0, 2)
        };
        var ship = new Ship(positions);
        ship.Hit(new Coordinate(0, 0));
        ship.Hit(new Coordinate(0, 1));
        ship.Hit(new Coordinate(0, 2));

        // Act
        var result = ship.IsSunk;

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("0", false, "Invalid format")]
    [InlineData("000", false, "Invalid format")]
    [InlineData(null, false, "Invalid format")]
    [InlineData("", false, "Invalid format")]
    public void ProcessPlayerGuess_Should_Reject_Invalid_Guess_Formats(string input, bool expectedValid, string expectedReason)
    {
        // Arrange
        _game.InitializeGame();

        // Act
        var result = _game.ProcessPlayerGuess(input);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        Assert.Equal(expectedReason, result.Message);
    }

    [Theory]
    [InlineData("99", true)] // 9,9 is valid
    [InlineData("aa", false)] // Invalid coordinates
    public void ProcessPlayerGuess_Should_Validate_Coordinates(string input, bool expectedValid)
    {
        // Arrange
        _game.InitializeGame();

        // Act
        var result = _game.ProcessPlayerGuess(input);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

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

    [Fact]
    public void ProcessPlayerGuess_Should_Process_Miss_Correctly()
    {
        // Arrange
        _game.InitializeGame();

        // Act
        var result = _game.ProcessPlayerGuess("99");
        var opponentBoard = _game.GetOpponentBoard();

        // Assert
        Assert.True(result.IsValid);
        Assert.False(result.IsHit);
        Assert.False(result.IsSunk);
        Assert.Equal(CellState.Miss, opponentBoard.GetCellState(new Coordinate(9, 9)));
    }

    [Fact]
    public void ProcessPlayerGuess_Should_Detect_Sunk_Ship()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlaceOpponentShip(new Coordinate(0, 0), ShipOrientation.Horizontal);

        // Act
        _game.ProcessPlayerGuess("00");
        _game.ProcessPlayerGuess("01");
        var result = _game.ProcessPlayerGuess("02");

        // Assert
        Assert.True(result.IsValid);
        Assert.True(result.IsHit);
        Assert.True(result.IsSunk);
    }

    [Fact]
    public void ProcessPlayerGuess_Should_Prevent_Duplicate_Guesses()
    {
        // Arrange
        _game.InitializeGame();
        _game.ProcessPlayerGuess("00");

        // Act
        var result = _game.ProcessPlayerGuess("00");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Already guessed", result.Message);
    }

    [Fact]
    public void Game_Should_Detect_Player_Victory()
    {
        // Arrange
        _game.InitializeGame();
        
        // Place ships with proper spacing (no adjacent placement)
        _game.PlaceOpponentShip(new Coordinate(0, 0), ShipOrientation.Horizontal); // 0,0 0,1 0,2
        _game.PlaceOpponentShip(new Coordinate(2, 0), ShipOrientation.Horizontal); // 2,0 2,1 2,2
        _game.PlaceOpponentShip(new Coordinate(4, 0), ShipOrientation.Horizontal); // 4,0 4,1 4,2

        // Sink all ships
        string[] positions = { "00", "01", "02", "20", "21", "22", "40", "41", "42" };
        foreach (var position in positions)
        {
            _game.ProcessPlayerGuess(position);
        }

        // Act
        var gameState = _game.GetGameState();

        // Assert
        Assert.Equal(GameState.PlayerWon, gameState);
    }

    [Fact]
    public void Game_Should_Detect_CPU_Victory()
    {
        // Arrange
        _game.InitializeGame();
        
        // Place player ships with proper spacing
        _game.PlacePlayerShip(new Coordinate(0, 0), ShipOrientation.Horizontal); // 0,0 0,1 0,2
        _game.PlacePlayerShip(new Coordinate(2, 0), ShipOrientation.Horizontal); // 2,0 2,1 2,2
        _game.PlacePlayerShip(new Coordinate(4, 0), ShipOrientation.Horizontal); // 4,0 4,1 4,2

        // Simulate CPU sinking all player ships
        Coordinate[] positions = {
            new(0, 0), new(0, 1), new(0, 2),
            new(2, 0), new(2, 1), new(2, 2),
            new(4, 0), new(4, 1), new(4, 2)
        };
        
        foreach (var coordinate in positions)
        {
            _game.ProcessCpuGuess(coordinate);
        }

        // Act
        var gameState = _game.GetGameState();

        // Assert
        Assert.Equal(GameState.CpuWon, gameState);
    }

    [Fact]
    public void Game_Should_Be_In_Progress_Initially()
    {
        // Arrange
        _game.InitializeGame();

        // Act
        var gameState = _game.GetGameState();

        // Assert
        Assert.Equal(GameState.InProgress, gameState);
    }

    // ===== TESTS FOR FIXED ISSUES =====

    [Fact]
    public void PlaceOpponentShip_Should_Keep_Ships_On_Board_For_Game_Logic()
    {
        // Arrange
        _game.InitializeGame();

        // Act
        var ship = _game.PlaceOpponentShip(new Coordinate(0, 0), ShipOrientation.Horizontal);
        var opponentBoard = _game.GetOpponentBoard();

        // Assert - Ships should remain on the board for game logic to work
        Assert.Equal(CellState.Ship, opponentBoard.GetCellState(new Coordinate(0, 0)));
        Assert.Equal(CellState.Ship, opponentBoard.GetCellState(new Coordinate(0, 1)));
        Assert.Equal(CellState.Ship, opponentBoard.GetCellState(new Coordinate(0, 2)));
        
        // Verify the ship is properly tracked
        var shipAtPosition = opponentBoard.GetShipAt(new Coordinate(0, 0));
        Assert.NotNull(shipAtPosition);
        Assert.Equal(ship, shipAtPosition);
    }

    [Fact]
    public void Ships_Cannot_Be_Placed_Adjacent_To_Each_Other()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlacePlayerShip(new Coordinate(0, 0), ShipOrientation.Horizontal); // Places at 0,0 0,1 0,2

        // Act - Try to place ship directly adjacent (touching)
        var canPlaceAdjacent = _game.CanPlaceShip(new Coordinate(1, 0), ShipOrientation.Horizontal, 3); // Would be at 1,0 1,1 1,2

        // Assert - Adjacent placement should NOT be allowed (proper battleship rules)
        Assert.False(canPlaceAdjacent);
    }

    [Fact] 
    public void Ships_Can_Be_Placed_With_Proper_Spacing()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlacePlayerShip(new Coordinate(0, 0), ShipOrientation.Horizontal); // Places at 0,0 0,1 0,2

        // Act - Place ship with at least one empty cell spacing
        var canPlaceWithSpacing = _game.CanPlaceShip(new Coordinate(2, 0), ShipOrientation.Horizontal, 3); // Would be at 2,0 2,1 2,2

        // Assert - Placement with proper spacing should be allowed
        Assert.True(canPlaceWithSpacing);
    }

    [Fact]
    public void GameState_Should_Be_InProgress_When_No_Ships_Placed()
    {
        // Arrange
        _game.InitializeGame();
        // No ships placed yet

        // Act
        var gameState = _game.GetGameState();

        // Assert - Game should be in progress, not show PlayerWon when no ships exist
        Assert.Equal(GameState.InProgress, gameState);
    }

    [Fact]
    public void AutoPlaceOpponentShips_Should_Place_Ships_That_Stay_On_Board()
    {
        // Arrange
        _game.InitializeGame();

        // Act
        _game.AutoPlaceOpponentShips();
        var opponentBoard = _game.GetOpponentBoard();

        // Assert - Should have 3 ships placed and visible on the board
        Assert.Equal(3, opponentBoard.Ships.Count);
        
        // Count ship cells on the board
        int shipCellCount = 0;
        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                if (opponentBoard.GetCellState(new Coordinate(row, col)) == CellState.Ship)
                {
                    shipCellCount++;
                }
            }
        }
        
        // Should have 3 ships * 3 cells each = 9 ship cells
        Assert.Equal(9, shipCellCount);
    }

    [Fact]
    public void AutoPlacePlayerShips_Should_Place_Correct_Number_Of_Ships()
    {
        // Arrange
        _game.InitializeGame();

        // Act
        _game.AutoPlacePlayerShips();
        var playerBoard = _game.GetPlayerBoard();

        // Assert - Should have exactly 3 ships of 3 length each
        Assert.Equal(3, playerBoard.Ships.Count);
        
        // Verify each ship has correct length
        foreach (var ship in playerBoard.Ships)
        {
            Assert.Equal(3, ship.Positions.Count);
        }
        
        // Count ship cells on the board
        int shipCellCount = 0;
        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                if (playerBoard.GetCellState(new Coordinate(row, col)) == CellState.Ship)
                {
                    shipCellCount++;
                }
            }
        }
        
        // Should have 3 ships * 3 cells each = 9 ship cells
        Assert.Equal(9, shipCellCount);
    }

    [Fact]
    public void GameState_Should_Correctly_Detect_Victory_After_Ship_Placement()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlaceOpponentShip(new Coordinate(0, 0), ShipOrientation.Horizontal);
        
        // Verify game is in progress with ships placed
        Assert.Equal(GameState.InProgress, _game.GetGameState());

        // Act - Sink the only opponent ship
        _game.ProcessPlayerGuess("00");
        _game.ProcessPlayerGuess("01");
        _game.ProcessPlayerGuess("02");

        // Assert - Should now detect player victory
        Assert.Equal(GameState.PlayerWon, _game.GetGameState());
    }

    [Fact]
    public void OpponentShip_Hit_Logic_Should_Work_With_Proper_Ship_Placement()
    {
        // Arrange
        _game.InitializeGame();
        var ship = _game.PlaceOpponentShip(new Coordinate(5, 5), ShipOrientation.Vertical);

        // Act - Hit the ship at positions (5,5), (6,5), (7,5)
        var hit1 = _game.ProcessPlayerGuess("55"); // Should hit
        var hit2 = _game.ProcessPlayerGuess("65"); // Should hit  
        var miss = _game.ProcessPlayerGuess("85"); // Should miss (beyond ship)

        // Assert
        Assert.True(hit1.IsHit);
        Assert.False(hit1.IsSunk); // Ship not fully sunk yet
        
        Assert.True(hit2.IsHit);
        Assert.False(hit2.IsSunk); // Still not fully sunk
        
        Assert.False(miss.IsHit); // Should miss
        
        // Finish sinking the ship
        var finalHit = _game.ProcessPlayerGuess("75"); // Hit the last position
        Assert.True(finalHit.IsHit);
        Assert.True(finalHit.IsSunk); // Ship should now be sunk
    }

    [Fact]
    public void Ships_Should_Maintain_Correct_Configuration_From_JavaScript()
    {
        // Arrange & Act
        var config = _game.GetGameConfiguration();

        // Assert - Verify it matches the original JavaScript configuration
        Assert.Equal(10, config.BoardSize); // boardSize = 10;
        Assert.Equal(3, config.NumberOfShips); // numShips = 3;
        Assert.Equal(3, config.ShipLength); // shipLength = 3;
    }

    // ===== ADJACENCY RULE TESTS =====

    [Theory]
    [InlineData(1, 0)] // Directly below
    [InlineData(-1, 0)] // Directly above (would be invalid coordinate, but tests the logic)
    [InlineData(0, 1)] // Directly right (overlaps, but tests the logic)
    [InlineData(1, 1)] // Diagonal bottom-right
    [InlineData(1, -1)] // Diagonal bottom-left
    public void Ships_Cannot_Be_Placed_Adjacent_In_Any_Direction(int rowOffset, int colOffset)
    {
        // Arrange
        _game.InitializeGame();
        _game.PlacePlayerShip(new Coordinate(2, 2), ShipOrientation.Horizontal); // Places at 2,2 2,3 2,4

        // Act - Try to place ship adjacent in the specified direction
        var adjacentPosition = new Coordinate(2 + rowOffset, 2 + colOffset);
        var canPlace = _game.CanPlaceShip(adjacentPosition, ShipOrientation.Horizontal, 3);

        // Assert - Adjacent placement should not be allowed
        Assert.False(canPlace);
    }

    [Fact]
    public void Ships_Can_Be_Placed_With_One_Cell_Spacing_Vertically()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlacePlayerShip(new Coordinate(1, 1), ShipOrientation.Horizontal); // Places at 1,1 1,2 1,3

        // Act - Place ship with one empty row spacing
        var canPlace = _game.CanPlaceShip(new Coordinate(3, 1), ShipOrientation.Horizontal, 3); // Would be at 3,1 3,2 3,3

        // Assert - Should be allowed with proper spacing
        Assert.True(canPlace);
    }

    [Fact]
    public void Ships_Can_Be_Placed_With_One_Cell_Spacing_Horizontally()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlacePlayerShip(new Coordinate(1, 1), ShipOrientation.Vertical); // Places at 1,1 2,1 3,1

        // Act - Place ship with one empty column spacing
        var canPlace = _game.CanPlaceShip(new Coordinate(1, 3), ShipOrientation.Vertical, 3); // Would be at 1,3 2,3 3,3

        // Assert - Should be allowed with proper spacing
        Assert.True(canPlace);
    }

    [Fact]
    public void Diagonal_Adjacency_Is_Also_Prevented()
    {
        // Arrange
        _game.InitializeGame();
        _game.PlacePlayerShip(new Coordinate(1, 1), ShipOrientation.Horizontal); // Places at 1,1 1,2 1,3

        // Act - Try to place ship diagonally adjacent
        var canPlaceDiagonal = _game.CanPlaceShip(new Coordinate(2, 2), ShipOrientation.Vertical, 3); // Would be at 2,2 3,2 4,2

        // Assert - Diagonal adjacency should also be prevented
        Assert.False(canPlaceDiagonal);
    }
} 