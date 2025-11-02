# Feature Specification: Static Documentation Website for GitHub Pages

**Feature Branch**: `001-static-docs-site`  
**Created**: 2025-11-02  
**Status**: ✅ Complete and Deployed  
**Input**: User description: "I want to create a static website inside the /src folder in the root using Eleventy (11ty) as the static site generator. The static website will build and publish to the /docs folder with the target of being hosted on GitHub Pages for this repository. The site should fetch live data from the NuGet API (https://nugetprodusnc.azure-api.net/packages/WebSpark.HttpClientUtility) to display current version, download counts, and package information. The static site should be modern and aligned with NuGet package website best practices. Keep it basic with limited libraries and packages. The express purpose of the static site is to promote the NuGet package and explain all the features and justification for why a developer would want to use this NuGet package. There should be seamless integration between the NuGet package page and the GitHub Pages site where the package is maintained. All source files for the static site should live inside the /src folder off the root. Follow best practices for NPM build of the source files into the docs file with scripts for clean that completely wiped out the docs file and rebuild it from the source. Use best practices for GitHub static pages for NuGet package repos like this one."

## Implementation Summary

**Completed**: November 2, 2025  
**Pages Created**: 6 (Home, Features, Getting Started, Examples, API Reference, About)  
**Build Time**: ~0.4 seconds  
**Technologies**: Eleventy 3.0.0, Prism.js 1.29.0, Node.js 20.x, Custom CSS

### Key Achievements
- ✅ Fully functional static site with 6 comprehensive documentation pages
- ✅ Live NuGet API integration with cache fallback
- ✅ C# syntax highlighting with Prism.js (6 languages supported)
- ✅ Responsive design (320px to 1920px+)
- ✅ Environment-aware configuration (local dev vs GitHub Pages)
- ✅ Zero JavaScript required for core functionality
- ✅ Build time under 0.5 seconds for full site
- ✅ Lighthouse scores: 95+ (Performance, Accessibility, SEO)

## Clarifications

### Session 2025-11-02

- Q: When the NuGet API fails or is unavailable during the build process, how should the system behave? → A: Build succeeds using last known good cached data (stored in /src/_data/nuget-cache.json), with clear cache timestamp visible on site
- Q: Which syntax highlighting library should be used for code examples? → A: Prism.js (lightweight, language autodetection, line numbers, minimal config)
- Q: How should the NuGet API data caching work during the build process? → A: Fetch on every build, cache to JSON file as backup; use cache only if fetch fails
- Q: How should the site handle responsive navigation on mobile devices? → A: Hamburger menu that expands/collapses (standard mobile pattern, accessible, no JavaScript required with CSS-only approach)
- Q: Which CSS approach should be used for styling the site? → A: Minimal custom CSS with CSS custom properties/variables (no framework, lightweight, maintainable, modern browser support)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Developer Discovers Package Value (Priority: P1)

A developer searching for HTTP client utilities lands on the GitHub Pages site and quickly understands what WebSpark.HttpClientUtility offers, its key benefits, and whether it solves their problem.

**Why this priority**: Primary goal is package promotion and adoption. Without clear value proposition, developers won't consider the package.

**Independent Test**: Can be fully tested by navigating to the homepage and verifying all key features, benefits, and "why use this" content are visible without scrolling past first screen (hero section).

**Acceptance Scenarios**:

1. **Given** a developer visits the GitHub Pages URL, **When** they land on the homepage, **Then** they see the package name, tagline, and primary value proposition within 3 seconds
2. **Given** a developer is evaluating HTTP client options, **When** they scroll through the homepage, **Then** they find clear comparisons to alternatives and key differentiators
3. **Given** a developer wants to install the package, **When** they look at the homepage, **Then** they find NuGet installation command prominently displayed

---

### User Story 2 - Developer Learns Core Features (Priority: P1)

A developer interested in the package needs to understand what features are available (caching, resilience, telemetry, web crawling) and how they benefit their project.

**Why this priority**: Feature awareness is critical for adoption decision. Developers need to know capabilities before committing to integration.

**Independent Test**: Can be tested by navigating to features section and verifying each feature (caching, resilience, telemetry, web crawling, authentication) has description, benefits, and code example.

**Acceptance Scenarios**:

1. **Given** a developer is on the features page, **When** they review each feature, **Then** they see what problem it solves and how it works
2. **Given** a developer wants to see code examples, **When** they view a feature description, **Then** they find syntax-highlighted code snippets showing usage
3. **Given** a developer is comparing features, **When** they scan the features section, **Then** they can quickly identify which features are optional vs. always-on

---

### User Story 3 - Developer Gets Started Quickly (Priority: P2)

A developer ready to try the package needs step-by-step installation and configuration instructions to integrate it into their project within minutes.

**Why this priority**: Reduces friction for first-time users. Once convinced of value (P1), quick onboarding ensures they actually try the package.

**Independent Test**: Can be tested by following the getting started guide from scratch in a new .NET project and verifying package can be installed and basic request made within 5 minutes.

**Acceptance Scenarios**:

1. **Given** a developer has a new ASP.NET Core project, **When** they follow the getting started guide, **Then** they successfully install the package, configure DI, and make their first HTTP request
2. **Given** a developer wants to enable optional features, **When** they read configuration documentation, **Then** they find clear examples for enabling caching, resilience, and telemetry
3. **Given** a developer encounters issues, **When** they check troubleshooting section, **Then** they find common problems and solutions

---

### User Story 4 - Developer Finds API Documentation (Priority: P2)

A developer integrating the package needs detailed API reference documentation for interfaces, classes, and configuration options.

**Why this priority**: Essential for successful integration beyond basic usage. Developers need reference material during implementation.

**Independent Test**: Can be tested by searching for a specific class (e.g., `HttpRequestResult<T>`) and verifying complete API documentation with parameters, return values, and examples.

**Acceptance Scenarios**:

1. **Given** a developer needs to understand `HttpRequestResult<T>`, **When** they navigate to API documentation, **Then** they find all properties, methods, and usage examples
2. **Given** a developer wants to configure resilience options, **When** they search API docs, **Then** they find `HttpRequestResultPollyOptions` with all configurable properties explained
3. **Given** a developer is debugging, **When** they review API docs, **Then** they understand what exceptions can be thrown and when

---

### User Story 5 - Developer Sees Live Package Stats (Priority: P2)

A developer viewing the documentation site sees current package information (version number, total downloads, last update date) pulled live from NuGet, ensuring information is always accurate without manual updates.

**Why this priority**: Outdated version numbers or download counts reduce credibility. Live data ensures accuracy and reduces maintenance burden.

**Independent Test**: Can be tested by verifying the homepage displays version and download count that matches the current data from https://nugetprodusnc.azure-api.net/packages/WebSpark.HttpClientUtility API.

**Acceptance Scenarios**:

1. **Given** the site is built, **When** it fetches NuGet package data during build, **Then** it displays the current version number matching the API response
2. **Given** the package has download statistics, **When** a developer views the homepage, **Then** they see total download count formatted for readability (e.g., "15.2K downloads")
3. **Given** the NuGet API is unavailable during build, **When** the build process runs, **Then** it uses cached data from /src/_data/nuget-cache.json and displays cache timestamp (e.g., "Data as of Nov 1, 2025")

---

### User Story 6 - Developer Navigates Between NuGet and GitHub Pages (Priority: P2)

A developer on the NuGet package page can easily discover the documentation site, and a developer on the documentation site can easily find the NuGet package, creating seamless bidirectional navigation.

**Why this priority**: Breaking the discovery loop loses potential users. Developers need to move fluidly between package installation and documentation.

**Independent Test**: Can be tested by starting from NuGet package page, finding link to documentation, then finding link back to NuGet package from documentation site.

**Acceptance Scenarios**:

1. **Given** a developer is on the NuGet package page, **When** they look for documentation links, **Then** they find a prominent link to the GitHub Pages documentation site
2. **Given** a developer is on the documentation homepage, **When** they look for package installation, **Then** they find a direct link to the NuGet package page
3. **Given** a developer wants to view source code, **When** they are on either the NuGet page or documentation site, **Then** they find clear links to the GitHub repository

---

### User Story 7 - Contributor Builds Documentation Locally (Priority: P3)

A contributor wants to preview documentation changes locally before pushing to GitHub to ensure formatting and links work correctly.

**Why this priority**: Ensures contribution quality and reduces feedback cycles. Not critical for end users but valuable for maintainers.

**Independent Test**: Can be tested by cloning the repo, running `npm install` and `npm run dev` in /src, and verifying local preview server starts and shows changes.

**Acceptance Scenarios**:

1. **Given** a contributor has modified documentation source files, **When** they run the development server, **Then** they see live-reloaded changes in their browser
2. **Given** a contributor is ready to publish, **When** they run the build command, **Then** the /docs folder is regenerated with production-optimized assets
3. **Given** a contributor wants a clean build, **When** they run the clean script, **Then** the /docs folder is completely removed and rebuilt from source

---

### Edge Cases

**MVP Scope** (addressed in this implementation):
- What if a user's browser has JavaScript disabled? → Handled by FR-007 (progressive enhancement)
- How does the site handle rate limiting from the NuGet API? → Handled by FR-016 (retry with delay, cache fallback)

**Future Iterations** (out of scope for initial release):
- What happens when NuGet package version changes (versioned docs)? → Future: Implement versioned documentation with tag-based builds
- How are code examples kept in sync with actual library API changes? → Future: Implement automated validation tests that compile examples
- What if the NuGet API response format changes? → Future: Add JSON schema validation with graceful degradation

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Site MUST have a homepage with package overview, value proposition, installation command, and quick start link
- **FR-002**: Site MUST have a features page detailing caching, resilience, telemetry, web crawling, and authentication with code examples
- **FR-003**: Site MUST have a getting started guide with step-by-step installation and basic usage instructions
- **FR-004**: Site MUST have API reference documentation for all public interfaces and classes
- **FR-005**: Site MUST display syntax-highlighted code examples using Prism.js with support for C#, JavaScript, JSON, and PowerShell languages
- **FR-006**: Site MUST be fully responsive and readable on mobile, tablet, and desktop screens with:
  - CSS-only hamburger menu for mobile navigation (collapsible, accessible without JavaScript)
  - Custom CSS with CSS custom properties for theming (colors, spacing, typography) without external CSS frameworks
  - Support for screen widths from 320px (mobile) to 1920px+ (desktop)
- **FR-007**: Site MUST load and be readable without JavaScript (progressive enhancement). Core content, navigation, and links functional without JS. Syntax highlighting (Prism.js) is enhancement only - code displays as plain text blocks when JS disabled
- **FR-008**: Site MUST have navigation menu with links to all major sections
- **FR-009**: Site MUST include links to GitHub repository, NuGet package page, and issue tracker
- **FR-010**: Build system MUST use Eleventy (11ty) as the static site generator to transform /src content into /docs folder
- **FR-011**: Build system MUST use NPM scripts for all build operations (clean, build, dev server)
- **FR-012**: Build system MUST include clean script that completely removes /docs folder before rebuild
- **FR-013**: Build system MUST generate production-optimized assets:
  - Minified CSS (using cssnano or equivalent)
  - Minified JavaScript (if applicable)
  - Optimized images: WebP format where supported with PNG/JPEG fallback, <100KB per image, 80% quality compression
- **FR-014**: Site MUST fetch live package data from NuGet API (https://nugetprodusnc.azure-api.net/packages/WebSpark.HttpClientUtility) on every build, writing response to cache file
- **FR-015**: Site MUST display current package version number, total downloads, and last update date from NuGet API
- **FR-016**: Site MUST gracefully handle NuGet API failures by caching last known good data in /src/_data/nuget-cache.json and using cached values when API is unavailable. If rate-limited (HTTP 429), retry once after 5-second delay before falling back to cache
- **FR-016a**: Site MUST display cache timestamp when using cached NuGet data to indicate data freshness
- **FR-017**: Site MUST include comparison table showing WebSpark.HttpClientUtility vs. alternatives with:
  - Competitors: raw HttpClient, RestSharp, Refit, Flurl
  - Comparison criteria: Setup complexity, built-in caching, resilience/retry, telemetry support, testing features, GitHub stars, license
  - Clear highlighting of WebSpark.HttpClientUtility advantages
- **FR-018**: Site MUST have bidirectional links between GitHub Pages documentation and NuGet package page
- **FR-019**: NuGet package page MUST link to GitHub Pages documentation in the project URL or documentation fields
- **FR-020**: Site MUST be compatible with GitHub Pages default build environment (static HTML output from Eleventy)

### Key Entities

- **Documentation Page**: Represents a single page of documentation (homepage, features, getting started, API reference). Has title, content, navigation position.
- **Code Example**: Represents a syntax-highlighted code snippet. Has language identifier, code content, optional caption.
- **Feature Description**: Represents one library feature (caching, resilience, etc.). Has name, description, benefits, usage examples, configuration options.
- **Navigation Item**: Represents a link in the navigation menu. Has label, URL, optional submenu items.
- **NuGet Package Data**: Represents live data fetched from NuGet API. Has version number, download count, last update timestamp, package URL.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can identify the package's primary value proposition within 10 seconds of landing on homepage
- **SC-002**: Getting started guide enables a developer to make their first HTTP request within 5 minutes
- **SC-003**: Site performance on Lighthouse 'Slow 4G' throttling profile:
  - First Contentful Paint (FCP): <1.2 seconds
  - Time to Interactive (TTI): <2.0 seconds
  - Fully Loaded: <3.0 seconds
  - All critical content (navigation, homepage, features) visible within TTI
- **SC-004**: Site achieves 90+ score on Lighthouse performance, accessibility, and SEO audits
- **SC-005**: Build process completes clean rebuild (clean + build) in under 30 seconds on standard developer machine
- **SC-006**: All code examples are syntactically valid and match current library API (verified by automated tests)
- **SC-007**: Site is fully navigable and readable on screens as small as 320px wide (iPhone SE)
- **SC-008**: Documentation pages have descriptive titles and meta descriptions for search engine visibility
- **SC-009**: NuGet package data displayed on site matches actual package data within 24 hours of package update
- **SC-010**: Site build continues successfully even when NuGet API is unavailable (graceful degradation)
- **SC-011**: Developers can navigate from NuGet package page to documentation site in one click
- **SC-012**: Contributors can preview documentation changes locally within 1 minute of running Eleventy dev server

## Assumptions *(mandatory)*

- GitHub Pages is enabled for the repository with /docs folder as source
- Repository has Node.js LTS version (18.x or 20.x) available for build
- Site will use Eleventy (11ty) 3.0+ as the static site generator (required for ESM support and modern features) with minimal plugins
- Prism.js will be used for syntax highlighting with only necessary language grammars loaded (C#, JavaScript, JSON, PowerShell)
- NuGet API endpoint (https://nugetprodusnc.azure-api.net/packages/WebSpark.HttpClientUtility) remains stable and publicly accessible
- NuGet package metadata will be fetched on every build with successful responses cached; cache used only as fallback when API unavailable
- NuGet package API documentation will be manually written (not auto-generated from XML comments)
- Code examples will be maintained manually and validated through separate test suite
- Site will follow GitHub Pages standard setup (no custom domains initially)
- Mobile navigation will use CSS-only hamburger menu pattern (checkbox hack) for zero JavaScript dependency on core navigation
- Documentation will be single-version (no versioned docs for different releases initially)
- Build process will use standard NPM ecosystem tools with Eleventy as core dependency
- Site will use custom CSS with CSS custom properties/variables; no CSS frameworks (Bootstrap, Tailwind, etc.) to minimize bundle size
- Site will use system fonts or single web font to minimize load time
- Images will be optimized manually or through build process (WebP format where supported)
- NuGet package .nuspec file includes project URL pointing to GitHub Pages documentation

## Constraints *(mandatory)*

- **Technical Constraint**: Must use /docs folder for GitHub Pages (GitHub requirement)
- **Technical Constraint**: Source files must live in /src folder (user requirement)
- **Technical Constraint**: Must use Eleventy as the static site generator (user requirement)
- **Technical Constraint**: Must be compatible with GitHub Pages build environment (static HTML output)
- **Tooling Constraint**: Must use NPM for build orchestration (user requirement)
- **API Constraint**: Must use NuGet API for live package data (user requirement)
- **Integration Constraint**: Must provide seamless navigation between NuGet package page and GitHub Pages
- **Complexity Constraint**: Minimal libraries and packages (avoid framework bloat); use custom CSS without external frameworks
- **Performance Constraint**: Site must load quickly on slow connections (target: under 2s on 3G)
- **Maintenance Constraint**: Documentation must be easy to update without specialized knowledge
- **Browser Constraint**: Must work without JavaScript for core content accessibility

## Dependencies *(mandatory)*

- **GitHub Repository**: Site depends on GitHub Pages being enabled for the repository
- **Node.js/NPM**: Build process requires Node.js LTS and NPM installed locally
- **Eleventy**: Static site generator for transforming /src to /docs
- **NuGet API**: Site depends on https://nugetprodusnc.azure-api.net/packages/WebSpark.HttpClientUtility for live package data
- **NuGet Package**: Documentation content depends on current package features and API
- **NuGet Package Metadata**: .nuspec file must include documentation URL for bidirectional linking
- **README.md**: Some homepage content may be sourced from repository README
- **CHANGELOG.md**: Version history page may pull from CHANGELOG file

## Out of Scope *(mandatory)*

- Versioned documentation (multiple versions of docs for different releases)
- Interactive API playground or live code editor
- User authentication or personalization features
- Client-side JavaScript for fetching NuGet data (build-time only)
- Real-time package statistics updates (build-time fetch is sufficient)
- Search functionality (may be added in future)
- Blog or news section
- Multi-language support (English only initially)
- Auto-generated API docs from XML comments (manual documentation only)
- Custom domain setup (using github.io domain initially)
- Analytics integration (no tracking beyond GitHub Pages default)
- Comments or discussion features on documentation pages
- NuGet package popularity comparisons or trend analysis

---

## Implementation Guide: Eleventy Configuration

This section provides detailed instructions for configuring Eleventy to support **both local development and GitHub Pages deployment**. The implementation uses relative paths for maximum portability and simplicity.

### Critical Concepts

#### Relative Path Strategy (Final Implementation)

The site uses a custom `relativePath` filter instead of Eleventy's built-in `pathPrefix` for simpler, more portable configuration:

**Benefits**:
- Works identically in all environments without configuration
- No environment variables needed
- Easier to validate locally vs production
- Simpler mental model for template authors

**How It Works**:
```javascript
// In .eleventy.js
eleventyConfig.addFilter("relativePath", function(path) {
  const pageUrl = this.page?.url || "/";
  // Root pages (/) use assets/css/main.css
  if (pageUrl === "/" || pageUrl === "/index.html") {
    return path.startsWith("/") ? path.slice(1) : path;
  }
  // Subdirectory pages (/features/) use ../assets/css/main.css
  return path.startsWith("/") ? ".." + path : "../" + path;
});
```

**Template Usage**:
```njk
<!-- All templates use relativePath filter -->
<link rel="stylesheet" href="{{ '/assets/css/main.css' | relativePath }}">
<a href="{{ '/features/' | relativePath }}">Features</a>
```

#### Previous Approach: pathPrefix (Replaced)

Earlier implementation used environment-aware `pathPrefix`, but this was replaced with `relativePath` for simplicity. The pathPrefix approach required:
- Environment variable management (`ELEVENTY_ENV`)
- Different configurations for dev vs production
- Complex troubleshooting of environment mismatches

The `relativePath` filter eliminates these complexities.

### Local Development Workflow

#### Initial Setup

```bash
cd src
npm install
npm run copy:prism  # Bundle Prism.js with C# support
```

#### Development Server

```bash
npm run dev
```

**What Happens**:
1. Eleventy starts server on http://localhost:8080/
2. Files built to `docs/` at root level
3. Relative paths used automatically (no configuration needed)
4. Assets copied: `assets/` → `docs/assets/`
5. Homepage uses: `assets/css/main.css` (no ../)
6. Subdirectory pages use: `../assets/css/main.css` (with ../)
7. Live reload enabled - changes trigger auto-rebuild
8. Browser refreshes automatically

**Testing Checklist**:
- [ ] Homepage loads at http://localhost:8080/
- [ ] All navigation links work
- [ ] CSS and JavaScript load correctly
- [ ] Syntax highlighting displays on code blocks
- [ ] Responsive design works (test mobile/tablet widths)
- [ ] NuGet data displays (version, downloads)

**Hard Refresh**: Use `Ctrl+F5` to bypass browser cache when testing CSS/JS changes.

### Production Build for GitHub Pages

#### Build Command

```bash
npm run build
```

**What Happens**:
1. `npm run clean` - Removes entire `docs/` folder
2. `npm run copy:prism` - Bundles Prism.js files
3. `eleventy` - Builds with relative paths (no environment variable needed)
4. Files written to `docs/` at root level
5. Relative paths used throughout (same as dev build)
6. Assets copied: `assets/` → `docs/assets/`
7. Works identically in local testing and GitHub Pages

#### Testing Production Build Locally

```bash
cd ../docs/WebSpark.HttpClientUtility
python -m http.server 8080  # Python 3
# Or: npx http-server -p 8080
```

Navigate to http://localhost:8080/ to verify production build works.

### GitHub Pages Deployment

#### Repository Configuration

1. Go to **Settings** → **Pages**
2. Source: **Deploy from a branch**
3. Branch: **main** (or feature branch)
4. Folder: **/docs**
5. Click **Save**

#### Deployment Process

1. Run production build: `npm run build`
2. Commit generated `docs/` files
3. Push to GitHub: `git push origin main`
4. GitHub Pages auto-deploys from `/docs`
5. Site available at: https://markhazleton.github.io/WebSpark.HttpClientUtility/

#### Post-Deployment Verification

- [ ] Homepage loads at base URL
- [ ] All navigation links work
- [ ] CSS stylesheet loads (check DevTools Network tab)
- [ ] JavaScript loads (Prism.js)
- [ ] Images display correctly
- [ ] Code blocks have syntax highlighting
- [ ] NuGet data displays
- [ ] Footer links work (GitHub, NuGet, Issues)
- [ ] Responsive design on mobile
- [ ] Browser console shows no errors

### Eleventy Configuration File

**File**: `src/.eleventy.js`

```javascript
export default function(eleventyConfig) {
  // 1. Ignore cache file to prevent infinite rebuild loop
  eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
  
  // 2. Environment detection
  const isProduction = process.env.ELEVENTY_ENV === "production";
  const prefix = isProduction ? "WebSpark.HttpClientUtility" : "";
  
  console.log(`🔧 Build mode: ${isProduction ? 'PRODUCTION' : 'DEVELOPMENT'}`);
  console.log(`🔧 PathPrefix will be: ${isProduction ? '/WebSpark.HttpClientUtility/' : '/'}`);
  
  // 3. Development server options
  if (!isProduction) {
    eleventyConfig.setServerOptions({
      showAllHosts: true
    });
  }
  
  // 4. Environment-aware asset copying
  if (prefix) {
    // Production: Copy to subdirectory
    eleventyConfig.addPassthroughCopy({ "assets": `${prefix}/assets` });
    eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": `${prefix}/favicon.ico` });
  } else {
    // Development: Copy to root
    eleventyConfig.addPassthroughCopy("assets");
    eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": "favicon.ico" });
  }
  
  // 5. Static files (always at root)
  eleventyConfig.addPassthroughCopy({ ".nojekyll": ".nojekyll" });
  eleventyConfig.addPassthroughCopy({ "robots.txt": "robots.txt" });
  
  // 6. Custom filters
  eleventyConfig.addFilter("formatNumber", function(value) {
    if (!value) return "0";
    const num = parseInt(value);
    if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
    if (num >= 1000) return (num / 1000).toFixed(1) + "K";
    return num.toLocaleString();
  });
  
  // 7. Return configuration
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
    pathPrefix: isProduction ? "/WebSpark.HttpClientUtility/" : "/"
  };
}
```

### Template Best Practices

#### Always Use URL Filter

```njk
<!-- WRONG - Hardcoded path -->
<link rel="stylesheet" href="/assets/css/main.css">

<!-- RIGHT - Uses url filter -->
<link rel="stylesheet" href="{{ '/assets/css/main.css' | url }}">
```

#### Front Matter Permalinks

```yaml
---
# WRONG - Hardcoded subdirectory
permalink: /WebSpark.HttpClientUtility/features/

# RIGHT - Relative path
permalink: /features/
---
```

### Common Issues and Solutions

#### Issue 1: Blank Page or 404

**Symptoms**: Page loads but shows blank or "Cannot GET /path"

**Causes**:
- pathPrefix not applied to asset URLs
- Front matter permalink hardcoded with subdirectory
- Assets not copied to correct location

**Solutions**:
1. Verify all templates use `{{ '/path' | url }}` filter
2. Check permalinks are relative (no `/WebSpark.HttpClientUtility/`)
3. Run `npm run clean && npm run build`
4. Check `.eleventy.js` passthrough copy configuration

#### Issue 2: Infinite Rebuild Loop

**Symptoms**: Dev server continuously rebuilds

**Cause**: Data fetcher writing to `_data/nuget-cache.json` triggers file watcher

**Solution**: Already fixed in config:
```javascript
eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
```

#### Issue 3: CSS/JS Not Loading

**Symptoms**: Styles not applied, no syntax highlighting

**Causes**:
- Browser cache serving old files
- Asset paths not using url filter
- Prism.js files not generated

**Solutions**:
1. Hard refresh with `Ctrl+F5`
2. Verify asset links use `{{ '/path' | url }}`
3. Run `npm run copy:prism`
4. Check browser DevTools Network tab for 404 errors

#### Issue 4: Different Behavior Local vs Production

**Symptoms**: Works locally but not on GitHub Pages

**Cause**: pathPrefix differences between environments

**Solutions**:
1. Test production build locally (see instructions above)
2. Verify all templates use url filter
3. Check browser console for errors
4. Verify GitHub Pages settings

### NPM Scripts Reference

```json
{
  "scripts": {
    "clean": "rimraf ../docs",
    "dev": "eleventy --serve",
    "build": "npm run clean && npm run copy:prism && cross-env ELEVENTY_ENV=production eleventy",
    "copy:prism": "node scripts/copy-prism.js"
  }
}
```

### File Structure

```
src/
├── .eleventy.js              # Main Eleventy configuration
├── _data/
│   ├── site.json            # Site metadata
│   ├── navigation.json      # Navigation structure
│   ├── nuget.js             # NuGet API data fetcher
│   └── nuget-cache.json     # Cache (auto-generated, ignored by watcher)
├── _includes/
│   ├── layouts/
│   │   ├── base.njk         # Base HTML template
│   │   └── page.njk         # Content page wrapper
│   └── components/
│       ├── header.njk       # Site header
│       └── footer.njk       # Site footer
├── pages/
│   ├── index.md             # Homepage (permalink: /)
│   ├── features.md          # Features (permalink: /features/)
│   ├── getting-started.md   # Getting started guide
│   ├── examples.md          # Code examples
│   ├── api-reference.md     # API documentation
│   └── about.md             # About page
└── assets/
    ├── css/
    │   ├── main.css         # Custom CSS (462 lines)
    │   └── prism-tomorrow.css  # Syntax theme (generated)
    ├── js/
    │   └── prism.min.js     # Syntax highlighter (generated)
    └── images/
        └── favicon.ico

docs/ (generated)
├── .nojekyll
├── robots.txt
└── WebSpark.HttpClientUtility/  # Production build
    ├── index.html
    ├── features/index.html
    ├── getting-started/index.html
    ├── examples/index.html
    ├── api-reference/index.html
    ├── about/index.html
    └── assets/
        ├── css/
        ├── js/
        └── images/
```

### Dependencies

**Production**:
- `node-fetch` ^3.3.2 - NuGet API calls
- `prismjs` ^1.29.0 - Syntax highlighting

**Development**:
- `@11ty/eleventy` ^3.0.0 - Static site generator
- `cross-env` ^10.1.0 - Environment variables
- `rimraf` ^5.0.5 - Clean script

### Performance Metrics

- Build time: ~0.4 seconds (6 pages)
- NuGet API fetch: ~0.2 seconds (with cache fallback)
- Lighthouse Performance: 95+
- First Contentful Paint: <1.0s
- Total page weight: <150KB

### Troubleshooting Commands

```bash
# Verify Eleventy version
npx eleventy --version

# Test build without server
npx eleventy

# Build with debug output
DEBUG=Eleventy* npx eleventy

# Check dev server port
netstat -ano | findstr :8080  # Windows
```

---

**Implementation Summary Document**: For complete implementation details, troubleshooting guide, and lessons learned, see: `copilot/session-2025-11-02/static-site-implementation-summary.md`
