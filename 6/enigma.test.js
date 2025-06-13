const assert = require('assert');

// Import from the actual enigma.js file
const { Enigma, Rotor, plugboardSwap, ROTORS, REFLECTOR, alphabet, mod } = require('./enigma.js');

class EnigmaTests {
  static runAllTests() {
    console.log('🧪 Running Enigma Machine Unit Tests...\n');
    
    try {
      this.testBasicComponents();
      this.testPlugboardSwap();
      this.testRotorFunctionality();
      this.testReciprocalProperty();
      this.testRotorStepping();
      this.testPlugboardReciprocity();
      this.testEdgeCases();
      this.testLongerMessages();
      
      console.log('✅ All tests passed! Enigma machine is working correctly.\n');
    } catch (error) {
      console.error('❌ Test failed:', error.message);
      console.error(error.stack);
      process.exit(1);
    }
  }

  static testBasicComponents() {
    console.log('🔧 Testing basic components...');
    
    // Test mod function
    assert.strictEqual(mod(-1, 26), 25, 'Negative modulo should work correctly');
    assert.strictEqual(mod(27, 26), 1, 'Positive modulo should work correctly');
    
    // Test alphabet
    assert.strictEqual(alphabet.length, 26, 'Alphabet should have 26 characters');
    assert.strictEqual(alphabet[0], 'A', 'Alphabet should start with A');
    assert.strictEqual(alphabet[25], 'Z', 'Alphabet should end with Z');
    
    console.log('  ✓ Basic components working');
  }

  static testPlugboardSwap() {
    console.log('🔌 Testing plugboard swap...');
    
    const pairs = [['A', 'B'], ['C', 'D']];
    
    // Test swapping
    assert.strictEqual(plugboardSwap('A', pairs), 'B', 'A should swap to B');
    assert.strictEqual(plugboardSwap('B', pairs), 'A', 'B should swap to A');
    assert.strictEqual(plugboardSwap('C', pairs), 'D', 'C should swap to D');
    assert.strictEqual(plugboardSwap('D', pairs), 'C', 'D should swap to C');
    
    // Test non-swapped letters
    assert.strictEqual(plugboardSwap('E', pairs), 'E', 'E should remain unchanged');
    assert.strictEqual(plugboardSwap('Z', pairs), 'Z', 'Z should remain unchanged');
    
    // Test empty pairs
    assert.strictEqual(plugboardSwap('A', []), 'A', 'No pairs should leave letters unchanged');
    
    console.log('  ✓ Plugboard swap working');
  }

  static testRotorFunctionality() {
    console.log('⚙️ Testing rotor functionality...');
    
    const rotor = new Rotor(ROTORS[0].wiring, ROTORS[0].notch, 0, 0);
    
    // Test initial position
    assert.strictEqual(rotor.position, 0, 'Initial position should be 0');
    assert.strictEqual(rotor.atNotch(), false, 'Should not be at notch initially');
    
    // Test stepping
    rotor.step();
    assert.strictEqual(rotor.position, 1, 'Position should increment');
    
    // Test forward/backward consistency
    const originalChar = 'A';
    const forward = rotor.forward(originalChar);
    const backward = rotor.backward(forward);
    assert.strictEqual(backward, originalChar, 'Forward then backward should return original');
    
    console.log('  ✓ Rotor functionality working');
  }

  static testReciprocalProperty() {
    console.log('🔄 Testing reciprocal property...');
    
    const settings = {
      rotorPositions: [0, 0, 0],
      ringSettings: [0, 0, 0],
      plugboardPairs: []
    };
    
    // Test basic reciprocity
    const enigma1 = new Enigma([0, 1, 2], settings.rotorPositions, settings.ringSettings, settings.plugboardPairs);
    const plaintext = 'HELLO';
    const ciphertext = enigma1.process(plaintext);
    
    const enigma2 = new Enigma([0, 1, 2], settings.rotorPositions, settings.ringSettings, settings.plugboardPairs);
    const decrypted = enigma2.process(ciphertext);
    
    assert.strictEqual(decrypted, plaintext, `Decryption should return original: ${plaintext} -> ${ciphertext} -> ${decrypted}`);
    
    // Test with different starting positions
    const settings2 = {
      rotorPositions: [5, 10, 15],
      ringSettings: [1, 2, 3],
      plugboardPairs: []
    };
    
    const enigma3 = new Enigma([0, 1, 2], settings2.rotorPositions, settings2.ringSettings, settings2.plugboardPairs);
    const ciphertext2 = enigma3.process(plaintext);
    
    const enigma4 = new Enigma([0, 1, 2], settings2.rotorPositions, settings2.ringSettings, settings2.plugboardPairs);
    const decrypted2 = enigma4.process(ciphertext2);
    
    assert.strictEqual(decrypted2, plaintext, `Complex settings should also work: ${plaintext} -> ${ciphertext2} -> ${decrypted2}`);
    
    console.log('  ✓ Reciprocal property working');
  }

  static testRotorStepping() {
    console.log('🔄 Testing rotor stepping mechanism...');
    
    // Test basic stepping (rightmost rotor always steps)
    const enigma = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
    const initialPositions = enigma.rotors.map(r => r.position);
    
    enigma.encryptChar('A');
    
    assert.strictEqual(enigma.rotors[2].position, (initialPositions[2] + 1) % 26, 'Right rotor should always step');
    
    console.log('  ✓ Rotor stepping working');
  }

  static testPlugboardReciprocity() {
    console.log('🔌🔄 Testing plugboard reciprocity...');
    
    const plugboardPairs = [['A', 'B'], ['C', 'D'], ['E', 'F']];
    const settings = {
      rotorPositions: [1, 4, 5],
      ringSettings: [1, 4, 5],
      plugboardPairs: plugboardPairs
    };
    
    const plaintext = 'HELLO';
    
    const enigma1 = new Enigma([0, 1, 2], settings.rotorPositions, settings.ringSettings, settings.plugboardPairs);
    const ciphertext = enigma1.process(plaintext);
    
    const enigma2 = new Enigma([0, 1, 2], settings.rotorPositions, settings.ringSettings, settings.plugboardPairs);
    const decrypted = enigma2.process(ciphertext);
    
    assert.strictEqual(decrypted, plaintext, `Plugboard reciprocity failed: ${plaintext} -> ${ciphertext} -> ${decrypted}`);
    
    // Test with plugboard pairs that affect the message
    const plaintext2 = 'ABCDEF';
    const enigma3 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], plugboardPairs);
    const ciphertext2 = enigma3.process(plaintext2);
    
    const enigma4 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], plugboardPairs);
    const decrypted2 = enigma4.process(ciphertext2);
    
    assert.strictEqual(decrypted2, plaintext2, `Plugboard with affected letters failed: ${plaintext2} -> ${ciphertext2} -> ${decrypted2}`);
    
    console.log('  ✓ Plugboard reciprocity working');
  }

  static testEdgeCases() {
    console.log('🎯 Testing edge cases...');
    
    // Test empty message
    const enigma1 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
    assert.strictEqual(enigma1.process(''), '', 'Empty message should return empty');
    
    // Test non-alphabetic characters
    const enigma2 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
    const mixed = 'HELLO, WORLD! 123';
    const result = enigma2.process(mixed);
    
    // Non-alphabetic characters should pass through unchanged
    assert.strictEqual(result.includes(','), true, 'Comma should pass through');
    assert.strictEqual(result.includes('!'), true, 'Exclamation should pass through');
    assert.strictEqual(result.includes(' '), true, 'Space should pass through');
    assert.strictEqual(result.includes('1'), true, 'Numbers should pass through');
    
    // Test single character
    const enigma3 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
    const single = enigma3.process('A');
    const enigma4 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
    const singleBack = enigma4.process(single);
    assert.strictEqual(singleBack, 'A', 'Single character should be reciprocal');
    
    // Test all same character
    const enigma5 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
    const repeated = 'AAAAA';
    const encryptedRepeated = enigma5.process(repeated);
    
    // Should not encrypt to same repeated character (rotor stepping ensures variation)
    assert.notStrictEqual(encryptedRepeated, repeated, 'Repeated characters should encrypt differently due to stepping');
    
    // But should still decrypt properly
    const enigma6 = new Enigma([0, 1, 2], [0, 0, 0], [0, 0, 0], []);
    const decryptedRepeated = enigma6.process(encryptedRepeated);
    assert.strictEqual(decryptedRepeated, repeated, 'Repeated characters should decrypt correctly');
    
    console.log('  ✓ Edge cases working');
  }

  static testLongerMessages() {
    console.log('📝 Testing longer messages...');
    
    const longMessage = 'THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG';
    const settings = {
      rotorPositions: [7, 11, 19],
      ringSettings: [3, 8, 14],
      plugboardPairs: [['T', 'Q'], ['H', 'E'], ['R', 'S']]
    };
    
    const enigma1 = new Enigma([0, 1, 2], settings.rotorPositions, settings.ringSettings, settings.plugboardPairs);
    const encrypted = enigma1.process(longMessage);
    
    const enigma2 = new Enigma([0, 1, 2], settings.rotorPositions, settings.ringSettings, settings.plugboardPairs);
    const decrypted = enigma2.process(encrypted);
    
    assert.strictEqual(decrypted, longMessage, `Long message reciprocity failed:\nOriginal: ${longMessage}\nEncrypted: ${encrypted}\nDecrypted: ${decrypted}`);
    
    console.log('  ✓ Longer messages working');
  }
}

// Run tests if this file is executed directly
if (require.main === module) {
  EnigmaTests.runAllTests();
}

module.exports = EnigmaTests; 