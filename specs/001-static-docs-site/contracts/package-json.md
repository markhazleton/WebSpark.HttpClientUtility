# Contract: package.json

**Location**: `/src/package.json`  
**Purpose**: NPM dependency and script configuration for Eleventy build system

## Required Structure

```json
{
  "name": "webspark-httpclientutility-docs",
  "version": "1.4.0",
  "description": "Documentation website for WebSpark.HttpClientUtility NuGet package",
  "private": true,
  "scripts": {
    "clean": "rimraf ../docs",
    "dev": "eleventy --serve --incremental",
    "build": "npm run clean && eleventy",
    "build:prod": "npm run clean && NODE_ENV=production eleventy",
    "test:links": "hyperlink ../docs",
    "test:html": "htmlhint ../docs/**/*.html",
    "validate": "npm run test:html && npm run test:links"
  },
  "keywords": [
    "documentation",
    "eleventy",
    "static-site",
    "webspark",
    "httpclient"
  ],
  "author": "Mark Hazleton",
  "license": "MIT",
  "devDependencies": {
    "@11ty/eleventy": "^3.0.0",
    "rimraf": "^5.0.5",
    "htmlhint": "^1.1.4",
    "hyperlink": "^5.0.4"
  },
  "dependencies": {
    "node-fetch": "^3.3.2"
  },
  "engines": {
    "node": ">=20.0.0",
    "npm": ">=10.0.0"
  }
}
```

## Contract Rules

### Dependencies

**Production Dependencies** (`dependencies`):
- `node-fetch`: Required for NuGet API calls in build-time data files
- Minimal - only what's needed for build process

**Development Dependencies** (`devDependencies`):
- `@11ty/eleventy`: Static site generator (REQUIRED, v3.0+)
- `rimraf`: Cross-platform directory removal (REQUIRED for clean script)
- `htmlhint`: HTML validation (OPTIONAL but recommended)
- `hyperlink`: Link checker (OPTIONAL but recommended)

### Scripts

**REQUIRED Scripts**:
1. **`clean`**: Remove entire /docs directory
   - MUST use `rimraf` for cross-platform compatibility
   - MUST target `../docs` (relative to /src)
   - MUST complete without errors even if /docs doesn't exist

2. **`dev`**: Development server with live reload
   - MUST use `eleventy --serve`
   - SHOULD use `--incremental` for faster rebuilds
   - MUST start server on http://localhost:8080 by default

3. **`build`**: Production build
   - MUST run `clean` first
   - MUST run `eleventy` (production mode)
   - MUST output to `/docs` directory
   - MUST complete without errors

**OPTIONAL Scripts**:
4. **`build:prod`**: Production build with environment variable
   - Sets `NODE_ENV=production` for minification
   - Otherwise same as `build`

5. **`test:links`**: Validate all links
   - Runs hyperlink checker on /docs
   - Fails on broken links

6. **`test:html`**: Validate HTML
   - Runs htmlhint on all generated HTML
   - Fails on validation errors

7. **`validate`**: Run all tests
   - Combines `test:html` and `test:links`
   - Use in CI/CD pipeline

### Version Management

- **`version`**: MUST match library version (synchronized)
- **`name`**: MUST be unique, lowercase, kebab-case
- **`private`**: MUST be `true` (not published to npm)

### Node Version

- **`engines.node`**: `>=20.0.0` (Node.js 20 LTS minimum)
- **`engines.npm`**: `>=10.0.0` (npm 10+ for lock file v3)

## Validation

### Pre-Commit Checks
```bash
# Verify package.json is valid JSON
npm pkg get name

# Verify engines match installed versions
node --version  # Should be >=20.0.0
npm --version   # Should be >=10.0.0

# Verify dependencies install cleanly
npm ci
```

### CI/CD Integration
```yaml
# In GitHub Actions workflow
- name: Validate package.json
  run: |
    cd src
    npm ci
    npm run build
    npm run validate
```

## Breaking Changes

Changes that require version bump in library:

**MAJOR**:
- Remove required script
- Change build output directory from /docs
- Upgrade Node.js requirement beyond LTS

**MINOR**:
- Add new optional script
- Add new optional dependency
- Add new validation tool

**PATCH**:
- Update dependency patch versions
- Fix script bugs

## Example Usage

```bash
# Initial setup
cd src
npm install

# Development workflow
npm run dev
# Visit http://localhost:8080
# Edit files, see live updates

# Production build
npm run build

# Validate build
npm run validate

# Clean rebuild
npm run clean && npm run build
```

## Related Contracts

- [eleventy-config.md](./eleventy-config.md) - Eleventy configuration
- [github-actions.md](./github-actions.md) - CI/CD pipeline
