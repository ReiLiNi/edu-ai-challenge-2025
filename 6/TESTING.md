# Testing & Coverage Guide

This document explains how to run tests and generate coverage reports for the Enigma Machine project.

## Quick Start

```bash
# Run tests
npm test

# Run tests with coverage
npm run test:coverage

# Generate HTML coverage report
npm run test:coverage-html

# Open HTML coverage report (Windows)
npm run coverage:open
```

## Available Commands

### Testing Commands

| Command | Description |
|---------|-------------|
| `npm test` | Run all unit tests |
| `npm run test:coverage` | Run tests with basic coverage report |
| `npm run test:coverage-html` | Generate HTML coverage report |
| `npm run test:coverage-detailed` | Generate both text and HTML reports |
| `npm run test:coverage-check` | Run tests and check coverage thresholds |
| `npm run test:coverage-json` | Generate JSON coverage report |

### Coverage Utilities

| Command | Description |
|---------|-------------|
| `npm run coverage:open` | Open HTML coverage report in browser |
| `npm run coverage:clean` | Delete coverage directory |

## Coverage Thresholds

The project maintains the following coverage thresholds (configured in `.c8rc.json`):

- **Statements**: 75%
- **Lines**: 75% 
- **Functions**: 90%
- **Branches**: 90%

## Current Coverage Status

✅ **Current Coverage**: 
- **Statements**: 78.98% (exceeds 75% threshold)
- **Lines**: 78.98% (exceeds 75% threshold)  
- **Functions**: 91.66% (exceeds 90% threshold)
- **Branches**: 96.55% (exceeds 90% threshold)

## Uncovered Code

The uncovered lines (112-142, 145-146) are:
- Interactive CLI interface (`promptEnigma()` function)
- Module execution check

This is expected and acceptable - unit tests focus on core functionality, not interactive UI.

## Test Categories

The test suite covers:

1. **🔧 Basic Components**
   - Utility functions (`mod`, alphabet)
   - Constants validation

2. **🔌 Plugboard Functionality**
   - Letter swapping
   - Bidirectional operations
   - Empty configuration handling

3. **⚙️ Rotor Operations**
   - Position tracking
   - Stepping mechanism
   - Forward/backward encryption

4. **🔄 Reciprocal Property**
   - Encrypt → Decrypt cycles
   - Various configurations
   - Settings validation

5. **🔌🔄 Plugboard Integration**
   - Full encryption pipeline
   - Complex configurations

6. **🎯 Edge Cases**
   - Empty inputs
   - Non-alphabetic characters
   - Single characters
   - Repeated patterns

7. **📝 Long Messages**
   - Full sentence encryption
   - Complex settings
   - Real-world scenarios

## HTML Coverage Report

The HTML report provides:
- **Visual code highlighting** (red = uncovered, green = covered)
- **Interactive file navigation**
- **Detailed line-by-line coverage**
- **Branch coverage visualization**

To view the HTML report:
1. Run `npm run test:coverage-html`
2. Open `coverage/index.html` in your browser
   - Or use `npm run coverage:open` on Windows

## Coverage Files

After running coverage, these files are generated:

```
coverage/
├── index.html          # Main coverage report
├── enigma.js.html      # Detailed file coverage
├── prettify.css        # Styling
├── prettify.js         # Code highlighting
├── base.css           # Base styles
└── ...                # Additional assets
```

## Integration with CI/CD

For continuous integration, use:

```bash
npm run test:coverage-check
```

This command will:
- Run all tests
- Generate coverage report
- **Exit with error code 1** if coverage falls below thresholds
- **Exit with code 0** if all tests pass and coverage meets requirements

## Configuration

Coverage settings are configured in `.c8rc.json`:

```json
{
  "reporter": ["text", "html", "json"],
  "check-coverage": true,
  "lines": 75,
  "functions": 90,
  "branches": 90,
  "statements": 75,
  "exclude": [
    "coverage/**",
    "**/*.test.js"
  ]
}
```

## Troubleshooting

**Coverage shows 0%:**
- Ensure tests import from actual source files
- Check module exports are properly configured

**Tests fail:**
- Run `npm test` to see detailed error messages
- Check that all dependencies are installed

**HTML report not opening:**
- Manually navigate to `coverage/index.html`
- Use different browser if needed

## Best Practices

1. **Run coverage before commits** to ensure quality
2. **Aim for >80% coverage** for critical functions
3. **Focus on testing business logic** over UI code
4. **Use HTML reports** for detailed analysis
5. **Set up CI/CD** to enforce coverage thresholds 