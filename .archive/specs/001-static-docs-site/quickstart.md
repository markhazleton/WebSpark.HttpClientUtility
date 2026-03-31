# Quick Start Implementation Guide

**Feature**: Static Documentation Website for GitHub Pages  
**Date**: 2025-11-02  
**Audience**: Developers implementing the feature

## Overview

This guide provides a step-by-step walkthrough for implementing the static documentation website using Eleventy. Follow these steps in order to build and deploy the site.

---

## Prerequisites

**Required Software**:
- Node.js 20.x LTS or later
- npm 10.x or later
- Git
- Code editor (VS Code recommended)

**Verify Installation**:
```bash
node --version   # Should be v20.x.x or higher
npm --version    # Should be 10.x.x or higher
git --version    # Any recent version
```

---

## Phase 1: Project Setup (30 minutes)

### Step 1: Create Directory Structure

From repository root:

```bash
# Create src directory and subdirectories
mkdir -p src/_data
mkdir -p src/_includes/layouts
mkdir -p src/_includes/components
mkdir -p src/_includes/partials
mkdir -p src/assets/css
mkdir -p src/assets/js
mkdir -p src/assets/images
mkdir -p src/pages
mkdir -p src/pages/features
mkdir -p src/pages/examples
mkdir -p src/pages/api-reference
mkdir -p src/pages/about

# Create empty docs directory (will be populated by build)
mkdir -p docs
```

### Step 2: Initialize NPM Project

```bash
cd src

# Initialize package.json
npm init -y

# Install dependencies
npm install --save-dev @11ty/eleventy@^3.0.0
npm install --save-dev rimraf@^5.0.5
npm install --save-dev markdown-it@^13.0.0
npm install --save-dev markdown-it-anchor@^8.6.0
npm install --save-dev htmlhint@^1.1.4
npm install --save-dev hyperlink@^5.0.4

# Install production dependencies
npm install node-fetch@^3.3.2
```

### Step 3: Configure package.json

Edit `src/package.json` to add scripts (see [package-json.md](contracts/package-json.md) for full contract):

```json
{
  "name": "webspark-httpclientutility-docs",
  "version": "1.4.0",
  "private": true,
  "scripts": {
    "clean": "rimraf ../docs",
    "dev": "eleventy --serve --incremental",
    "build": "npm run clean && eleventy",
    "test:links": "hyperlink ../docs",
    "test:html": "htmlhint ../docs/**/*.html",
    "validate": "npm run test:html && npm run test:links"
  },
  "engines": {
    "node": ">=20.0.0",
    "npm": ">=10.0.0"
  }
}
```

### Step 4: Create Eleventy Configuration

Create `src/.eleventy.js` (see [eleventy-config.md](contracts/eleventy-config.md) for full contract):

```javascript
module.exports = function(eleventyConfig) {
  // Copy assets
  eleventyConfig.addPassthroughCopy("assets");
  eleventyConfig.addPassthroughCopy({ ".nojekyll": ".nojekyll" });
  
  // Add filters
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
  
  // Return configuration
  return {
    dir: {
      input: ".",
      output: "../docs",
      includes: "_includes",
      data: "_data"
    },
    templateFormats: ["md", "njk", "html"],
    markdownTemplateEngine: "njk",
    htmlTemplateEngine: "njk"
  };
};
```

### Step 5: Create Essential Files

**Create `.nojekyll`** (disables Jekyll on GitHub Pages):
```bash
touch src/.nojekyll
```

**Create `robots.txt`**:
```txt
# src/robots.txt
User-agent: *
Allow: /

Sitemap: https://markhazleton.github.io/WebSpark.HttpClientUtility/sitemap.xml
```

---

## Phase 2: Global Data Files (20 minutes)

### Step 1: Create Site Metadata

Create `src/_data/site.json`:

```json
{
  "name": "WebSpark.HttpClientUtility",
  "title": "WebSpark.HttpClientUtility - Production-Ready HttpClient Wrapper",
  "description": "A comprehensive HttpClient wrapper for .NET 8+ with resilience, caching, telemetry, and web crawling capabilities.",
  "url": "https://markhazleton.github.io/WebSpark.HttpClientUtility",
  "repositoryUrl": "https://github.com/markhazleton/WebSpark.HttpClientUtility",
  "nugetUrl": "https://www.nuget.org/packages/WebSpark.HttpClientUtility",
  "issueTrackerUrl": "https://github.com/markhazleton/WebSpark.HttpClientUtility/issues",
  "author": {
    "name": "Mark Hazleton",
    "url": "https://github.com/markhazleton"
  }
}
```

### Step 2: Create NuGet Data Fetcher

Create `src/_data/nuget.js`:

```javascript
const fetch = require('node-fetch');
const fs = require('fs');
const path = require('path');

const CACHE_FILE = path.join(__dirname, 'nuget-cache.json');
const API_URL = 'https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility';

function formatNumber(value) {
  if (!value) return "0";
  const num = parseInt(value);
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toLocaleString();
}

module.exports = async function() {
  try {
    console.log('Fetching NuGet package data...');
    const response = await fetch(API_URL);
    const data = await response.json();
    
    if (data.data && data.data.length > 0) {
      const packageData = data.data[0];
      
      // Cache successful response
      fs.writeFileSync(CACHE_FILE, JSON.stringify(packageData, null, 2));
      console.log('✓ NuGet data fetched successfully');
      
      return {
        version: packageData.version,
        downloads: packageData.totalDownloads,
        displayDownloads: formatNumber(packageData.totalDownloads),
        description: packageData.description,
        projectUrl: packageData.projectUrl,
        lastUpdate: new Date().toISOString(),
        cached: false
      };
    }
  } catch (error) {
    console.warn('⚠ NuGet API failed, using cached data:', error.message);
    
    if (fs.existsSync(CACHE_FILE)) {
      const cached = JSON.parse(fs.readFileSync(CACHE_FILE, 'utf8'));
      return {
        version: cached.version,
        downloads: cached.totalDownloads,
        displayDownloads: formatNumber(cached.totalDownloads),
        description: cached.description,
        projectUrl: cached.projectUrl,
        lastUpdate: cached.cachedAt || 'Unknown',
        cached: true
      };
    }
    
    // Ultimate fallback
    return {
      version: "1.4.0",
      downloads: 0,
      displayDownloads: "0",
      description: "A production-ready HttpClient wrapper",
      cached: true,
      error: true
    };
  }
};
```

### Step 3: Create Navigation Data

Create `src/_data/navigation.json`:

```json
{
  "main": [
    { "label": "Home", "url": "/", "order": 1 },
    { "label": "Features", "url": "/features/", "order": 2 },
    { "label": "Getting Started", "url": "/getting-started/", "order": 3 },
    { "label": "API Reference", "url": "/api-reference/", "order": 4 },
    { "label": "Examples", "url": "/examples/", "order": 5 },
    { "label": "About", "url": "/about/", "order": 6 }
  ],
  "footer": [
    { "label": "GitHub", "url": "https://github.com/markhazleton/WebSpark.HttpClientUtility", "external": true },
    { "label": "NuGet", "url": "https://www.nuget.org/packages/WebSpark.HttpClientUtility", "external": true },
    { "label": "Issues", "url": "https://github.com/markhazleton/WebSpark.HttpClientUtility/issues", "external": true }
  ]
}
```

---

## Phase 3: Base Templates (45 minutes)

### Step 1: Create Base Layout

Create `src/_includes/layouts/base.njk`:

```nunjucks
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>{{ title }} | {{ site.name }}</title>
  <meta name="description" content="{{ description }}">
  
  <!-- Open Graph -->
  <meta property="og:title" content="{{ title }}">
  <meta property="og:description" content="{{ description }}">
  <meta property="og:url" content="{{ site.url }}{{ page.url }}">
  <meta property="og:type" content="website">
  
  <!-- Canonical -->
  <link rel="canonical" href="{{ site.url }}{{ page.url }}">
  
  <!-- Stylesheets -->
  <link rel="stylesheet" href="/assets/css/main.css">
  <link rel="stylesheet" href="/assets/css/prism-tomorrow.css">
  
  <!-- Favicon -->
  <link rel="icon" href="/favicon.ico">
</head>
<body>
  {% include "components/header.njk" %}
  
  <main class="main-content">
    {{ content | safe }}
  </main>
  
  {% include "components/footer.njk" %}
  
  <!-- Scripts -->
  <script src="/assets/js/prism.min.js"></script>
</body>
</html>
```

### Step 2: Create Page Layout

Create `src/_includes/layouts/page.njk`:

```nunjucks
---
layout: layouts/base.njk
---
<article class="page">
  <header class="page-header">
    <h1>{{ title }}</h1>
    {% if description %}
    <p class="page-description">{{ description }}</p>
    {% endif %}
  </header>
  
  <div class="page-content">
    {{ content | safe }}
  </div>
</article>
```

### Step 3: Create Header Component

Create `src/_includes/components/header.njk`:

```nunjucks
<header class="site-header">
  <div class="container">
    <div class="site-logo">
      <a href="/">{{ site.name }}</a>
    </div>
    
    <nav class="main-nav">
      <input type="checkbox" id="nav-toggle" class="nav-toggle" aria-label="Toggle navigation">
      <label for="nav-toggle" class="nav-toggle-label">
        <span></span>
        <span></span>
        <span></span>
      </label>
      
      <ul class="nav-menu">
        {% for item in navigation.main %}
        <li class="nav-item{% if page.url == item.url %} active{% endif %}">
          <a href="{{ item.url }}">{{ item.label }}</a>
        </li>
        {% endfor %}
      </ul>
    </nav>
  </div>
</header>
```

### Step 4: Create Footer Component

Create `src/_includes/components/footer.njk`:

```nunjucks
<footer class="site-footer">
  <div class="container">
    <div class="footer-content">
      <div class="footer-section">
        <h3>{{ site.name }}</h3>
        <p>Version {{ nuget.version }}</p>
        {% if nuget.cached %}
        <p class="cache-notice">Data cached: {{ nuget.lastUpdate | formatDate }}</p>
        {% endif %}
      </div>
      
      <div class="footer-section">
        <h3>Links</h3>
        <ul>
          {% for item in navigation.footer %}
          <li>
            <a href="{{ item.url }}"{% if item.external %} target="_blank" rel="noopener noreferrer"{% endif %}>
              {{ item.label }}
            </a>
          </li>
          {% endfor %}
        </ul>
      </div>
      
      <div class="footer-section">
        <p>&copy; {{ site.build.timestamp | date('Y') }} {{ site.author.name }}</p>
        <p>Licensed under MIT</p>
      </div>
    </div>
  </div>
</footer>
```

---

## Phase 4: Basic Styling (30 minutes)

### Step 1: Create Main Stylesheet

Create `src/assets/css/main.css`:

```css
/* CSS Custom Properties */
:root {
  --color-primary: #0066cc;
  --color-secondary: #555;
  --color-background: #ffffff;
  --color-text: #333;
  --color-border: #e0e0e0;
  
  --space-sm: 1rem;
  --space-md: 1.5rem;
  --space-lg: 2rem;
  
  --font-body: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  --max-width: 1200px;
}

/* Reset */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

/* Base Styles */
body {
  font-family: var(--font-body);
  line-height: 1.6;
  color: var(--color-text);
  background: var(--color-background);
}

.container {
  max-width: var(--max-width);
  margin: 0 auto;
  padding: 0 var(--space-md);
}

/* Header */
.site-header {
  background: var(--color-primary);
  color: white;
  padding: var(--space-sm) 0;
}

.site-logo a {
  color: white;
  text-decoration: none;
  font-size: 1.5rem;
  font-weight: bold;
}

/* Navigation */
.main-nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-toggle {
  display: none;
}

.nav-toggle-label {
  display: none;
}

.nav-menu {
  display: flex;
  list-style: none;
  gap: var(--space-md);
}

.nav-item a {
  color: white;
  text-decoration: none;
}

.nav-item.active a {
  border-bottom: 2px solid white;
}

/* Mobile Navigation */
@media (max-width: 768px) {
  .nav-toggle-label {
    display: block;
    cursor: pointer;
  }
  
  .nav-toggle-label span {
    display: block;
    width: 25px;
    height: 3px;
    background: white;
    margin: 5px 0;
  }
  
  .nav-menu {
    display: none;
    flex-direction: column;
    position: absolute;
    top: 60px;
    right: 0;
    background: var(--color-primary);
    width: 100%;
  }
  
  .nav-toggle:checked ~ .nav-menu {
    display: flex;
  }
}

/* Main Content */
.main-content {
  min-height: calc(100vh - 200px);
  padding: var(--space-lg) 0;
}

/* Footer */
.site-footer {
  background: var(--color-secondary);
  color: white;
  padding: var(--space-lg) 0;
  margin-top: var(--space-lg);
}

.footer-content {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: var(--space-lg);
}
```

### Step 2: Download Prism.js

1. Visit https://prismjs.com/download.html
2. Select: Core + Line Numbers
3. Languages: C# (clike + csharp), JavaScript, JSON, PowerShell
4. Theme: Tomorrow Night
5. Download and save to `src/assets/js/prism.min.js` and `src/assets/css/prism-tomorrow.css`

---

## Phase 5: Create Homepage (20 minutes)

Create `src/pages/index.md`:

```markdown
---
layout: layouts/base.njk
title: Home
description: A production-ready HttpClient wrapper for .NET 8+ with resilience, caching, telemetry, and web crawling capabilities.
permalink: /
---

<div class="hero">
  <h1>{{ site.name }}</h1>
  <p class="hero-tagline">{{ nuget.description }}</p>
  
  <div class="hero-stats">
    <span class="stat">v{{ nuget.version }}</span>
    <span class="stat">{{ nuget.displayDownloads }} downloads</span>
  </div>
  
  <div class="hero-cta">
    <code>dotnet add package WebSpark.HttpClientUtility</code>
  </div>
</div>

## Features

- ✅ Simple one-line service registration
- ✅ Built-in resilience with Polly integration
- ✅ Automatic response caching
- ✅ Comprehensive telemetry and logging
- ✅ Web crawling capabilities

## Quick Example

```csharp
// Register services
builder.Services.AddHttpClientUtility();

// Make requests
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.example.com/weather",
    RequestMethod = HttpMethod.Get
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

<div class="cta-buttons">
  <a href="/getting-started/" class="button primary">Get Started</a>
  <a href="/features/" class="button secondary">View Features</a>
</div>
```

---

## Phase 6: Test Build (10 minutes)

### Step 1: Run Development Server

```bash
cd src
npm run dev
```

Visit http://localhost:8080 - you should see your homepage!

### Step 2: Test Production Build

```bash
npm run build
```

Check that `/docs` folder was created with generated files.

### Step 3: Validate Output

```bash
# Check HTML validity
npm run test:html

# Check for broken links
npm run test:links
```

---

## Phase 7: GitHub Pages Deployment (15 minutes)

### Step 1: Create GitHub Actions Workflow

Create `.github/workflows/publish-docs.yml`:

```yaml
name: Build and Deploy Documentation

on:
  push:
    branches: [main]
    paths:
      - 'src/**'
      - '.github/workflows/publish-docs.yml'
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: src/package-lock.json
      
      - name: Install dependencies
        working-directory: ./src
        run: npm ci
      
      - name: Build site
        working-directory: ./src
        run: npm run build
      
      - name: Commit and push
        run: |
          git config user.name "GitHub Actions"
          git config user.email "actions@github.com"
          git add docs/
          git diff --staged --quiet || git commit -m "docs: update documentation site"
          git push
```

### Step 2: Enable GitHub Pages

1. Go to repository **Settings** → **Pages**
2. Source: **Deploy from a branch**
3. Branch: **main** → **/docs** folder
4. Save

### Step 3: Push and Deploy

```bash
git add .
git commit -m "feat: add static documentation website"
git push origin main
```

GitHub Actions will build and deploy automatically!

---

## Phase 8: Content Migration (Ongoing)

Now that the infrastructure is ready, migrate content from existing documentation:

1. **Getting Started**: Migrate from `documentation/GettingStarted.md`
2. **Features**: Create individual pages for each feature
3. **API Reference**: Document core classes and interfaces
4. **Examples**: Add code examples from existing docs

---

## Troubleshooting

### Build Fails: "Cannot find module 'node-fetch'"

```bash
cd src
npm install node-fetch
```

### NuGet Data Not Fetching

Check console output:
```bash
npm run build
# Look for "✓ NuGet data fetched successfully" or error message
```

### CSS Not Loading

Verify passthrough copy in `.eleventy.js`:
```javascript
eleventyConfig.addPassthroughCopy("assets");
```

### Mobile Menu Not Working

Ensure checkbox input and label are correctly linked:
```html
<input type="checkbox" id="nav-toggle" ...>
<label for="nav-toggle" ...>
```

---

## Next Steps

1. ✅ Project setup complete
2. ✅ Basic templates and styling
3. ✅ Homepage created
4. ✅ GitHub Pages deployed
5. ⏭️ Migrate remaining content
6. ⏭️ Add feature pages
7. ⏭️ Add API reference documentation
8. ⏭️ Performance optimization
9. ⏭️ Accessibility audit

---

## Success Criteria Checklist

- [ ] Site builds without errors (`npm run build`)
- [ ] Development server runs (`npm run dev`)
- [ ] Homepage displays NuGet version and downloads
- [ ] Navigation works on desktop and mobile
- [ ] Site deploys to GitHub Pages
- [ ] All links work (no 404s)
- [ ] HTML validates
- [ ] Lighthouse scores 90+ (Performance, Accessibility, SEO)

---

## Resources

- [Eleventy Documentation](https://www.11ty.dev/docs/)
- [Nunjucks Templating](https://mozilla.github.io/nunjucks/)
- [Prism.js](https://prismjs.com/)
- [GitHub Pages Docs](https://docs.github.com/en/pages)

**Questions?** Open an issue or discussion in the repository!
