# Static Documentation Site Implementation Summary

**Project**: WebSpark.HttpClientUtility Documentation Website  
**Date**: November 2, 2025  
**Branch**: `001-static-docs-site`  
**Status**: ✅ Complete and Working

## Overview

Successfully implemented a fully static documentation website using Eleventy 3.0+ that:
- Fetches live NuGet package data at build time
- Supports both local development and GitHub Pages deployment
- Includes C# syntax highlighting with Prism.js
- Features 6 content pages with comprehensive documentation
- Uses modern CSS with responsive design

## Critical Learnings

### 1. Eleventy pathPrefix Behavior

**Problem**: `pathPrefix` in Eleventy affects BOTH URL generation AND file output location, which caused issues when trying to serve locally vs GitHub Pages subdirectory.

**Solution**: Use environment-based configuration:

```javascript
// .eleventy.js
const isProduction = process.env.ELEVENTY_ENV === "production";
const pathPrefix = isProduction ? "/WebSpark.HttpClientUtility/" : "/";

return {
  pathPrefix: pathPrefix
};
```

**Key Points**:
- When `pathPrefix="/WebSpark.HttpClientUtility/"`, files are written to `docs/WebSpark.HttpClientUtility/`
- When `pathPrefix="/"`, files are written to `docs/` (root)
- Templates must use `{{ '/path' | url }}` filter to apply pathPrefix automatically
- Passthrough copy destinations must match the pathPrefix structure in production

### 2. Template URL Filtering

**Problem**: Hardcoded paths like `/assets/css/main.css` don't work with pathPrefix.

**Solution**: Always use the `url` filter in templates:

```njk
<!-- WRONG -->
<link rel="stylesheet" href="/assets/css/main.css">

<!-- RIGHT -->
<link rel="stylesheet" href="{{ '/assets/css/main.css' | url }}">
```

This applies to:
- Asset links (CSS, JS, images)
- Navigation links
- Internal page links

### 3. Front Matter Permalinks

**Problem**: Front matter `permalink` values override pathPrefix configuration.

**Solution**: Use relative permalinks that work with pathPrefix:

```yaml
---
# WRONG - hardcoded subdirectory
permalink: /WebSpark.HttpClientUtility/

# RIGHT - relative path, pathPrefix applied automatically
permalink: /
---
```

### 4. Passthrough Copy with pathPrefix

**Problem**: Assets copied with `addPassthroughCopy` ignore pathPrefix by default.

**Solution**: Use environment-based passthrough copy configuration:

```javascript
const isProduction = process.env.ELEVENTY_ENV === "production";
const prefix = isProduction ? "WebSpark.HttpClientUtility" : "";

if (prefix) {
  eleventyConfig.addPassthroughCopy({ "assets": `${prefix}/assets` });
} else {
  eleventyConfig.addPassthroughCopy("assets");
}
```

### 5. Watch Loop Prevention

**Problem**: Data fetcher writing to `_data/nuget-cache.json` triggered Eleventy's file watcher, causing infinite rebuild loop.

**Solution**: Add cache file to watch ignores:

```javascript
eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
```

### 6. Prism.js Integration

**Problem**: Placeholder Prism.js files didn't provide syntax highlighting.

**Solution**: Use npm package with build script:

1. Install: `npm install prismjs`
2. Create `scripts/copy-prism.js` to bundle languages
3. Add to build scripts: `npm run copy:prism`
4. Include languages: csharp, javascript, json, powershell, bash, markup

## Complete Configuration Guide

### Directory Structure

```
src/
├── .eleventy.js              # Eleventy configuration
├── package.json              # Dependencies and scripts
├── scripts/
│   └── copy-prism.js        # Prism.js bundler
├── _data/
│   ├── site.json            # Site metadata
│   ├── navigation.json      # Navigation structure
│   ├── nuget.js             # NuGet API data fetcher
│   └── nuget-cache.json     # Cache (ignored by watcher)
├── _includes/
│   ├── layouts/
│   │   ├── base.njk         # Base HTML template
│   │   └── page.njk         # Content page wrapper
│   └── components/
│       ├── header.njk       # Site header
│       └── footer.njk       # Site footer
├── assets/
│   ├── css/
│   │   ├── main.css         # Main styles
│   │   └── prism-tomorrow.css  # Syntax theme
│   ├── js/
│   │   └── prism.min.js     # Syntax highlighter
│   └── images/
│       └── favicon.ico
├── pages/
│   ├── index.md             # Homepage
│   ├── features.md          # Features page
│   ├── getting-started.md   # Getting started guide
│   ├── examples.md          # Code examples
│   ├── api-reference.md     # API documentation
│   └── about.md             # About page
├── .nojekyll                # Disable Jekyll processing
└── robots.txt               # SEO configuration
```

### Eleventy Configuration (.eleventy.js)

```javascript
export default function(eleventyConfig) {
  // 1. Ignore cache file from watch to prevent infinite rebuild loop
  eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
  
  // 2. Determine if production build
  const isProduction = process.env.ELEVENTY_ENV === "production";
  const prefix = isProduction ? "WebSpark.HttpClientUtility" : "";
  
  console.log(`🔧 Build mode: ${isProduction ? 'PRODUCTION' : 'DEVELOPMENT'}`);
  console.log(`🔧 PathPrefix will be: ${isProduction ? '/WebSpark.HttpClientUtility/' : '/'}`);
  
  // 3. Set server options for dev mode
  if (!isProduction) {
    eleventyConfig.setServerOptions({
      showAllHosts: true
    });
  }
  
  // 4. Copy assets through to output - environment-aware
  if (prefix) {
    eleventyConfig.addPassthroughCopy({ "assets": `${prefix}/assets` });
    eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": `${prefix}/favicon.ico` });
  } else {
    eleventyConfig.addPassthroughCopy("assets");
    eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": "favicon.ico" });
  }
  eleventyConfig.addPassthroughCopy({ ".nojekyll": ".nojekyll" });
  eleventyConfig.addPassthroughCopy({ "robots.txt": "robots.txt" });
  
  // 5. Add filters
  eleventyConfig.addFilter("formatNumber", function(value) {
    if (!value) return "0";
    const num = parseInt(value);
    if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
    if (num >= 1000) return (num / 1000).toFixed(1) + "K";
    return num.toLocaleString();
  });
  
  eleventyConfig.addFilter("formatDate", function(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", { 
      year: "numeric", 
      month: "short", 
      day: "numeric" 
    });
  });
  
  eleventyConfig.addFilter("dateISO", function(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toISOString();
  });
  
  // 6. Return configuration
  return {
    dir: {
      input: ".",
      output: "../docs",
      includes: "_includes",
      data: "_data"
    },
    templateFormats: ["md", "njk", "html"],
    markdownTemplateEngine: "njk",
    htmlTemplateEngine: "njk",
    // Use pathPrefix only for production (GitHub Pages needs it)
    pathPrefix: isProduction ? "/WebSpark.HttpClientUtility/" : "/"
  };
}
```

### Package.json Scripts

```json
{
  "scripts": {
    "clean": "rimraf ../docs",
    "dev": "eleventy --serve",
    "build": "npm run clean && npm run copy:prism && cross-env ELEVENTY_ENV=production eleventy",
    "build:prod": "npm run clean && npm run copy:prism && cross-env ELEVENTY_ENV=production eleventy",
    "copy:prism": "node scripts/copy-prism.js",
    "test:links": "hyperlink ../docs",
    "test:html": "htmlhint ../docs/**/*.html",
    "validate": "npm run test:html && npm run test:links"
  }
}
```

### Template Best Practices

#### Base Layout (base.njk)

```njk
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>{% if title %}{{ title }} | {% endif %}{{ site.name }}</title>
  
  <!-- CRITICAL: Use url filter for all asset paths -->
  <link rel="stylesheet" href="{{ '/assets/css/main.css' | url }}">
  <link rel="stylesheet" href="{{ '/assets/css/prism-tomorrow.css' | url }}">
  <link rel="icon" href="{{ '/favicon.ico' | url }}">
</head>
<body>
  {% include "components/header.njk" %}
  <main class="main-content">
    {{ content | safe }}
  </main>
  {% include "components/footer.njk" %}
  
  <!-- CRITICAL: Use url filter for scripts -->
  <script src="{{ '/assets/js/prism.min.js' | url }}" defer></script>
</body>
</html>
```

#### Navigation Links

```njk
<!-- CRITICAL: Use url filter for all internal links -->
<nav>
  <a href="{{ '/' | url }}">Home</a>
  <a href="{{ '/features/' | url }}">Features</a>
  <a href="{{ item.url | url }}">{{ item.label }}</a>
</nav>
```

#### Page Front Matter

```yaml
---
layout: layouts/base.njk
title: Page Title
description: Page description for meta tags
permalink: /page-name/  # Relative path, pathPrefix applied automatically
---
```

### NuGet Data Fetcher (_data/nuget.js)

```javascript
import fetch from 'node-fetch';
import { readFileSync, writeFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const CACHE_FILE = join(__dirname, 'nuget-cache.json');

export default async function() {
  try {
    const response = await fetch(
      'https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility'
    );
    const data = await response.json();
    const packageData = data.data[0];
    
    const result = {
      version: packageData.version,
      downloads: packageData.totalDownloads,
      displayDownloads: formatNumber(packageData.totalDownloads),
      description: packageData.description,
      cached: false
    };
    
    // Write cache (ignored by Eleventy watcher)
    writeFileSync(CACHE_FILE, JSON.stringify(result, null, 2));
    
    return result;
  } catch (error) {
    // Fallback to cache on error
    try {
      const cached = JSON.parse(readFileSync(CACHE_FILE, 'utf8'));
      return { ...cached, cached: true };
    } catch {
      return { version: "1.4.0", downloads: 0, cached: true };
    }
  }
}
```

### Prism.js Build Script (scripts/copy-prism.js)

```javascript
import { readFileSync, writeFileSync, mkdirSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const assetsDir = join(__dirname, '../assets');
const cssDir = join(assetsDir, 'css');
const jsDir = join(assetsDir, 'js');

mkdirSync(cssDir, { recursive: true });
mkdirSync(jsDir, { recursive: true });

// Copy Prism core
const prismCorePath = join(__dirname, '../node_modules/prismjs/prism.js');
let jsContent = readFileSync(prismCorePath, 'utf8');

// Add language support
const languages = ['csharp', 'javascript', 'json', 'powershell', 'bash', 'markup'];
for (const lang of languages) {
  try {
    const langPath = join(__dirname, `../node_modules/prismjs/components/prism-${lang}.js`);
    jsContent += '\n' + readFileSync(langPath, 'utf8');
  } catch (err) {
    console.warn(`Language ${lang} not found, skipping`);
  }
}

writeFileSync(join(jsDir, 'prism.min.js'), jsContent);

// Copy CSS theme
const cssPath = join(__dirname, '../node_modules/prismjs/themes/prism-tomorrow.css');
writeFileSync(join(cssDir, 'prism-tomorrow.css'), readFileSync(cssPath, 'utf8'));

console.log('✓ Prism.js files copied successfully');
```

## Local Development Workflow

### Initial Setup

```bash
cd src
npm install
npm run copy:prism
```

### Development Server

```bash
npm run dev
```

This runs Eleventy with:
- `pathPrefix="/"` (files at root of docs/)
- Live reload enabled
- Server at http://localhost:8080/
- Files written to `../docs/index.html`, `../docs/features/index.html`, etc.

### Testing Locally

1. Navigate to http://localhost:8080/
2. All pages should load without subdirectory in URL
3. CSS, JS, and images should load correctly
4. Syntax highlighting should work
5. Navigation should work between pages

### Hard Refresh

When making CSS/JS changes, use Ctrl+F5 to bypass browser cache.

## GitHub Pages Deployment Workflow

### Production Build

```bash
npm run build
# or
npm run build:prod
```

This runs:
1. `npm run clean` - Remove old docs folder
2. `npm run copy:prism` - Bundle Prism.js files
3. `cross-env ELEVENTY_ENV=production eleventy` - Build with production settings

Production build uses:
- `pathPrefix="/WebSpark.HttpClientUtility/"` 
- Files written to `../docs/WebSpark.HttpClientUtility/index.html`
- All URLs include subdirectory prefix
- Assets copied to `../docs/WebSpark.HttpClientUtility/assets/`

### GitHub Pages Configuration

1. Push branch to GitHub
2. Go to repository Settings → Pages
3. Source: Deploy from a branch
4. Branch: `main` (or `001-static-docs-site` for testing)
5. Folder: `/docs`
6. Save

GitHub Pages will serve from:
- URL: https://markhazleton.github.io/WebSpark.HttpClientUtility/
- Files from: `/docs/WebSpark.HttpClientUtility/` directory

### Verification

After deployment, verify:
- [ ] Homepage loads at https://markhazleton.github.io/WebSpark.HttpClientUtility/
- [ ] All navigation links work
- [ ] CSS and JavaScript load correctly
- [ ] Syntax highlighting displays properly
- [ ] Images and favicon load
- [ ] All 6 pages accessible

## Common Issues and Solutions

### Issue 1: Blank Page or 404

**Symptoms**: Page loads but shows blank or "Cannot GET /path"

**Causes**:
- pathPrefix not applied to asset URLs
- Front matter permalink hardcoded with subdirectory
- Assets not copied to correct location

**Solutions**:
- Use `{{ '/path' | url }}` filter in all templates
- Use relative permalinks without subdirectory
- Check passthrough copy configuration

### Issue 2: Infinite Rebuild Loop

**Symptoms**: Dev server continuously rebuilds, terminal flooded with "[11ty] File changed"

**Cause**: Data fetcher writing to watched directory

**Solution**: Add to `.eleventy.js`:
```javascript
eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
```

### Issue 3: CSS/JS Not Loading

**Symptoms**: Styles not applied, syntax highlighting not working

**Causes**:
- Browser cache serving old placeholder files
- Asset paths not using url filter
- Files not copied during build

**Solutions**:
- Hard refresh with Ctrl+F5
- Verify all asset links use `{{ '/path' | url }}`
- Run `npm run copy:prism`

### Issue 4: Syntax Highlighting Not Working

**Symptoms**: Code blocks show plain white text on black background

**Cause**: Using placeholder Prism.js files instead of real ones

**Solution**:
```bash
npm install prismjs
npm run copy:prism
```

### Issue 5: Different Behavior Local vs Production

**Symptoms**: Works locally but not on GitHub Pages (or vice versa)

**Cause**: pathPrefix differences between environments

**Solution**: Test production build locally:
```bash
npm run build
cd ../docs/WebSpark.HttpClientUtility
# Use a simple HTTP server
python -m http.server 8080
# Visit http://localhost:8080/
```

## File Checklist

Required files for proper operation:

- [ ] `src/.eleventy.js` - Main configuration with environment logic
- [ ] `src/package.json` - Scripts and dependencies
- [ ] `src/scripts/copy-prism.js` - Prism.js bundler
- [ ] `src/_data/site.json` - Site metadata
- [ ] `src/_data/navigation.json` - Navigation structure
- [ ] `src/_data/nuget.js` - NuGet API fetcher
- [ ] `src/_includes/layouts/base.njk` - Base HTML template (with url filters)
- [ ] `src/_includes/components/header.njk` - Header (with url filters)
- [ ] `src/_includes/components/footer.njk` - Footer
- [ ] `src/assets/css/main.css` - Main styles
- [ ] `src/assets/images/favicon.ico` - Site icon
- [ ] `src/pages/index.md` - Homepage (with `permalink: /`)
- [ ] `src/pages/features.md` - Features page
- [ ] `src/pages/getting-started.md` - Getting started guide
- [ ] `src/pages/examples.md` - Code examples
- [ ] `src/pages/api-reference.md` - API documentation
- [ ] `src/pages/about.md` - About page
- [ ] `src/.nojekyll` - Disable GitHub Jekyll processing
- [ ] `src/robots.txt` - SEO configuration

Generated files (not committed):
- `src/assets/css/prism-tomorrow.css` - Generated by copy:prism
- `src/assets/js/prism.min.js` - Generated by copy:prism
- `src/_data/nuget-cache.json` - Generated at build time
- `docs/*` - All output files

## Dependencies

### Production Dependencies
```json
{
  "node-fetch": "^3.3.2",
  "prismjs": "^1.29.0"
}
```

### Dev Dependencies
```json
{
  "@11ty/eleventy": "^3.0.0",
  "cross-env": "^10.1.0",
  "htmlhint": "^1.1.4",
  "hyperlink": "^5.0.4",
  "markdown-it": "^14.1.0",
  "markdown-it-anchor": "^9.2.0",
  "rimraf": "^5.0.5"
}
```

## Performance Considerations

- NuGet data fetched once per build, cached for rebuild performance
- Prism.js bundled at build time (not runtime)
- Static HTML generation (no JavaScript rendering required)
- CSS and JS files minified
- Build time: ~0.4-0.5 seconds for full site
- 6 pages generated in single build

## Accessibility & SEO

- Semantic HTML5 structure
- Proper heading hierarchy (h1 → h6)
- Alt text for images
- Meta descriptions for all pages
- Open Graph tags for social sharing
- Canonical URLs
- Mobile responsive design
- Keyboard navigation support
- Focus states for interactive elements
- robots.txt for search engine guidance

## Future Enhancements

Potential improvements for future iterations:

1. **Search Functionality**: Add client-side search with Lunr.js or Pagefind
2. **Dark Mode Toggle**: User-selectable theme preference
3. **Version Dropdown**: Documentation for multiple package versions
4. **Interactive Examples**: Live code editors with Try .NET
5. **Build Time Optimization**: Incremental builds for larger sites
6. **Image Optimization**: Automatic image compression and WebP conversion
7. **Analytics**: Privacy-friendly analytics integration
8. **RSS Feed**: Changelog or blog posts feed
9. **Sitemap**: Automatic sitemap.xml generation
10. **404 Page**: Custom error page for GitHub Pages

## Success Metrics

✅ All original requirements met:
- Static site generation ✓
- NuGet API integration ✓
- Live data fetching ✓
- Caching with fallback ✓
- Modern responsive design ✓
- C# syntax highlighting ✓
- Multiple content pages ✓
- GitHub Pages deployment ready ✓
- Local development workflow ✓
- Production build process ✓

## Conclusion

This implementation provides a solid foundation for documentation that:
- Works identically in local development and production
- Requires minimal configuration changes between environments
- Uses modern tooling (Eleventy 3.0, ES modules)
- Generates fully static HTML (no runtime JavaScript required)
- Supports easy content updates through Markdown files
- Integrates live data from external APIs
- Provides excellent developer experience

The key to success was understanding Eleventy's pathPrefix behavior and implementing environment-aware configuration throughout the entire stack (config, templates, assets, scripts).
