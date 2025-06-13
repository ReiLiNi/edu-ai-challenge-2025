// JavaScript tests for the original seabattle.js functionality
// These tests document the expected behavior before C# rewrite

const assert = require('assert');

// Mock the original game components for testing
class SeaBattleGame {
  constructor() {
    this.boardSize = 10;
    this.numShips = 3;
    this.shipLength = 3;
    this.playerShips = [];
    this.cpuShips = [];
    this.playerNumShips = this.numShips;
    this.cpuNumShips = this.numShips;
    this.guesses = [];
    this.cpuGuesses = [];
    this.cpuMode = 'hunt';
    this.cpuTargetQueue = [];
    this.board = [];
    this.playerBoard = [];
  }

  createBoard() {
    for (let i = 0; i < this.boardSize; i++) {
      this.board[i] = [];
      this.playerBoard[i] = [];
      for (let j = 0; j < this.boardSize; j++) {
        this.board[i][j] = '~';
        this.playerBoard[i][j] = '~';
      }
    }
  }

  isValidCoordinate(row, col) {
    return row >= 0 && row < this.boardSize && col >= 0 && col < this.boardSize;
  }

  isValidAndNewGuess(row, col, guessList) {
    if (!this.isValidCoordinate(row, col)) return false;
    const guessStr = String(row) + String(col);
    return guessList.indexOf(guessStr) === -1;
  }

  canPlaceShip(startRow, startCol, orientation) {
    for (let i = 0; i < this.shipLength; i++) {
      let checkRow = startRow;
      let checkCol = startCol;
      if (orientation === 'horizontal') {
        checkCol += i;
      } else {
        checkRow += i;
      }
      
      if (!this.isValidCoordinate(checkRow, checkCol)) return false;
      if (this.playerBoard[checkRow][checkCol] !== '~') return false;
    }
    return true;
  }

  placeShip(startRow, startCol, orientation, targetBoard, shipsArray) {
    const newShip = { locations: [], hits: [] };
    
    for (let i = 0; i < this.shipLength; i++) {
      let placeRow = startRow;
      let placeCol = startCol;
      if (orientation === 'horizontal') {
        placeCol += i;
      } else {
        placeRow += i;
      }
      
      const locationStr = String(placeRow) + String(placeCol);
      newShip.locations.push(locationStr);
      newShip.hits.push('');
      
      if (targetBoard === this.playerBoard) {
        targetBoard[placeRow][placeCol] = 'S';
      }
    }
    
    shipsArray.push(newShip);
    return newShip;
  }

  isSunk(ship) {
    for (let i = 0; i < this.shipLength; i++) {
      if (ship.hits[i] !== 'hit') {
        return false;
      }
    }
    return true;
  }

  processGuess(guess, targetShips, targetBoard) {
    if (!guess || guess.length !== 2) return { valid: false, reason: 'Invalid format' };
    
    const row = parseInt(guess[0]);
    const col = parseInt(guess[1]);
    
    if (isNaN(row) || isNaN(col) || !this.isValidCoordinate(row, col)) {
      return { valid: false, reason: 'Invalid coordinates' };
    }
    
    if (this.guesses.includes(guess)) {
      return { valid: false, reason: 'Already guessed' };
    }
    
    this.guesses.push(guess);
    
    let hit = false;
    let sunk = false;
    
    for (let ship of targetShips) {
      const index = ship.locations.indexOf(guess);
      if (index >= 0 && ship.hits[index] !== 'hit') {
        ship.hits[index] = 'hit';
        targetBoard[row][col] = 'X';
        hit = true;
        sunk = this.isSunk(ship);
        break;
      }
    }
    
    if (!hit) {
      targetBoard[row][col] = 'O';
    }
    
    return { valid: true, hit, sunk };
  }
}

// Test Suite
describe('SeaBattle Game Tests', () => {
  let game;
  
  beforeEach(() => {
    game = new SeaBattleGame();
    game.createBoard();
  });

  describe('Board Creation', () => {
    it('should create 10x10 boards filled with water (~)', () => {
      assert.equal(game.board.length, 10);
      assert.equal(game.playerBoard.length, 10);
      
      for (let i = 0; i < 10; i++) {
        assert.equal(game.board[i].length, 10);
        assert.equal(game.playerBoard[i].length, 10);
        
        for (let j = 0; j < 10; j++) {
          assert.equal(game.board[i][j], '~');
          assert.equal(game.playerBoard[i][j], '~');
        }
      }
    });
  });

  describe('Coordinate Validation', () => {
    it('should validate coordinates within board bounds', () => {
      assert.equal(game.isValidCoordinate(0, 0), true);
      assert.equal(game.isValidCoordinate(9, 9), true);
      assert.equal(game.isValidCoordinate(5, 5), true);
      assert.equal(game.isValidCoordinate(-1, 0), false);
      assert.equal(game.isValidCoordinate(0, -1), false);
      assert.equal(game.isValidCoordinate(10, 0), false);
      assert.equal(game.isValidCoordinate(0, 10), false);
    });

    it('should check for new guesses correctly', () => {
      const guesses = ['00', '23', '45'];
      assert.equal(game.isValidAndNewGuess(0, 0, guesses), false);
      assert.equal(game.isValidAndNewGuess(2, 3, guesses), false);
      assert.equal(game.isValidAndNewGuess(1, 1, guesses), true);
      assert.equal(game.isValidAndNewGuess(-1, 0, guesses), false);
    });
  });

  describe('Ship Placement', () => {
    it('should validate horizontal ship placement', () => {
      assert.equal(game.canPlaceShip(0, 0, 'horizontal'), true);
      assert.equal(game.canPlaceShip(0, 7, 'horizontal'), true);
      assert.equal(game.canPlaceShip(0, 8, 'horizontal'), false); // Would extend beyond board
      assert.equal(game.canPlaceShip(9, 0, 'horizontal'), true);
    });

    it('should validate vertical ship placement', () => {
      assert.equal(game.canPlaceShip(0, 0, 'vertical'), true);
      assert.equal(game.canPlaceShip(7, 0, 'vertical'), true);
      assert.equal(game.canPlaceShip(8, 0, 'vertical'), false); // Would extend beyond board
      assert.equal(game.canPlaceShip(0, 9, 'vertical'), true);
    });

    it('should place ship correctly and mark player board', () => {
      const ship = game.placeShip(0, 0, 'horizontal', game.playerBoard, game.playerShips);
      
      assert.equal(ship.locations.length, 3);
      assert.equal(ship.locations[0], '00');
      assert.equal(ship.locations[1], '01');
      assert.equal(ship.locations[2], '02');
      
      assert.equal(game.playerBoard[0][0], 'S');
      assert.equal(game.playerBoard[0][1], 'S');
      assert.equal(game.playerBoard[0][2], 'S');
    });

    it('should detect collision with existing ships', () => {
      game.placeShip(0, 0, 'horizontal', game.playerBoard, game.playerShips);
      assert.equal(game.canPlaceShip(0, 1, 'vertical'), false); // Would overlap
    });
  });

  describe('Ship Status', () => {
    it('should detect when ship is not sunk', () => {
      const ship = { locations: ['00', '01', '02'], hits: ['', 'hit', ''] };
      assert.equal(game.isSunk(ship), false);
    });

    it('should detect when ship is sunk', () => {
      const ship = { locations: ['00', '01', '02'], hits: ['hit', 'hit', 'hit'] };
      assert.equal(game.isSunk(ship), true);
    });
  });

  describe('Guess Processing', () => {
    beforeEach(() => {
      // Place a ship for testing
      game.placeShip(0, 0, 'horizontal', game.playerBoard, game.cpuShips);
    });

    it('should reject invalid guess formats', () => {
      const result1 = game.processGuess('0', game.cpuShips, game.board);
      assert.equal(result1.valid, false);
      assert.equal(result1.reason, 'Invalid format');

      const result2 = game.processGuess('000', game.cpuShips, game.board);
      assert.equal(result2.valid, false);
      assert.equal(result2.reason, 'Invalid format');
    });

    it('should reject invalid coordinates', () => {
      const result = game.processGuess('99', game.cpuShips, game.board); // 9,9 is valid
      assert.equal(result.valid, true);

      const result2 = game.processGuess('aa', game.cpuShips, game.board);
      assert.equal(result2.valid, false);
      assert.equal(result2.reason, 'Invalid coordinates');
    });

    it('should process hit correctly', () => {
      const result = game.processGuess('00', game.cpuShips, game.board);
      assert.equal(result.valid, true);
      assert.equal(result.hit, true);
      assert.equal(result.sunk, false);
      assert.equal(game.board[0][0], 'X');
    });

    it('should process miss correctly', () => {
      const result = game.processGuess('99', game.cpuShips, game.board);
      assert.equal(result.valid, true);
      assert.equal(result.hit, false);
      assert.equal(result.sunk, false);
      assert.equal(game.board[9][9], 'O');
    });

    it('should detect sunk ship', () => {
      game.processGuess('00', game.cpuShips, game.board);
      game.processGuess('01', game.cpuShips, game.board);
      const result = game.processGuess('02', game.cpuShips, game.board);
      
      assert.equal(result.valid, true);
      assert.equal(result.hit, true);
      assert.equal(result.sunk, true);
    });

    it('should prevent duplicate guesses', () => {
      game.processGuess('00', game.cpuShips, game.board);
      const result = game.processGuess('00', game.cpuShips, game.board);
      
      assert.equal(result.valid, false);
      assert.equal(result.reason, 'Already guessed');
    });
  });

  describe('Game Constants', () => {
    it('should have correct game parameters', () => {
      assert.equal(game.boardSize, 10);
      assert.equal(game.numShips, 3);
      assert.equal(game.shipLength, 3);
    });
  });
});

// Export for running tests
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { SeaBattleGame };
} 