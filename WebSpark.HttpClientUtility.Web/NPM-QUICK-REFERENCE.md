# NPM Build Process - Quick Reference

## Installation

```bash
cd WebSpark.HttpClientUtility.Web
npm install
```

## Common Commands

| Command | Description |
|---------|-------------|
| `npm run dev` | Start dev server (localhost:5173) |
| `npm run build` | Clean + Lint + Build for production |
| `npm run build:prod` | Full build with tests |
| `npm run clean` | Remove build output |
| `npm run lint` | Run all linters (JS + CSS) |
| `npm run lint:fix` | Auto-fix linting issues |
| `npm run format` | Format code with Prettier |
| `npm run preview` | Preview production build |

## Pre-Commit Checklist

1. ✅ `npm run lint` - No errors or warnings
2. ✅ `npm run format:check` - Code is formatted
3. ✅ `npm run build` - Production build succeeds

## Build Output

```
wwwroot/dist/
├── .vite/
│   └── manifest.json       # Asset manifest
├── js/
│   ├── main.[hash].js      # Application bundle
│   └── vendor.[hash].js    # Third-party libraries
├── css/
│   └── site.[hash].css     # Compiled styles
└── assets/
    └── [files with hashes] # Images, fonts, etc.
```

## Troubleshooting

### "Module not found" errors
```bash
npm install
```

### ESLint errors
```bash
npm run lint:js:fix
```

### Stylelint errors
```bash
npm run lint:css:fix
```

### Build not reflecting changes
```bash
npm run clean
npm run build
```

## IDE Integration

### Visual Studio Code

Install these extensions:
- ESLint (dbaeumer.vscode-eslint)
- Stylelint (stylelint.vscode-stylelint)
- Prettier (esbenp.prettier-vscode)

Add to `.vscode/settings.json`:
```json
{
  "editor.defaultFormatter": "esbenp.prettier-vscode",
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": true,
    "source.fixAll.stylelint": true
  }
}
```

### Visual Studio 2022

Extensions:
- NPM Task Runner
- ESLint Language Service
- Prettier - Code formatter

## Quality Standards

- **Zero warnings policy** - Build fails with any ESLint warnings
- **Consistent formatting** - All code formatted with Prettier
- **Modern JavaScript** - ES2022+ features, no `var`, prefer `const`
- **Clean builds** - Always clean before production builds

## Need Help?

See [NPM-BUILD-README.md](NPM-BUILD-README.md) for detailed documentation.
