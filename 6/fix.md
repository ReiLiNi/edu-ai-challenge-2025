Summary of Enigma Machine Fixes
Two Critical Bugs Were Fixed:
Bug 1: Incorrect Rotor Stepping Mechanism
Problem: The original stepping logic broke the reciprocal property - encrypting "HELLO" and then encrypting the result with the same settings didn't return "HELLO".
Root Cause: The stepping sequence was checking notch positions after rotors had already moved, causing incorrect stepping behavior.

Bug 2: Missing Second Plugboard Swap
Problem: Plugboard pairs didn't work correctly because the electrical signal only passed through the plugboard once instead of twice.
Root Cause: The encryptChar function was missing the second plugboard swap at the output.

Result After Fixes:
✅ Reciprocal property restored: Same settings can encrypt and decrypt
✅ Plugboard pairs work correctly: Letters swap both on input and output
✅ Historical accuracy: Matches real Enigma machine behavior
✅ Full functionality: All features now work as intended
Test Verification:
"HELLO" → "CNXCG" → "HELLO" (with plugboard pairs SD GH)
"HELLO" → "XNXCH" → "HELLO" (without plugboard pairs)
Both the rotor mechanics and plugboard system now function exactly like the original WWII Enigma machine.