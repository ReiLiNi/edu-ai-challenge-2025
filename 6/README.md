# 🔐 Enigma Machine Simulator

A historically accurate JavaScript implementation of the famous WWII German Enigma cipher machine. This simulator replicates the complete electro-mechanical behavior of the original Enigma machine, including rotor stepping, plugboard swapping, and the reciprocal encryption property.

[![Node.js](https://img.shields.io/badge/Node.js-18%2B-green.svg)](https://nodejs.org/)
[![Coverage](https://img.shields.io/badge/Coverage-78.98%25-brightgreen.svg)](./coverage/index.html)
[![Tests](https://img.shields.io/badge/Tests-Passing-brightgreen.svg)](./enigma.test.js)
[![License](https://img.shields.io/badge/License-ISC-blue.svg)](./LICENSE)

## 📖 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Usage Guide](#-usage-guide)
- [Examples](#-examples)
- [Architecture](#-architecture)
- [Testing](#-testing)
- [API Reference](#-api-reference)
- [Historical Context](#-historical-context)
- [Contributing](#-contributing)
- [License](#-license)

## 🔍 Overview

The Enigma machine was a cipher device used by Nazi Germany during World War II to encrypt military communications. This implementation provides:

- **Historical Accuracy**: Faithful reproduction of the original machine's behavior
- **Reciprocal Encryption**: Same settings encrypt and decrypt messages
- **Complete Functionality**: All components including rotors, plugboard, and reflector
- **Interactive CLI**: Easy-to-use command-line interface
- **Programmatic API**: Use as a module in your own projects

## ✨ Features

### Core Functionality
- ✅ **3 Historical Rotors** (I, II, III) with authentic wiring
- ✅ **Plugboard Support** (Steckerbrett) with up to 13 pairs
- ✅ **Ring Settings** (Ringstellung) for additional security
- ✅ **Double-Stepping** mechanism (historically accurate anomaly)
- ✅ **Reciprocal Property** (encryption = decryption with same settings)

### Technical Features
- ✅ **Non-destructive**: Preserves spaces, punctuation, and numbers
- ✅ **Case Insensitive**: Automatically converts to uppercase
- ✅ **Robust Testing**: 78.98% code coverage with comprehensive unit tests
- ✅ **Multiple Interfaces**: CLI and programmatic API
- ✅ **TypeScript Ready**: Full JSDoc documentation

## 🚀 Installation

### Prerequisites
- **Node.js** 16.0.0 or higher
- **npm** (comes with Node.js)

### Install Dependencies
```bash
# Clone or download the project
cd enigma-machine

# Install dependencies
npm install
```

### Verify Installation
```bash
# Run tests to ensure everything works
npm test

# Run the application
node enigma.js
```

## ⚡ Quick Start

### Interactive Mode (CLI)
```bash
node enigma.js
```

**Example Session:**
```
Enter message: HELLO WORLD
Rotor positions (e.g. 0 0 0): 1 4 5
Ring settings (e.g. 0 0 0): 1 4 5
Plugboard pairs (e.g. AB CD): AB CD EF
Output: CNXCG TZCPU
```

### Programmatic Usage
```javascript
const { Enigma } = require('./enigma.js');

// Create Enigma machine
const enigma = new Enigma(
  [0, 1, 2],           // Rotor selection (I, II, III)
  [1, 4, 5],           // Rotor positions
  [1, 4, 5],           // Ring settings
  [['A','B'],['C','D']] // Plugboard pairs
);

// Encrypt message
const encrypted = enigma.process('HELLO WORLD');
console.log('Encrypted:', encrypted);

// Decrypt (create new machine with same settings)
const enigma2 = new Enigma([0, 1, 2], [1, 4, 5], [1, 4, 5], [['A','B'],['C','D']]);
const decrypted = enigma2.process(encrypted);
console.log('Decrypted:', decrypted); // -> HELLO WORLD
```

## 📚 Usage Guide

### Understanding the Settings

#### 1. Rotor Positions (Grundstellung)
- **Range**: 0-25 (A-Z)
- **Purpose**: Starting position of each rotor
- **Example**: `[5, 10, 15]` = rotors start at F, K, P

#### 2. Ring Settings (Ringstellung)
- **Range**: 0-25 (A-Z)
- **Purpose**: Internal rotor offset (like adjusting a combination lock)
- **Effect**: Changes the relationship between rotor position and wiring

#### 3. Plugboard Pairs (Steckerbrett)
- **Format**: Array of letter pairs
- **Limit**: Up to 13 pairs (26 letters max)
- **Example**: `[['A','B'], ['C','D']]` swaps A↔B and C↔D

### Input Formats

#### Interactive Mode
```
Rotor positions: 1 4 5
Ring settings: 1 4 5  
Plugboard pairs: AB CD EF GH
```

#### Programmatic Mode
```javascript
const rotorPositions = [1, 4, 5];
const ringSettings = [1, 4, 5];
const plugboardPairs = [['A','B'], ['C','D'], ['E','F'], ['G','H']];
```

## 💡 Examples

### Example 1: Basic Encryption
```javascript
const { Enigma } = require('./enigma.js');

const enigma = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
const message = "ATTACK AT DAWN";
const encrypted = enigma.process(message);
console.log(`"${message}" -> "${encrypted}"`);
// Output: "ATTACK AT DAWN" -> "QJXQIS XW BSMR"
```

### Example 2: With Plugboard
```javascript
const enigma = new Enigma(
  [0, 1, 2],                    // Rotors I, II, III
  [7, 11, 19],                  // Positions H, L, T
  [3, 8, 14],                   // Ring settings D, I, O
  [['A','B'], ['C','D'], ['E','F']] // Plugboard pairs
);

const secret = enigma.process("TOP SECRET MESSAGE");
console.log('Encrypted:', secret);

// Decrypt with same settings
const enigma2 = new Enigma([0, 1, 2], [7, 11, 19], [3, 8, 14], [['A','B'], ['C','D'], ['E','F']]);
const decrypted = enigma2.process(secret);
console.log('Decrypted:', decrypted); // -> "TOP SECRET MESSAGE"
```

### Example 3: Historical Scenario
```javascript
// Recreate a typical WWII scenario
const uboat = new Enigma(
  [0, 1, 2],           // Standard rotor order
  [16, 0, 13],         // Daily key: Q A N
  [1, 1, 1],           // Ring setting: B B B  
  [['A','R'], ['G','K'], ['O','X']] // Kriegsmarine plugboard
);

const message = "WEATHER REPORT WIND NORTHWEST FORCE 6";
const encrypted = uboat.process(message);
console.log('U-boat transmission:', encrypted);
```

### Example 4: Batch Processing
```javascript
const { Enigma } = require('./enigma.js');

const messages = [
  "OPERATION OVERLORD",
  "PANZER DIVISION MOVE",
  "AIR RAID EXPECTED"
];

const enigma = new Enigma([0, 1, 2], [5, 12, 8], [0, 0, 0], [['S','T'], ['E','R']]);

messages.forEach((msg, i) => {
  // Create new machine for each message (reset rotor positions)
  const machine = new Enigma([0, 1, 2], [5, 12, 8], [0, 0, 0], [['S','T'], ['E','R']]);
  const encrypted = machine.process(msg);
  console.log(`Message ${i + 1}: "${msg}" -> "${encrypted}"`);
});
```

## 🏗️ Architecture

### Core Components

```
┌─────────────────────┐
│    Interactive CLI   │
│   (promptEnigma)    │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   Enigma Class      │
│  - Constructor      │
│  - process()        │
│  - encryptChar()    │
└──────────┬──────────┘
           │
    ┌──────▼──────┐    ┌─────────────┐
    │ Rotor Class │    │ Plugboard   │
    │ - forward() │    │ Function    │
    │ - backward()│    │             │
    │ - step()    │    └─────────────┘
    └─────────────┘
```

### Signal Flow

```
Input Letter
     │
     ▼
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│Plugboard │ -> │ Rotor 3  │ -> │ Rotor 2  │ -> │ Rotor 1  │
│(Input)   │    │(Forward) │    │(Forward) │    │(Forward) │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
                                                       │
                                                       ▼
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│Plugboard │ <- │ Rotor 3  │ <- │ Rotor 2  │ <- │ Rotor 1  │
│(Output)  │    │(Backward)│    │(Backward)│    │(Backward)│
└──────────┘    └──────────┘    └──────────┘    └──────────┘
     │                                                 ▲
     ▼                                                 │
Output Letter                                  ┌──────────┐
                                              │Reflector │
                                              └──────────┘
```

### Class Structure

#### `Enigma` Class
- **Purpose**: Main machine controller
- **Key Methods**:
  - `constructor(rotorIDs, rotorPositions, ringSettings, plugboardPairs)`
  - `process(text)`: Encrypt/decrypt entire message
  - `encryptChar(c)`: Process single character
  - `stepRotors()`: Advance rotor positions

#### `Rotor` Class
- **Purpose**: Individual rotor simulation
- **Key Methods**:
  - `forward(char)`: Encrypt character going into reflector
  - `backward(char)`: Encrypt character coming from reflector
  - `step()`: Advance rotor position
  - `atNotch()`: Check if at turnover position

### File Structure

```
enigma-machine/
├── enigma.js          # Main implementation
├── enigma.test.js     # Comprehensive unit tests
├── package.json       # NPM configuration & scripts
├── .c8rc.json        # Coverage configuration
├── README.md         # This file
├── TESTING.md        # Testing & coverage guide
└── coverage/         # Generated coverage reports
    ├── index.html    # Visual coverage report
    └── ...
```

## 🧪 Testing

This project maintains excellent test coverage with comprehensive unit tests.

### Run Tests
```bash
# Run all tests
npm test

# Run with coverage
npm run test:coverage

# Generate HTML coverage report
npm run test:coverage-html

# Open coverage report in browser
npm run coverage:open
```

### Coverage Stats
- **Statements**: 78.98%
- **Functions**: 91.66%
- **Branches**: 96.55%
- **Lines**: 78.98%

### Test Categories
- ✅ Basic component validation
- ✅ Plugboard functionality
- ✅ Rotor operations
- ✅ Reciprocal property verification
- ✅ Edge cases and error handling
- ✅ Integration tests with complex scenarios

For detailed testing information, see [TESTING.md](./TESTING.md).

## 📋 API Reference

### `Enigma` Class

#### Constructor
```javascript
new Enigma(rotorIDs, rotorPositions, ringSettings, plugboardPairs)
```

**Parameters:**
- `rotorIDs` `{number[]}` - Array of rotor indices [0, 1, 2]
- `rotorPositions` `{number[]}` - Starting positions [0-25]
- `ringSettings` `{number[]}` - Ring offset settings [0-25]
- `plugboardPairs` `{string[][]}` - Letter pair swaps [['A','B'], ...]

#### Methods

##### `process(text)`
Encrypts or decrypts a complete message.

**Parameters:**
- `text` `{string}` - Input message

**Returns:** `{string}` - Encrypted/decrypted message

**Example:**
```javascript
const result = enigma.process("HELLO WORLD");
```

##### `encryptChar(char)`
Encrypts a single character and advances rotors.

**Parameters:**
- `char` `{string}` - Single character

**Returns:** `{string}` - Encrypted character

### `Rotor` Class

#### Constructor
```javascript
new Rotor(wiring, notch, ringSetting, position)
```

#### Methods

##### `forward(char)` / `backward(char)`
Process character through rotor wiring.

##### `step()`
Advance rotor position by one.

##### `atNotch()`
Check if rotor is at turnover position.

### Utility Functions

#### `plugboardSwap(char, pairs)`
Apply plugboard letter swapping.

#### `mod(n, m)`
Proper modulo function handling negative numbers.

## 📜 Historical Context

### The Original Enigma Machine

The Enigma machine was invented by German engineer Arthur Scherbius at the end of World War I. The German military adopted it in the 1920s and used it extensively during WWII.

**Key Historical Facts:**
- **Operators**: Required 3-person team (operator, radio operator, supervisor)
- **Daily Keys**: Settings changed daily using codebooks
- **Variants**: Navy (M4), Army/Air Force (M3), different rotor sets
- **Breaking**: Cracked by Polish mathematicians, later by Alan Turing at Bletchley Park

### This Implementation

This simulator replicates the **M3 Enigma** used by the German Army and Air Force:
- **3 Rotors**: From a set of 5 available rotors (we implement I, II, III)
- **Reflector B**: Fixed reflector (most common variant)
- **Plugboard**: Up to 13 pairs of letters
- **26 Letters**: German alphabet (no numbers in original)

### Security Weaknesses (Historically)

The original Enigma had several weaknesses that aided Allied codebreakers:
1. **No letter could encrypt to itself** (reflector property)
2. **Predictable patterns** in military messages
3. **Human errors** in key selection and operation
4. **Captured machines and codebooks**

This implementation maintains these historical characteristics for authenticity.

## 🎯 Use Cases

### Educational
- **Cryptography courses**: Demonstrate rotor cipher principles
- **History classes**: Show WWII encryption technology
- **Computer science**: Example of electro-mechanical computing

### Development
- **Security research**: Analyze historical cipher weaknesses
- **Algorithm study**: Understand substitution cipher evolution
- **Testing framework**: Example of comprehensive unit testing

### Recreation
- **Puzzle solving**: Create and solve cipher challenges
- **Historical simulation**: Recreate famous encrypted messages
- **Escape rooms**: Generate authentic-looking encrypted clues

## 🛠️ Troubleshooting

### Common Issues

#### "Tests show 0% coverage"
**Solution**: Ensure you're importing from the actual `enigma.js` file:
```javascript
const { Enigma } = require('./enigma.js');
```

#### "Decryption doesn't match original"
**Solution**: Ensure you're using identical machine settings:
```javascript
// WRONG - rotors have stepped during encryption
const encrypted = enigma1.process("HELLO");
const decrypted = enigma1.process(encrypted); // ❌ Wrong result

// CORRECT - create new machine with original settings
const enigma2 = new Enigma([0,1,2], [0,0,0], [0,0,0], []);
const decrypted = enigma2.process(encrypted); // ✅ Correct
```

#### "Plugboard pairs not working"
**Solution**: Check pair format - must be arrays of strings:
```javascript
// WRONG
const pairs = ["AB", "CD"];

// CORRECT  
const pairs = [["A","B"], ["C","D"]];
```

### Getting Help

1. **Check the tests**: `npm test` - they show correct usage
2. **Review examples**: See the examples section above
3. **Read TESTING.md**: Comprehensive testing guide
4. **Check coverage**: `npm run coverage:open` shows which code is tested

## 🤝 Contributing

Contributions are welcome! This project maintains high code quality standards.

### Development Setup
```bash
git clone <repository>
cd enigma-machine
npm install
npm test
```

### Before Submitting
- ✅ All tests pass: `npm test`
- ✅ Coverage maintained: `npm run test:coverage-check`
- ✅ Code follows existing style
- ✅ New features include tests

### Areas for Contribution
- **Additional rotors**: Implement rotors IV, V, VI, VII, VIII
- **M4 Navy variant**: 4-rotor configuration with thin reflector
- **Alternative reflectors**: Reflector C and other variants
- **Performance optimization**: Faster encryption for large texts
- **Historical accuracy**: More authentic rotor stepping edge cases

## 📄 License

ISC License - see LICENSE file for details.

## 🙏 Acknowledgments

- **Alan Turing** and the Bletchley Park codebreakers
- **Polish Cipher Bureau** for initial Enigma breakthroughs
- **Arthur Scherbius** for the original Enigma design
- **Historical references**: Extensive research into original machine specifications

---

**Made with ❤️ for history and cryptography enthusiasts**

*"The Enigma machine is a lesson in the power of mathematics over mere mechanical complexity."* 