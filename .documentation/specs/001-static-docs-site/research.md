# Research Report: Static Documentation Website Technology Stack

**Feature**: Static Documentation Website for GitHub Pages  
**Date**: 2025-11-02  
**Status**: Complete

## Executive Summary

This research validates technology choices for building a static documentation website using Eleventy (11ty) that generates from `/src` to `/docs` for GitHub Pages. All technical unknowns from the planning phase have been resolved with concrete decisions and implementation guidance.

---

## A. Eleventy (11ty) Static Site Generator

### Decision: Eleventy 3.0+ with Nunjucks Templates

**Rationale**:
- **Eleventy 3.0+**: Latest stable release with ESM support, improved performance, better TypeScript support
- **Nunjucks**: Most powerful templating language, supports inheritance, macros, filters, and logic
- **Zero-config philosophy**: Eleventy works out-of-box with sensible defaults

**Alternatives Considered**:
- **Hugo**: Faster build times but Go-based (team is JavaScript-focused), less flexible templating
- **Jekyll**: GitHub Pages native but Ruby dependency, slower builds, less modern ecosystem
- **Astro**: Modern but overkill for documentation, requires component framework knowledge
- **Liquid templates**: Simpler but less powerful than Nunjucks (no macros, limited logic)

**Implementation Notes**:
```javascript
// .eleventy.js minimal configuration
module.exports = function(eleventyConfig) {
  // Copy assets through to output
  eleventyConfig.addPassthroughCopy("src/assets");
  
  // Set input/output directories
  return {
    dir: {
      input: "src",
      output: "docs",
      includes: "_includes",
      data: "_data"
    },
    templateFormats: ["md", "njk", "html"],
    markdownTemplateEngine: "njk",
    htmlTemplateEngine: "njk"
  };
};
```

**Project Structure Best Practices**:
```
src/
├── _data/           # Global data available to all templates
├── _includes/       # Layouts, components, partials
├── assets/          # Static files (CSS, JS, images)
└── pages/           # Content (Markdown + front matter)
```

**Development Server**:
- Use `eleventy --serve` for hot-reloading development
- BrowserSync built-in for live reload
- Typical startup time: <2 seconds

**Build Optimization**:
- Use `eleventy` (production build) for final output
- Minify HTML with `@11ty/eleventy-plugin-html-minifier` (optional)
- Bundle CSS/JS separately (keep Eleventy focused on HTML)

---

## B. NuGet API Integration

### Decision: Build-Time Fetch via Eleventy Global Data File

**Rationale**:
- **Performance**: Fetch once at build time vs every page load
- **Reliability**: Can cache and fallback if API fails
- **SEO**: Data is in HTML, not client-side JavaScript
- **Simplicity**: Eleventy _data files handle async naturally

**NuGet API Endpoint Analysis**:
```
URL: https://api.nuget.org/v3-flatcontainer/webspark.httpclientutility/index.json
Alternative: https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility
```

**Response Structure** (from NuGet Search API v3):
```json
{
  "data": [{
    "id": "WebSpark.HttpClientUtility",
    "version": "1.4.0",
    "description": "...",
    "totalDownloads": 15234,
    "verified": false,
    "packageTypes": [{"name": "Dependency"}],
    "authors": ["Mark Hazleton"],
    "tags": ["http", "client", "utility"],
    "projectUrl": "https://github.com/markhazleton/httpclientutility",
    "licenseUrl": "https://licenses.nuget.org/MIT"
  }]
}
```

**Implementation Pattern**:
```javascript
// src/_data/nuget.js (Eleventy global data file)
const fetch = require('node-fetch');
const fs = require('fs');
const path = require('path');

const CACHE_FILE = path.join(__dirname, 'nuget-cache.json');
const API_URL = 'https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility';

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
        description: packageData.description,
        lastUpdate: new Date().toISOString(),
        cached: false
      };
    }
  } catch (error) {
    console.warn('⚠ NuGet API failed, using cached data:', error.message);
    
    // Fallback to cache
    if (fs.existsSync(CACHE_FILE)) {
      const cached = JSON.parse(fs.readFileSync(CACHE_FILE, 'utf8'));
      return {
        version: cached.version,
        downloads: cached.totalDownloads,
        description: cached.description,
        lastUpdate: cached.lastUpdate || 'Unknown',
        cached: true
      };
    }
    
    // Ultimate fallback
    return {
      version: '1.4.0',
      downloads: 0,
      description: 'A production-ready HttpClient wrapper',
      lastUpdate: 'Unknown',
      cached: true,
      error: true
    };
  }
};
```

**Rate Limiting Considerations**:
- NuGet Search API: No documented rate limits for reasonable use
- Build frequency: Typically 1-10 builds/day for docs updates
- Caching: Successful responses cached, reduces API calls
- Recommendation: No additional throttling needed

**Alternatives Considered**:
- **Client-side fetch**: Slower (network waterfall), SEO issues, requires JavaScript
- **Manual updates**: Error-prone, defeats purpose of automation
- **GitHub Actions cron job**: Overcomplicated, unnecessary scheduled builds

---

## C. Prism.js Syntax Highlighting

### Decision: Self-Hosted Prism.js with Custom Build (C#, JavaScript, JSON, PowerShell)

**Rationale**:
- **Custom Build**: Only include needed languages → smaller bundle (30KB vs 200KB)
- **Self-Hosted**: No CDN dependency, works offline, better performance
- **Build-Time Highlighting**: Eleventy plugin available for build-time highlighting (even faster)

**Implementation Approach**:
1. **Download custom Prism.js** from https://prismjs.com/download.html
   - Core + Line Numbers plugin
   - Languages: C# (clike → csharp), JavaScript, JSON, PowerShell
   - Theme: Tomorrow Night (professional, good contrast)
   - Result: ~35KB minified

2. **Manual Integration** (Simpler than plugin for small sites):
   ```html
   <!-- In base layout <head> -->
   <link rel="stylesheet" href="/assets/css/prism-tomorrow.css">
   
   <!-- Before </body> -->
   <script src="/assets/js/prism.min.js"></script>
   ```

3. **Usage in Markdown**:
   ````markdown
   ```csharp
   services.AddHttpClientUtility(options => {
       options.EnableCaching = true;
   });
   ```
   ````

**Alternatives Considered**:
- **Highlight.js**: Heavier (189 languages default), auto-detection can be wrong
- **Shiki**: VS Code highlighter, most accurate but slow builds (~5s for 30 snippets)
- **Build-time highlighting**: `@11ty/eleventy-plugin-syntaxhighlight` - adds build complexity
- **CDN-hosted Prism**: External dependency, privacy concerns, slower (extra DNS/TCP)

**Theme Selection**:
- **Tomorrow Night**: Dark theme, excellent for code readability
- **Prism Default**: Light theme option for accessibility toggle (future enhancement)
- **Line Numbers**: Optional but recommended for longer snippets

---

## D. GitHub Pages + Eleventy Integration

### Decision: GitHub Actions Workflow with Manual Build Trigger

**Rationale**:
- **Automation**: Docs rebuild on every push to main or manual trigger
- **Consistency**: Same build process locally and in CI
- **Control**: Can run on schedule or manual for testing
- **Simplicity**: Single workflow file handles everything

**GitHub Actions Workflow**:
```yaml
# .github/workflows/publish-docs.yml
name: Build and Deploy Documentation

on:
  push:
    branches: [main]
    paths:
      - 'src/**'
      - '.github/workflows/publish-docs.yml'
  workflow_dispatch:  # Manual trigger

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
      
      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./docs
          publish_branch: gh-pages  # Or commit to main's /docs
```

**GitHub Pages Configuration**:
1. **Repository Settings** → Pages → Source: Deploy from a branch
2. **Branch**: `main` (or `gh-pages`) → `/docs` folder
3. **Custom domain**: Optional (not in initial scope)

**Critical Files**:
- **`.nojekyll`**: Create empty file in `/docs` to disable Jekyll processing
  ```bash
  # In .eleventy.js passthrough copy
  eleventyConfig.addPassthroughCopy({ "src/.nojekyll": ".nojekyll" });
  ```

**Asset Path Handling**:
- Use relative paths: `/assets/css/main.css` (works with or without custom domain)
- Eleventy automatically handles path resolution
- No special configuration needed for github.io domain

**Alternatives Considered**:
- **Jekyll native**: GitHub's default but slower, Ruby dependency
- **Netlify/Vercel**: Better performance but adds external service dependency
- **Manual build + commit**: Error-prone, no automation

---

## E. CSS-Only Hamburger Menu

### Decision: Checkbox Hack with ARIA Labels

**Rationale**:
- **Zero JavaScript**: Works without JS (progressive enhancement requirement)
- **Accessible**: Proper ARIA labels and keyboard navigation
- **Standard Pattern**: Well-documented, browser-tested technique
- **Lightweight**: ~50 lines of CSS

**Implementation Pattern**:
```html
<!-- In header.njk -->
<nav class="main-nav">
  <input type="checkbox" id="nav-toggle" class="nav-toggle" aria-label="Toggle navigation">
  <label for="nav-toggle" class="nav-toggle-label">
    <span></span><!-- Hamburger icon lines -->
  </label>
  
  <ul class="nav-menu">
    <li><a href="/">Home</a></li>
    <li><a href="/features/">Features</a></li>
    <!-- ... -->
  </ul>
</nav>
```

```css
/* Mobile-first CSS */
.nav-toggle {
  display: none; /* Hide checkbox */
}

.nav-menu {
  display: none; /* Hidden by default on mobile */
}

.nav-toggle:checked ~ .nav-menu {
  display: block; /* Show when checkbox checked */
}

.nav-toggle-label {
  /* Hamburger icon styling */
  display: block; /* Visible on mobile */
}

@media (min-width: 768px) {
  .nav-toggle-label {
    display: none; /* Hide hamburger on desktop */
  }
  
  .nav-menu {
    display: flex; /* Always visible on desktop */
  }
}
```

**Accessibility Enhancements**:
- `aria-label="Toggle navigation"` on checkbox
- `aria-expanded` dynamically via optional JS enhancement
- Focus styles on label for keyboard navigation
- Skip to content link for screen readers

**Alternatives Considered**:
- **JavaScript-only toggle**: Violates progressive enhancement requirement
- **`:target` pseudo-class**: Affects URL hash, breaks back button
- **`<details>` element**: Semantic but harder to style consistently

**Progressive Enhancement Layer** (Optional JavaScript):
```javascript
// src/assets/js/nav-toggle.js (optional)
// Adds smooth transitions and closes menu on outside click
const navToggle = document.getElementById('nav-toggle');
const navMenu = document.querySelector('.nav-menu');

document.addEventListener('click', (e) => {
  if (!navMenu.contains(e.target) && !e.target.matches('.nav-toggle-label')) {
    navToggle.checked = false;
  }
});
```

---

## F. Custom CSS with CSS Custom Properties

### Decision: Minimal Custom CSS Framework with Design Tokens

**Rationale**:
- **Maintainability**: CSS variables enable theme consistency
- **Performance**: ~10KB total CSS vs 150KB+ for Bootstrap
- **Control**: Full design control without framework constraints
- **Modern**: CSS custom properties supported in all target browsers

**Design Token Structure**:
```css
/* src/assets/css/main.css */
:root {
  /* Colors */
  --color-primary: #0066cc;
  --color-secondary: #555;
  --color-success: #28a745;
  --color-background: #ffffff;
  --color-text: #333;
  --color-border: #e0e0e0;
  
  /* Spacing */
  --space-xs: 0.5rem;
  --space-sm: 1rem;
  --space-md: 1.5rem;
  --space-lg: 2rem;
  --space-xl: 3rem;
  
  /* Typography */
  --font-body: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  --font-code: "Consolas", "Monaco", monospace;
  --font-size-base: 16px;
  --line-height-base: 1.6;
  
  /* Layout */
  --max-width-content: 1200px;
  --max-width-text: 720px;
  
  /* Transitions */
  --transition-fast: 150ms ease;
  --transition-base: 300ms ease;
}

/* Dark mode support (future enhancement) */
@media (prefers-color-scheme: dark) {
  :root {
    --color-background: #1a1a1a;
    --color-text: #e0e0e0;
    --color-border: #333;
  }
}
```

**File Organization**:
```
src/assets/css/
├── main.css           # Variables, reset, base styles
├── layout.css         # Grid, container, navigation
├── components.css     # Buttons, cards, badges
├── responsive.css     # Media queries, mobile overrides
└── prism-theme.css    # Prism.js syntax theme
```

**Critical CSS Patterns**:
- Mobile-first responsive design
- Flexbox for layouts (Grid for complex layouts)
- BEM naming convention for components
- Utility classes sparingly (margin/padding helpers only)

**Alternatives Considered**:
- **Tailwind CSS**: Requires PostCSS build step, verbose HTML, 200KB+ pre-purge
- **Bootstrap**: 150KB bundle, prescriptive design, harder to customize
- **Pico.css/Water.css**: Classless CSS, too minimal for custom branding
- **Sass/SCSS**: Adds build complexity, CSS custom properties sufficient

---

## G. Performance Optimization Strategy

### Decision: Multi-Layered Optimization Approach

**Bundle Size Targets**:
- HTML: 10-30KB per page (gzipped)
- CSS: 10-15KB total (gzipped)
- JavaScript: 30-40KB (Prism.js + optional enhancements, gzipped)
- Images: WebP format, <100KB each

**Optimization Techniques**:

1. **HTML Minification**:
   ```bash
   npm install --save-dev html-minifier-terser
   # Add to build script
   ```

2. **CSS Minification**:
   ```bash
   npm install --save-dev cssnano postcss postcss-cli
   # PostCSS config for production
   ```

3. **Image Optimization**:
   - Use WebP with PNG/JPEG fallback
   - Manual optimization via Squoosh or ImageOptim
   - Lazy loading: `<img loading="lazy">` (native)

4. **Critical CSS** (Future Enhancement):
   - Inline above-the-fold CSS
   - Defer non-critical CSS

5. **CDN Strategy**:
   - GitHub Pages uses Fastly CDN automatically
   - No additional configuration needed

**Lighthouse Target Scores**:
- Performance: 90+ (target: 95+)
- Accessibility: 90+ (target: 100)
- Best Practices: 90+ (target: 100)
- SEO: 90+ (target: 100)

---

## H. NPM Scripts and Build Pipeline

### Decision: Standard NPM Scripts for All Operations

**package.json Scripts**:
```json
{
  "scripts": {
    "clean": "rimraf ../docs",
    "dev": "eleventy --serve --incremental",
    "build": "npm run clean && eleventy",
    "build:prod": "npm run clean && NODE_ENV=production eleventy",
    "test:links": "hyperlink docs",
    "test:html": "htmlhint docs/**/*.html",
    "lighthouse": "lighthouse http://localhost:8080 --view"
  },
  "devDependencies": {
    "@11ty/eleventy": "^3.0.0",
    "rimraf": "^5.0.5",
    "node-fetch": "^3.3.2",
    "htmlhint": "^1.1.4",
    "hyperlink": "^5.0.4"
  }
}
```

**Workflow**:
1. **Development**: `npm run dev` → Live reload at http://localhost:8080
2. **Production Build**: `npm run build:prod` → Optimized output to /docs
3. **Testing**: `npm run test:links && npm run test:html`
4. **Clean**: `npm run clean` → Remove /docs completely

---

## I. Testing and Validation Strategy

### Decision: Multi-Layer Validation Approach

**Build-Time Validation**:
1. **Eleventy Build Success**: Exit code 0 = valid templates/data
2. **Link Checker**: `hyperlink` validates all internal/external links
3. **HTML Validation**: `htmlhint` checks HTML validity

**Manual Testing Checklist**:
- [ ] Homepage loads and displays NuGet data
- [ ] All navigation links work
- [ ] Mobile menu toggles correctly
- [ ] Code examples syntax-highlight properly
- [ ] Pages load without JavaScript
- [ ] Forms/interactive elements accessible via keyboard

**Automated Testing** (GitHub Actions):
```yaml
- name: Validate HTML
  run: npm run test:html
  
- name: Check Links
  run: npm run test:links
  
- name: Lighthouse CI
  uses: treosh/lighthouse-ci-action@v10
  with:
    urls: |
      http://localhost:8080
      http://localhost:8080/features/
    uploadArtifacts: true
```

**Browser Testing Matrix**:
- Chrome 90+ (latest stable)
- Firefox 88+ (latest stable)
- Safari 14+ (macOS/iOS)
- Edge 90+ (latest stable)
- Mobile: iOS Safari 14+, Android Chrome 90+

---

## J. Content Migration and Maintenance

### Decision: Manual Content Creation with Templates

**Content Sources**:
1. **README.md** → Homepage hero section and quick start
2. **CHANGELOG.md** → Changelog page (Eleventy collection)
3. **documentation/*.md** → Feature pages (restructured)
4. **New content**: API reference, examples, contributing guide

**Content Templates**:
```markdown
---
layout: layouts/page.njk
title: Page Title
description: SEO description
eleventyNavigation:
  key: Page Key
  order: 1
---

# {{ title }}

Content here...
```

**Update Frequency**:
- NuGet data: Every build (automated)
- Code examples: Manual updates with library changes
- Feature docs: Updated with each release
- API reference: Updated with breaking changes

**Version Tracking**:
- Documentation version matches library version
- Display library version from NuGet data
- No separate version for docs (single-version approach)

---

## K. Security and Privacy Considerations

### Decision: Privacy-First, Minimal Tracking

**Security Measures**:
1. **No User Input**: Static site, no forms/authentication
2. **No Cookies**: No client-side storage required
3. **HTTPS**: GitHub Pages enforces HTTPS automatically
4. **No Third-Party Scripts**: Self-hosted assets only (no analytics)
5. **Content Security Policy** (Future Enhancement):
   ```html
   <meta http-equiv="Content-Security-Policy" content="default-src 'self'; img-src 'self' data:">
   ```

**Privacy**:
- No analytics by default (GitHub provides basic traffic stats)
- No external CDN dependencies (all assets self-hosted)
- No tracking pixels or beacons
- Respect Do Not Track header (if analytics added later)

**Dependency Security**:
- Use `npm audit` to check for vulnerabilities
- Pin exact versions in package-lock.json
- Update dependencies quarterly (security patches immediately)

---

## Implementation Recommendations

### Phase 1: Minimum Viable Documentation (Week 1)
1. Set up Eleventy project structure
2. Create base layout and navigation
3. Implement NuGet API fetching
4. Build homepage with package stats
5. Add 2-3 core pages (features, getting started)

### Phase 2: Content Completion (Week 2)
1. Migrate/write all documentation pages
2. Add code examples with Prism.js
3. Implement responsive design and mobile menu
4. Add comparison table and feature grid

### Phase 3: Polish and Optimization (Week 3)
1. Performance optimization (minification, image optimization)
2. Accessibility audit and fixes
3. SEO optimization (meta tags, sitemap)
4. Testing across browsers
5. GitHub Actions workflow setup

### Phase 4: Launch and Maintenance (Ongoing)
1. Deploy to GitHub Pages
2. Update NuGet package page with docs link
3. Monitor Lighthouse scores
4. Content updates with library releases

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| NuGet API changes format | Low | Medium | Cache last known good structure, version detection |
| GitHub Pages outage | Low | Low | Site is static, no dynamic dependencies |
| Build fails in CI | Medium | Medium | Local build validation, comprehensive testing |
| Mobile browser compatibility | Low | Medium | Progressive enhancement, BrowserStack testing |
| Performance regression | Medium | High | Lighthouse CI in GitHub Actions, budget alerts |
| Stale documentation | High | Medium | Update docs checklist in release process |

---

## Conclusion

All technology decisions validated and implementation-ready. No blockers identified. Proceed to Phase 1 (Design & Contracts).

**Key Deliverables from Research**:
- ✅ Eleventy 3.0+ with Nunjucks templates
- ✅ NuGet API integration pattern with caching
- ✅ Prism.js custom build strategy
- ✅ GitHub Actions deployment workflow
- ✅ CSS-only hamburger menu pattern
- ✅ Custom CSS with design tokens
- ✅ Performance optimization strategy
- ✅ Testing and validation approach

**Next Steps**: Generate data-model.md (content structure) and contracts/ (file specifications).
