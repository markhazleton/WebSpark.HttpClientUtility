# Contract: .eleventy.js Configuration

**Location**: `/src/.eleventy.js`  
**Purpose**: Eleventy configuration for static site generation

## Required Structure

```javascript
module.exports = function(eleventyConfig) {
  
  // ============================================================
  // PASSTHROUGH COPIES (Assets that bypass processing)
  // ============================================================
  
  // Copy assets directory
  eleventyConfig.addPassthroughCopy("assets");
  
  // Copy .nojekyll file to disable GitHub Pages Jekyll processing
  eleventyConfig.addPassthroughCopy({ ".nojekyll": ".nojekyll" });
  
  // Copy robots.txt
  eleventyConfig.addPassthroughCopy({ "robots.txt": "robots.txt" });
  
  // Copy favicon
  eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": "favicon.ico" });
  
  
  // ============================================================
  // FILTERS (Template utilities)
  // ============================================================
  
  // Format number with K/M suffix (e.g., 15234 → "15.2K")
  eleventyConfig.addFilter("formatNumber", function(value) {
    if (!value) return "0";
    const num = parseInt(value);
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + "M";
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1) + "K";
    }
    return num.toLocaleString();
  });
  
  // Format date for display (ISO → "Nov 2, 2025")
  eleventyConfig.addFilter("formatDate", function(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", { 
      year: "numeric", 
      month: "short", 
      day: "numeric" 
    });
  });
  
  // Convert to ISO date for <time> element
  eleventyConfig.addFilter("dateISO", function(dateString) {
    if (!dateString) return "";
    return new Date(dateString).toISOString();
  });
  
  // Limit array length (for truncating lists)
  eleventyConfig.addFilter("limit", function(array, limit) {
    return array.slice(0, limit);
  });
  
  
  // ============================================================
  // SHORTCODES (Reusable components)
  // ============================================================
  
  // Code example shortcode (wraps Prism.js highlighting)
  eleventyConfig.addPairedShortcode("codeExample", function(content, language, caption = "") {
    const captionHtml = caption ? `<div class="code-caption">${caption}</div>` : "";
    return `${captionHtml}<pre><code class="language-${language}">${content}</code></pre>`;
  });
  
  // NuGet badge shortcode
  eleventyConfig.addShortcode("nugetBadge", function(nugetData) {
    return `
      <div class="nuget-badge">
        <span class="badge-version">v${nugetData.version}</span>
        <span class="badge-downloads">${nugetData.displayDownloads || nugetData.downloads} downloads</span>
      </div>
    `;
  });
  
  // Install command shortcode
  eleventyConfig.addShortcode("installCommand", function(packageName) {
    return `<code class="install-command">dotnet add package ${packageName}</code>`;
  });
  
  
  // ============================================================
  // COLLECTIONS (Content groupings)
  // ============================================================
  
  // Features collection (sorted by order)
  eleventyConfig.addCollection("features", function(collectionApi) {
    return collectionApi.getFilteredByTag("feature").sort((a, b) => {
      return (a.data.order || 999) - (b.data.order || 999);
    });
  });
  
  // Examples collection
  eleventyConfig.addCollection("examples", function(collectionApi) {
    return collectionApi.getFilteredByGlob("pages/examples/**/*.md").sort((a, b) => {
      return (a.data.order || 999) - (b.data.order || 999);
    });
  });
  
  // API reference collection (organized by category)
  eleventyConfig.addCollection("apiReference", function(collectionApi) {
    const apiPages = collectionApi.getFilteredByGlob("pages/api-reference/**/*.md");
    
    // Group by category
    const byCategory = {};
    apiPages.forEach(page => {
      const category = page.data.category || "Uncategorized";
      if (!byCategory[category]) {
        byCategory[category] = [];
      }
      byCategory[category].push(page);
    });
    
    // Sort within each category
    Object.keys(byCategory).forEach(category => {
      byCategory[category].sort((a, b) => a.data.title.localeCompare(b.data.title));
    });
    
    return byCategory;
  });
  
  
  // ============================================================
  // MARKDOWN CONFIGURATION
  // ============================================================
  
  const markdownIt = require("markdown-it");
  const markdownItAnchor = require("markdown-it-anchor");
  
  const markdownLibrary = markdownIt({
    html: true,           // Allow HTML in Markdown
    breaks: false,        // Convert \n to <br>
    linkify: true         // Auto-convert URLs to links
  }).use(markdownItAnchor, {
    permalink: markdownItAnchor.permalink.ariaHidden({
      placement: "after",
      class: "header-anchor",
      symbol: "#",
      level: [2, 3, 4]    // Add anchors to h2, h3, h4
    })
  });
  
  eleventyConfig.setLibrary("md", markdownLibrary);
  
  
  // ============================================================
  // WATCH TARGETS (Files to monitor in dev mode)
  // ============================================================
  
  eleventyConfig.addWatchTarget("assets/css/**/*.css");
  eleventyConfig.addWatchTarget("assets/js/**/*.js");
  
  
  // ============================================================
  // BROWSERSYNC CONFIGURATION (Dev server)
  // ============================================================
  
  eleventyConfig.setBrowserSyncConfig({
    ui: false,            // Disable BrowserSync UI
    ghostMode: false,     // Disable synchronized browsing
    port: 8080,
    callbacks: {
      ready: function(err, bs) {
        // Custom 404 page handling
        bs.addMiddleware("*", (req, res) => {
          const content_404 = fs.readFileSync("../docs/404.html");
          res.writeHead(404, { "Content-Type": "text/html; charset=UTF-8" });
          res.write(content_404);
          res.end();
        });
      }
    }
  });
  
  
  // ============================================================
  // RETURN CONFIGURATION
  // ============================================================
  
  return {
    dir: {
      input: ".",           // Current directory (src/)
      output: "../docs",    // Build output (relative to src/)
      includes: "_includes", // Templates and layouts
      data: "_data"         // Global data files
    },
    
    // Template formats to process
    templateFormats: ["md", "njk", "html"],
    
    // Use Nunjucks for Markdown files
    markdownTemplateEngine: "njk",
    htmlTemplateEngine: "njk",
    dataTemplateEngine: "njk",
    
    // Path prefix (empty for GitHub Pages user site, or repo name for project site)
    pathPrefix: "/"
  };
};
```

## Contract Rules

### Required Components

**MUST HAVE**:
1. **Passthrough Copies**:
   - `assets` directory (CSS, JS, images)
   - `.nojekyll` file (disables Jekyll on GitHub Pages)
   - `favicon.ico` at root

2. **Filters**:
   - `formatNumber`: Format large numbers (15234 → "15.2K")
   - `formatDate`: Human-readable dates
   - `dateISO`: ISO 8601 dates for `<time>` elements

3. **Collections**:
   - `features`: Sorted feature pages
   - `examples`: Example code pages
   - `apiReference`: API documentation pages

4. **Directory Configuration**:
   - `input`: Current directory (`.` relative to src/)
   - `output`: `../docs` (relative to src/)
   - `includes`: `_includes`
   - `data`: `_data`

5. **Template Formats**:
   - `md`: Markdown files
   - `njk`: Nunjucks templates
   - `html`: HTML files

**OPTIONAL BUT RECOMMENDED**:
- Shortcodes for reusable components
- Markdown-it plugins for enhanced Markdown
- BrowserSync customization
- Watch targets for asset changes

### Path Configuration

**CRITICAL**: All paths are relative to `/src/` directory:

```javascript
{
  input: ".",           // /src/ (current directory)
  output: "../docs",    // /docs/ (parent directory)
  includes: "_includes", // /src/_includes/
  data: "_data"         // /src/_data/
}
```

**DO NOT** use absolute paths or change output to anything other than `../docs`.

### Markdown Configuration

**Plugins**:
- `markdown-it-anchor`: Adds anchor links to headings
  - Required for table of contents functionality
  - Accessible via `aria-hidden` pattern

**Options**:
- `html: true`: Allow HTML in Markdown (for complex layouts)
- `linkify: true`: Auto-convert URLs to clickable links

### Collections Sorting

**Features Collection**:
- MUST sort by `order` field in front matter
- MUST handle missing `order` (default to 999)

**API Reference Collection**:
- MUST group by `category` field
- MUST sort alphabetically within category

### Filters

**formatNumber**:
- Input: Integer
- Output: String with K/M suffix
- Examples: `15234 → "15.2K"`, `1500000 → "1.5M"`

**formatDate**:
- Input: ISO 8601 date string
- Output: "Month Day, Year" (e.g., "Nov 2, 2025")
- Locale: `en-US`

### BrowserSync Configuration

**Dev Server**:
- Port: `8080` (default)
- UI: Disabled (not needed for docs)
- Ghost Mode: Disabled (no synchronized browsing)
- Custom 404 handling (serves /404.html)

### Environment-Specific Configuration

**Production Mode** (NODE_ENV=production):
```javascript
const isProduction = process.env.NODE_ENV === "production";

if (isProduction) {
  // Add minification plugins
  // Disable source maps
  // Enable aggressive caching
}
```

## Validation

### Pre-Build Checks

```bash
# Verify configuration syntax
node -c .eleventy.js

# Test configuration loads
npx eleventy --dryrun

# Verify output directory
npx eleventy --help | grep "Output"
```

### Build Validation

```bash
# Build should complete without errors
npm run build

# Verify /docs directory created
ls -la ../docs/

# Verify key files exist
ls ../docs/index.html
ls ../docs/.nojekyll
ls ../docs/favicon.ico
```

### Common Issues

**Issue**: Build fails with "ENOENT: no such file or directory '../docs'"
- **Solution**: Run `npm run clean` first, or remove `rimraf` error check

**Issue**: Assets not copied to /docs/assets/
- **Solution**: Verify `addPassthroughCopy("assets")` present

**Issue**: .nojekyll missing from /docs/
- **Solution**: Add passthrough copy for `.nojekyll`

**Issue**: Markdown not processing Nunjucks syntax
- **Solution**: Verify `markdownTemplateEngine: "njk"`

## Dependencies

**Required npm packages**:
```json
{
  "devDependencies": {
    "@11ty/eleventy": "^3.0.0",
    "markdown-it": "^13.0.0",
    "markdown-it-anchor": "^8.6.0"
  }
}
```

**Installation**:
```bash
npm install --save-dev @11ty/eleventy markdown-it markdown-it-anchor
```

## Performance Considerations

**Incremental Builds**:
- Use `--incremental` flag for faster rebuilds during development
- Only processes changed files
- Not recommended for production builds

**Watch Performance**:
- Limit watch targets to frequently changed files
- CSS and JS in `assets/` directory only
- Markdown files watched by default

## Breaking Changes

**MAJOR**:
- Change output directory from `../docs`
- Remove required filter or collection
- Change template format defaults

**MINOR**:
- Add new optional filter, shortcode, or collection
- Add new Markdown plugin
- Add new passthrough copy

**PATCH**:
- Fix filter logic
- Improve collection sorting
- Update Markdown-it options

## Related Contracts

- [package-json.md](./package-json.md) - NPM configuration
- [data-model.md](../data-model.md) - Content structure
- [github-actions.md](./github-actions.md) - CI/CD pipeline
