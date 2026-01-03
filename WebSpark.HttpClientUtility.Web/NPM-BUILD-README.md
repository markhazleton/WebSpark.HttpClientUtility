# WebSpark.HttpClientUtility.Web - NPM Build Process

## Overview

This project uses a modern NPM build pipeline with **Vite**, **ESLint**, **Stylelint**, and **Prettier** for maximum code quality.

## Prerequisites

- Node.js >= 18.0.0
- NPM >= 9.0.0

## Available Scripts

### Development

```bash
# Start development server with hot module replacement
npm run dev
```

### Production Build

```bash
# Full production build with clean, lint, and optimization
npm run build

# Production build with tests
npm run build:prod
```

### Code Quality

```bash
# Run all linters (JavaScript + CSS)
npm run lint

# Run JavaScript linter only
npm run lint:js

# Run CSS/SCSS linter only
npm run lint:css

# Auto-fix linting issues
npm run lint:fix
npm run lint:js:fix
npm run lint:css:fix

# Format code with Prettier
npm run format

# Check formatting without modifying files
npm run format:check
```

### Utilities

```bash
# Clean build output
npm run clean

# Preview production build
npm run preview
```

## Build Pipeline

The production build process follows these steps:

1. **Clean**: Removes `wwwroot/dist` directory
2. **Lint**: Runs ESLint (JavaScript) and Stylelint (CSS/SCSS) with zero warnings allowed
3. **Build**: Vite bundles and optimizes all assets
   - JavaScript minification with Terser
   - CSS optimization and bundling
   - Asset hashing for cache busting
   - Source maps generation
   - Manifest file creation

## Code Quality Tools

### ESLint (JavaScript)

- **Config**: `eslint.config.js` (ESM flat config)
- **Rules**: ES2022+ best practices, no console/debugger, consistent formatting
- **Max Warnings**: 0 (build fails on any warnings)

### Stylelint (CSS/SCSS)

- **Config**: `.stylelintrc.json`
- **Extends**: `stylelint-config-standard`
- **Rules**: Consistent indentation, quotes, color formats

### Prettier (Formatting)

- **Config**: `.prettierrc.json`
- **Scope**: JavaScript, CSS, SCSS, JSON
- **Style**: Single quotes, 2-space indent, 100 char width

## File Structure

```
WebSpark.HttpClientUtility.Web/
├── ClientApp/
│   ├── src/
│   │   ├── main.js          # Entry point
│   │   ├── site.js          # Site-specific logic
│   │   └── site.css         # Styles
│   └── public/              # Static assets
├── wwwroot/
│   └── dist/                # Build output (generated)
├── package.json             # NPM configuration
├── vite.config.js           # Vite bundler config
├── eslint.config.js         # ESLint rules
├── .stylelintrc.json        # Stylelint rules
├── .prettierrc.json         # Prettier formatting
└── .prettierignore          # Prettier ignore patterns
```

## Integration with ASP.NET Core

The build output in `wwwroot/dist` is served by ASP.NET Core:

- JavaScript: `wwwroot/dist/js/`
- CSS: `wwwroot/dist/css/`
- Assets: `wwwroot/dist/assets/`
- Manifest: `wwwroot/dist/.vite/manifest.json`

## Troubleshooting

### ESLint Errors

```bash
# View all linting errors
npm run lint:js

# Auto-fix fixable issues
npm run lint:js:fix
```

### Stylelint Errors

```bash
# View CSS/SCSS errors
npm run lint:css

# Auto-fix fixable issues
npm run lint:css:fix
```

### Build Failures

If build fails due to linting:

1. Run `npm run lint` to see all errors
2. Run `npm run lint:fix` to auto-fix
3. Manually fix remaining issues
4. Run `npm run build` again

### Clean Rebuild

```bash
# Full clean and rebuild
npm run clean
npm install
npm run build
```

## Best Practices

1. **Always run linters before committing**: `npm run lint`
2. **Format code**: `npm run format`
3. **Check formatting**: `npm run format:check` (use in CI/CD)
4. **Zero warnings policy**: Fix all ESLint warnings before merging
5. **Use `npm run build:prod`** before releasing to catch all issues

## CI/CD Integration

Recommended GitHub Actions workflow:

```yaml
- name: Install dependencies
  run: npm ci
  working-directory: WebSpark.HttpClientUtility.Web

- name: Lint code
  run: npm run lint
  working-directory: WebSpark.HttpClientUtility.Web

- name: Check formatting
  run: npm run format:check
  working-directory: WebSpark.HttpClientUtility.Web

- name: Build production bundle
  run: npm run build
  working-directory: WebSpark.HttpClientUtility.Web
```

## Performance Optimizations

The build pipeline includes:

- **Code Splitting**: Automatic vendor chunk separation
- **Tree Shaking**: Dead code elimination
- **Minification**: Terser for JavaScript, built-in for CSS
- **Asset Hashing**: Cache busting with content hashes
- **Source Maps**: For production debugging
- **Gzip/Brotli**: Optimized bundle sizes

## Dependencies

### Production

- **Bootstrap 5.3.3**: UI framework
- **Bootstrap Icons 1.11.3**: Icon library
- **Popper.js 2.11.8**: Tooltip positioning

### Development

- **Vite 6.0.5**: Build tool and dev server
- **Terser 5.36.0**: JavaScript minification
- **Sass 1.83.4**: CSS preprocessing
- **ESLint 9.17.0**: JavaScript linting
- **Stylelint 16.12.0**: CSS linting
- **Prettier 3.4.2**: Code formatting

## Version History

- **v2.0.0**: Modern build pipeline with full linting and quality checks
- **v1.x**: Legacy build configuration

## License

MIT License - See main project LICENSE file
