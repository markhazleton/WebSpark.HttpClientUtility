# Modern NPM Build Pipeline for WebSpark.HttpClientUtility.Web

## Overview

This demo application uses a **modern NPM + Vite build pipeline** to manage client-side libraries (Bootstrap, Bootstrap Icons) instead of the legacy LibMan approach. This provides:

- ✅ **Industry-standard tooling** (NPM, Vite)
- ✅ **Better performance** (tree-shaking, code splitting, minification)
- ✅ **Modern ES modules** with proper dependency resolution
- ✅ **Development experience** (Hot Module Replacement - HMR)
- ✅ **Optimized production builds** (hashed filenames for cache busting)

## Architecture

```
WebSpark.HttpClientUtility.Web/
├── ClientApp/                  # Source files (NOT served directly)
│   ├── src/
│   │   ├── main.js            # JavaScript entry point
│   │   ├── site.js            # Custom app logic
│   │   └── site.css           # Custom styles (imports Bootstrap)
│   └── public/                # Static assets to copy
├── wwwroot/
│   └── dist/                  # Vite build output (served to browser)
│       ├── .vite/
│       │   └── manifest.json  # Asset mapping with hashes
│       ├── css/
│       │   └── site.[hash].css
│       └── js/
│           └── main.[hash].js
├── package.json               # NPM dependencies & scripts
├── vite.config.js             # Vite build configuration
└── WebSpark.HttpClientUtility.Web.csproj  # MSBuild integration
```

## Getting Started

### Prerequisites

- **Node.js 18+** and **NPM 9+** (check with `node --version` and `npm --version`)
- **.NET 8/9/10 SDK**

### Initial Setup

1. **Install NPM dependencies** (done automatically on first build):
   ```powershell
   cd WebSpark.HttpClientUtility.Web
   npm install
   ```

2. **Build assets** (done automatically by MSBuild):
   ```powershell
   npm run build
   ```

3. **Run the application**:
   ```powershell
   dotnet run
   ```

The MSBuild integration will automatically:
- Run `npm install` if `node_modules/` doesn't exist
- Run `npm run build` before ASP.NET Core compilation

## NPM Scripts

| Script | Command | Purpose |
|--------|---------|---------|
| `dev` | `npm run dev` | Start Vite dev server with HMR (port 5173) |
| `build` | `npm run build` | Production build to `wwwroot/dist/` |
| `preview` | `npm run preview` | Preview production build locally |
| `clean` | `npm run clean` | Delete `wwwroot/dist/` directory |

## Development Workflow

### Option 1: Standard ASP.NET Core Development (Recommended)

Just use Visual Studio or `dotnet run`:

```powershell
dotnet run --project WebSpark.HttpClientUtility.Web
```

The application will:
1. Auto-install NPM packages (first run only)
2. Build Vite assets to `wwwroot/dist/`
3. Start ASP.NET Core on configured ports
4. Serve optimized, hashed assets

**Fallback Mode**: If Vite hasn't built yet, the app falls back to `wwwroot/lib/` (old Bootstrap files) so the app still runs during initial development.

### Option 2: Vite Development Server with HMR (Advanced)

For instant Hot Module Replacement during UI work:

1. **Terminal 1** - Start Vite dev server:
   ```powershell
   cd WebSpark.HttpClientUtility.Web
   npm run dev
   ```

2. **Terminal 2** - Start ASP.NET Core:
   ```powershell
   dotnet run --project WebSpark.HttpClientUtility.Web
   ```

3. Update `_Layout.cshtml` temporarily to use Vite dev server:
   ```html
   <script type="module" src="http://localhost:5173/@vite/client"></script>
   <script type="module" src="http://localhost:5173/src/main.js"></script>
   ```

Changes to `ClientApp/src/*` files will reflect instantly in the browser.

## MSBuild Integration

The `.csproj` file includes custom targets:

```xml
<!-- Automatically runs before ASP.NET Core build -->
<Target Name="NpmInstall" BeforeTargets="Build" Condition="!Exists('node_modules')">
  <Exec Command="npm install" />
</Target>

<Target Name="NpmBuild" BeforeTargets="Build" DependsOnTargets="NpmInstall">
  <Exec Command="npm run build" />
</Target>

<!-- Runs when you clean the solution -->
<Target Name="NpmClean" AfterTargets="Clean">
  <RemoveDir Directories="$(ProjectDir)wwwroot\dist" />
</Target>
```

### Deep Clean (Remove node_modules)

```powershell
dotnet clean -p:DeepClean=true
```

This removes `node_modules/` and `package-lock.json` for a fresh start.

## Adding New Client-Side Dependencies

### Example: Adding DataTables

1. **Install via NPM**:
   ```powershell
   npm install datatables.net-bs5
   ```

2. **Import in JavaScript** (`ClientApp/src/main.js` or component file):
   ```javascript
   import DataTable from 'datatables.net-bs5';
   
   // Initialize DataTable
   new DataTable('#myTable');
   ```

3. **Import styles** (if needed in `ClientApp/src/site.css`):
   ```css
   @import 'datatables.net-bs5/css/dataTables.bootstrap5.css';
   ```

4. **Rebuild**:
   ```powershell
   npm run build
   ```

Vite will automatically:
- Tree-shake unused code
- Bundle dependencies
- Generate optimized output with cache-busting hashes

## Asset Loading in Views

The application uses a custom `ViteAssetTagHelper` to load assets:

```html
@* In _Layout.cshtml *@
<vite-style entry="site.css" />
<vite-script entry="main.js" />
```

This:
1. Reads `wwwroot/dist/.vite/manifest.json`
2. Resolves `site.css` → `css/site.[hash].css`
3. Generates proper `<link>` or `<script>` tags

**Fallback**: If manifest doesn't exist (development before first build), falls back to `wwwroot/lib/`.

## Production Deployment

### Build for Production

```powershell
dotnet publish -c Release -o ./publish
```

This automatically:
1. Runs `npm install` (if needed)
2. Runs `npm run build` with production optimizations
3. Publishes ASP.NET Core app with `wwwroot/dist/` included

### Azure App Service / IIS

The `wwwroot/dist/` folder is included in the publish output. No special configuration needed.

### Docker

Ensure your Dockerfile includes Node.js:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy and restore
COPY ["WebSpark.HttpClientUtility.Web/package.json", "WebSpark.HttpClientUtility.Web/"]
RUN cd WebSpark.HttpClientUtility.Web && npm install

# Build
COPY . .
RUN dotnet publish -c Release -o /app/publish
```

## Performance Benefits

Compared to LibMan approach:

| Metric | LibMan (Old) | Vite (New) | Improvement |
|--------|--------------|------------|-------------|
| Bootstrap JS | 59 KB | 22 KB | **63% smaller** (tree-shaking) |
| Bootstrap CSS | 160 KB | ~80 KB | **50% smaller** (unused CSS removed) |
| jQuery | 88 KB | 0 KB | **Removed** (Bootstrap 5 doesn't need it) |
| Total Load | 307 KB | 102 KB | **67% reduction** |
| Build Time | N/A | ~2-5s | Cached on rebuild |

## Troubleshooting

### Build Fails with "npm: command not found"

**Solution**: Install Node.js from https://nodejs.org/

### Assets Not Loading (404 errors)

1. Check if `wwwroot/dist/` exists:
   ```powershell
   ls WebSpark.HttpClientUtility.Web/wwwroot/dist
   ```

2. Manually run build:
   ```powershell
   cd WebSpark.HttpClientUtility.Web
   npm run build
   ```

3. Check manifest file exists:
   ```powershell
   cat wwwroot/dist/.vite/manifest.json
   ```

### Old Bootstrap/jQuery Still Loading

The app falls back to `wwwroot/lib/` if Vite hasn't built yet. Solution:

1. Build Vite assets:
   ```powershell
   npm run build
   ```

2. Restart the application

### Clean Build Issues

```powershell
# Clean Vite artifacts
npm run clean

# Clean ASP.NET Core artifacts
dotnet clean

# Deep clean (removes node_modules)
dotnet clean -p:DeepClean=true

# Rebuild everything
npm install
npm run build
dotnet build
```

## CI/CD Pipeline Updates

For GitHub Actions, Azure DevOps, etc., add Node.js setup:

```yaml
# GitHub Actions example
- name: Setup Node.js
  uses: actions/setup-node@v4
  with:
    node-version: '20'
    cache: 'npm'
    cache-dependency-path: 'WebSpark.HttpClientUtility.Web/package-lock.json'

- name: Install NPM dependencies
  run: npm ci
  working-directory: WebSpark.HttpClientUtility.Web

- name: Build Vite assets
  run: npm run build
  working-directory: WebSpark.HttpClientUtility.Web

- name: Build .NET
  run: dotnet build --configuration Release
```

## Migration from LibMan

The old `wwwroot/lib/` folder is kept as a **fallback** for development. Once Vite builds successfully:

1. ✅ New users: Automatically use Vite (recommended)
2. ✅ Old users: Gradual migration (app still works without Vite)
3. ⚠️ **Do not delete** `wwwroot/lib/` until you verify all pages load correctly with Vite

To verify migration:

```powershell
# Build Vite assets
npm run build

# Check manifest exists
ls wwwroot/dist/.vite/manifest.json

# Test the app
dotnet run

# Open browser and verify Bootstrap/icons load from /dist/
# (Check DevTools Network tab - files should be /dist/css/*.css, /dist/js/*.js)
```

## References

- [Vite Documentation](https://vitejs.dev/)
- [Bootstrap 5 Documentation](https://getbootstrap.com/)
- [NPM Package Manager](https://www.npmjs.com/)
- [ASP.NET Core Static Files](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files)

## Questions?

If you encounter issues or have suggestions:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review Vite logs: `npm run build -- --debug`
3. Open an issue on GitHub: https://github.com/markhazleton/WebSpark.HttpClientUtility/issues
