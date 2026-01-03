# NPM Build Pipeline Setup - Quick Start

## Initial Setup (First Time Only)

After cloning the repository, run these commands to set up the NPM build pipeline:

```powershell
# Navigate to the Web project
cd WebSpark.HttpClientUtility.Web

# Install NPM dependencies
npm install

# Build client-side assets with Vite
npm run build

# Return to solution root
cd ..

# Build the entire solution
dotnet build
```

## What Gets Created

The setup creates the following structure:

```
WebSpark.HttpClientUtility.Web/
├── node_modules/          # NPM packages (Bootstrap, Vite, etc.) - NOT committed
├── wwwroot/
│   └── dist/              # Vite build output - NOT committed
│       ├── .vite/
│       │   └── manifest.json  # Asset mapping
│       ├── css/
│       │   └── site.[hash].css
│       └── js/
│           └── main.[hash].js
└── package-lock.json      # Dependency lock file - committed for reproducibility
```

## Daily Development Workflow

### Option 1: Standard Build (Recommended)

Just build normally - NPM integration is automatic:

```powershell
dotnet build
# or
dotnet run --project WebSpark.HttpClientUtility.Web
```

The `.csproj` MSBuild integration will automatically run `npm run build` if `node_modules` exists.

### Option 2: Disable NPM Build Temporarily

If you're working only on C# code and want faster builds:

```powershell
dotnet build -p:SkipNpmBuild=true
```

**Note**: The app will fallback to old `wwwroot/lib/` files when `wwwroot/dist/` doesn't exist.

## Troubleshooting

### "npm: command not found"

Install Node.js 18+ from https://nodejs.org/

### Build Fails with "The command 'npm.cmd run build' exited with code 1"

Run the initial setup commands above. This usually means `node_modules` doesn't exist.

### Assets Not Loading (404 errors)

Manually build the Vite assets:

```powershell
cd WebSpark.HttpClientUtility.Web
npm run build
```

## Clean Build

```powershell
# Clean .NET artifacts
dotnet clean

# Clean Vite artifacts
cd WebSpark.HttpClientUtility.Web
npm run clean

# Deep clean (removes node_modules)
dotnet clean -p:DeepClean=true
```

## CI/CD Setup

GitHub Actions / Azure DevOps pipelines need Node.js installed:

```yaml
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

## More Information

See [BUILD-PIPELINE.md](BUILD-PIPELINE.md) for comprehensive documentation including:
- Architecture details
- Development workflows with HMR
- Adding new client-side dependencies
- Performance benefits
- Migration from LibMan

## Summary

**TL;DR**: Run `npm install && npm run build` in the Web project directory once, then use normal `dotnet build` commands.
