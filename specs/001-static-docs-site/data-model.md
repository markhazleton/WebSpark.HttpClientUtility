# Data Model: Static Documentation Website

**Feature**: Static Documentation Website for GitHub Pages  
**Date**: 2025-11-02  
**Status**: Phase 1 Design

## Overview

This document defines the content structure, navigation model, and data relationships for the WebSpark.HttpClientUtility documentation website. The model is designed for Eleventy's data cascade and template system.

---

## Core Entities

### 1. Site Metadata (Global)

**Source**: `/src/_data/site.json`

**Purpose**: Global site configuration available to all templates

**Structure**:
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
  },
  "social": {
    "github": "markhazleton/WebSpark.HttpClientUtility"
  },
  "build": {
    "timestamp": "{{ generated at build time }}",
    "environment": "production"
  }
}
```

**Validation Rules**:
- All URLs must be absolute (https://)
- `name` must match NuGet package ID exactly
- `url` must match GitHub Pages domain

---

### 2. NuGet Package Data (Dynamic)

**Source**: `/src/_data/nuget.js` (Eleventy JavaScript data file)

**Purpose**: Live package statistics fetched from NuGet API at build time

**Structure**:
```javascript
// Output structure from nuget.js
{
  version: "1.4.0",                    // Current package version
  downloads: 15234,                    // Total download count
  description: "A production-ready...", // Package description
  lastUpdate: "2025-11-02T10:30:00Z",  // ISO timestamp of last fetch
  cached: false,                        // true if using cached data
  error: false,                         // true if API failed and using fallback
  displayDownloads: "15.2K",           // Formatted download count
  cacheTimestamp: "Nov 2, 2025"        // Human-readable cache date
}
```

**Validation Rules**:
- `version` must match SemVer format (X.Y.Z)
- `downloads` must be non-negative integer
- `lastUpdate` must be valid ISO 8601 timestamp
- If `cached: true`, display cache timestamp on site

**Relationships**:
- Referenced in homepage hero section
- Referenced in footer (version number)
- Referenced in getting started guide (installation command)

**Update Frequency**: Every build (with cache fallback)

---

### 3. Navigation Structure

**Source**: `/src/_data/navigation.json`

**Purpose**: Site navigation menu structure

**Structure**:
```json
{
  "main": [
    {
      "label": "Home",
      "url": "/",
      "order": 1
    },
    {
      "label": "Features",
      "url": "/features/",
      "order": 2,
      "children": [
        {
          "label": "Caching",
          "url": "/features/caching/"
        },
        {
          "label": "Resilience",
          "url": "/features/resilience/"
        },
        {
          "label": "Telemetry",
          "url": "/features/telemetry/"
        },
        {
          "label": "Web Crawling",
          "url": "/features/web-crawling/"
        }
      ]
    },
    {
      "label": "Getting Started",
      "url": "/getting-started/",
      "order": 3
    },
    {
      "label": "API Reference",
      "url": "/api-reference/",
      "order": 4
    },
    {
      "label": "Examples",
      "url": "/examples/",
      "order": 5
    },
    {
      "label": "About",
      "url": "/about/",
      "order": 6,
      "children": [
        {
          "label": "Contributing",
          "url": "/about/contributing/"
        },
        {
          "label": "Changelog",
          "url": "/about/changelog/"
        }
      ]
    }
  ],
  "footer": [
    {
      "label": "GitHub",
      "url": "https://github.com/markhazleton/WebSpark.HttpClientUtility",
      "external": true
    },
    {
      "label": "NuGet",
      "url": "https://www.nuget.org/packages/WebSpark.HttpClientUtility",
      "external": true
    },
    {
      "label": "Issues",
      "url": "https://github.com/markhazleton/WebSpark.HttpClientUtility/issues",
      "external": true
    }
  ]
}
```

**Validation Rules**:
- `order` must be unique within same level
- `url` for internal links must start with `/`
- `url` for external links must be absolute (https://)
- `external: true` required for external links (opens in new tab)
- Maximum 2 levels of nesting (main → children only)

**Relationships**:
- Consumed by header component for main navigation
- Consumed by footer component for secondary links
- Used for active link highlighting (current page detection)

---

### 4. Documentation Page

**Source**: Markdown files in `/src/pages/**/*.md`

**Purpose**: Individual content pages

**Front Matter Structure**:
```yaml
---
layout: layouts/page.njk          # Template layout
title: Page Title                 # H1 and browser title
description: SEO description      # Meta description
eleventyNavigation:               # Optional: Eleventy Navigation plugin
  key: page-key                   # Unique identifier
  order: 1                        # Order in collection
  parent: parent-key              # Parent page (for breadcrumbs)
permalink: /custom-url/           # Optional: Override default URL
tags:                             # Optional: For collections
  - feature
  - advanced
date: 2025-11-02                  # Optional: Publication date
---

# {{ title }}

Content here in Markdown...
```

**Validation Rules**:
- `title` required, max 70 characters (SEO)
- `description` required, 120-160 characters (SEO)
- `layout` must reference existing template in `_includes/layouts/`
- `permalink` must be unique across all pages
- `title` in front matter should match first H1 in content

**Relationships**:
- Belongs to navigation structure (via eleventyNavigation)
- May reference code examples (via shortcodes)
- May reference NuGet data (via global data)

**Lifecycle**:
- Created: Manual authoring in Markdown
- Updated: Manual edits trigger rebuild
- Published: Eleventy transforms to HTML in /docs

---

### 5. Code Example

**Source**: Embedded in Markdown content or separate includes

**Purpose**: Syntax-highlighted code snippets

**Inline Structure** (Markdown):
````markdown
```csharp
// Example: Basic HTTP request
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.example.com/weather",
    RequestMethod = HttpMethod.Get
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```
````

**Component Structure** (Nunjucks shortcode):
```nunjucks
{% codeExample "csharp", "Basic HTTP Request" %}
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.example.com/weather",
    RequestMethod = HttpMethod.Get
};
{% endcodeExample %}
```

**Attributes**:
- `language`: Prism.js language identifier (csharp, javascript, json, powershell)
- `caption`: Optional descriptive title above code block
- `lineNumbers`: Boolean, show line numbers (default: true for >5 lines)
- `highlight`: Optional line ranges to emphasize (e.g., "3-5,8")

**Validation Rules**:
- `language` must be supported by Prism.js build
- Code must be syntactically valid (no compilation check, but should be accurate)
- Code should reference current library API (avoid outdated examples)

**Relationships**:
- Embedded in Documentation Pages
- Styled by Prism.js CSS theme
- May be extracted into separate files for testing (future enhancement)

---

### 6. Feature Description

**Source**: Content in `/src/pages/features/*.md`

**Purpose**: Detailed feature documentation

**Structure**:
```yaml
---
layout: layouts/feature.njk       # Feature-specific layout
title: Response Caching
description: Automatic in-memory caching of HTTP responses
icon: cache                       # Icon identifier
tags: [feature, performance]
order: 1                          # Display order on features page
relatedFeatures:                  # Cross-references
  - resilience
  - telemetry
---

## Overview
[Feature overview paragraph]

## Benefits
- Benefit 1
- Benefit 2

## Usage
[Code example]

## Configuration Options
[Table of options]

## Performance Impact
[Metrics or considerations]
```

**Validation Rules**:
- Must include: title, description, overview, benefits, usage example
- Configuration options must match actual library API
- Related features must reference valid feature keys

**Relationships**:
- Aggregated into Features overview page
- Cross-linked to related features
- Referenced in Getting Started guide

---

### 7. API Reference Entity

**Source**: `/src/pages/api-reference/*.md` (manually authored)

**Purpose**: API documentation for classes, interfaces, and methods

**Structure**:
```yaml
---
layout: layouts/api-reference.njk
title: HttpRequestResult<T>
description: Represents an HTTP request configuration and response container
category: Core Models
namespace: WebSpark.HttpClientUtility.RequestResult
assembly: WebSpark.HttpClientUtility
---

## Definition

```csharp
public class HttpRequestResult<T> where T : class
```

## Properties

### RequestPath
- **Type**: `string`
- **Required**: Yes
- **Description**: The full URL for the HTTP request

### RequestMethod
- **Type**: `HttpMethod`
- **Default**: `HttpMethod.Get`
- **Description**: The HTTP method (GET, POST, PUT, DELETE, etc.)

### CacheDurationMinutes
- **Type**: `int?`
- **Default**: `null`
- **Description**: Cache duration in minutes. If null, response is not cached.

[Additional properties...]

## Examples

[Usage examples]

## See Also
- [Related API entity]
```

**Validation Rules**:
- `namespace` and `assembly` must match actual library structure
- Property types must be valid C# types
- Examples must compile and run with current library version

**Relationships**:
- Organized by category (Core Models, Services, Configuration, Authentication)
- Cross-linked to related API entities
- Referenced in feature documentation

---

### 8. Comparison Table Entry

**Source**: `/src/_data/comparisons.json`

**Purpose**: Feature comparison data for alternatives table

**Structure**:
```json
{
  "features": [
    { "name": "Setup Complexity", "category": "Developer Experience" },
    { "name": "Built-in Caching", "category": "Features" },
    { "name": "Built-in Resilience", "category": "Features" },
    { "name": "Telemetry", "category": "Observability" },
    { "name": "Type Safety", "category": "Developer Experience" },
    { "name": "Web Crawling", "category": "Features" }
  ],
  "libraries": [
    {
      "name": "WebSpark.HttpClientUtility",
      "scores": {
        "Setup Complexity": { "value": "⭐ One line", "score": 5 },
        "Built-in Caching": { "value": "✅ Yes", "score": 5 },
        "Built-in Resilience": { "value": "✅ Yes", "score": 5 },
        "Telemetry": { "value": "✅ Built-in", "score": 5 },
        "Type Safety": { "value": "✅ Yes", "score": 5 },
        "Web Crawling": { "value": "✅ Yes", "score": 5 }
      }
    },
    {
      "name": "Raw HttpClient",
      "scores": {
        "Setup Complexity": { "value": "⭐⭐⭐ Manual", "score": 2 },
        "Built-in Caching": { "value": "❌ Manual", "score": 1 },
        "Built-in Resilience": { "value": "❌ Manual", "score": 1 },
        "Telemetry": { "value": "⚠️ Manual", "score": 2 },
        "Type Safety": { "value": "⚠️ Partial", "score": 3 },
        "Web Crawling": { "value": "❌ No", "score": 1 }
      }
    },
    {
      "name": "RestSharp",
      "scores": {
        "Setup Complexity": { "value": "⭐⭐ Low", "score": 4 },
        "Built-in Caching": { "value": "❌ Manual", "score": 1 },
        "Built-in Resilience": { "value": "❌ Manual", "score": 1 },
        "Telemetry": { "value": "⚠️ Manual", "score": 2 },
        "Type Safety": { "value": "✅ Yes", "score": 5 },
        "Web Crawling": { "value": "❌ No", "score": 1 }
      }
    },
    {
      "name": "Refit",
      "scores": {
        "Setup Complexity": { "value": "⭐⭐ Low", "score": 4 },
        "Built-in Caching": { "value": "⚠️ Plugin", "score": 3 },
        "Built-in Resilience": { "value": "❌ Manual", "score": 1 },
        "Telemetry": { "value": "⚠️ Manual", "score": 2 },
        "Type Safety": { "value": "✅ Yes", "score": 5 },
        "Web Crawling": { "value": "❌ No", "score": 1 }
      }
    }
  ]
}
```

**Validation Rules**:
- `score` must be 1-5 (1=poor, 5=excellent)
- `value` should be emoji + short text for readability
- All libraries must have scores for all features

**Relationships**:
- Displayed on homepage
- Referenced in Why Choose section

---

## Data Relationships Diagram

```
┌─────────────────┐
│  Site Metadata  │ (Global, static)
└────────┬────────┘
         │
         ├──> Referenced by all layouts (header, footer, meta tags)
         │
┌────────┴────────┐
│  NuGet Data     │ (Global, dynamic - fetched at build time)
└────────┬────────┘
         │
         ├──> Homepage (hero section, badges)
         ├──> Footer (version display)
         └──> Getting Started (installation command)

┌─────────────────┐
│  Navigation     │ (Global, static)
└────────┬────────┘
         │
         ├──> Header component (main menu)
         ├──> Footer component (secondary links)
         └──> Mobile menu (hamburger toggle)

┌─────────────────┐
│ Documentation   │ (Collection of Markdown files)
│     Pages       │
└────────┬────────┘
         │
         ├──> Layouts (base, page, feature, api-reference)
         ├──> Navigation (via eleventyNavigation plugin)
         │
         ├────┐
         │    ▼
         │  ┌─────────────────┐
         │  │  Code Examples  │ (Embedded in pages)
         │  └─────────────────┘
         │
         └────┐
              ▼
            ┌─────────────────┐
            │ Feature Docs    │ (Subset of pages)
            └────────┬────────┘
                     │
                     └──> Features overview page (aggregated)

┌─────────────────┐
│  Comparisons    │ (Static data)
└────────┬────────┘
         │
         └──> Homepage comparison table
```

---

## Collections and Aggregations

### Eleventy Collections

Collections group related content for iteration in templates.

**1. All Pages Collection**
```javascript
// Automatically created by Eleventy from all Markdown files
collections.all
```

**2. Features Collection**
```javascript
// Pages tagged with "feature"
eleventyConfig.addCollection("features", function(collectionApi) {
  return collectionApi.getFilteredByTag("feature").sort((a, b) => {
    return a.data.order - b.data.order;
  });
});
```

**3. Examples Collection**
```javascript
// Pages in /examples/ directory
eleventyConfig.addCollection("examples", function(collectionApi) {
  return collectionApi.getFilteredByGlob("src/pages/examples/**/*.md");
});
```

**4. API Reference Collection**
```javascript
// Pages in /api-reference/ directory, organized by category
eleventyConfig.addCollection("apiReference", function(collectionApi) {
  return collectionApi.getFilteredByGlob("src/pages/api-reference/**/*.md");
});
```

---

## Content Lifecycle and Update Triggers

### Build-Time Data Fetching

1. **Site Build Initiated** (via `npm run build` or GitHub Actions)
2. **Global Data Files Executed**:
   - `nuget.js` fetches from NuGet API
   - Successful response cached to `nuget-cache.json`
   - If API fails, load from cache
3. **Templates Rendered** with data context
4. **Assets Copied** to /docs
5. **Build Complete** → Ready for GitHub Pages deployment

### Content Update Workflows

**Scenario 1: Library Release** (e.g., v1.5.0 with new feature)
1. Update code examples in relevant pages
2. Add new feature page if major feature
3. Update API reference for new classes/methods
4. Update getting started if configuration changes
5. Run build → NuGet data automatically fetches new version

**Scenario 2: Documentation-Only Update**
1. Edit Markdown files
2. Commit to repository
3. GitHub Actions triggers build
4. Site updates automatically

**Scenario 3: Design/Layout Change**
1. Modify templates in `_includes/`
2. Update CSS in `assets/css/`
3. Test locally with `npm run dev`
4. Commit and deploy

---

## SEO and Metadata Model

### Per-Page Metadata

Generated from front matter + global data:

```html
<!-- In <head> of every page -->
<title>{{ title }} | {{ site.name }}</title>
<meta name="description" content="{{ description }}">
<meta property="og:title" content="{{ title }}">
<meta property="og:description" content="{{ description }}">
<meta property="og:url" content="{{ site.url }}{{ page.url }}">
<meta property="og:type" content="website">
<meta name="twitter:card" content="summary">
<meta name="twitter:title" content="{{ title }}">
<meta name="twitter:description" content="{{ description }}">
<link rel="canonical" href="{{ site.url }}{{ page.url }}">
```

### Sitemap Generation

Eleventy plugin generates sitemap.xml automatically:

```xml
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <url>
    <loc>{{ site.url }}{{ page.url }}</loc>
    <lastmod>{{ page.date | dateISO }}</lastmod>
    <changefreq>weekly</changefreq>
    <priority>{{ page.data.priority | default: 0.5 }}</priority>
  </url>
</urlset>
```

---

## Validation and Testing

### Content Validation

**Build-Time Checks**:
- [ ] All front matter required fields present
- [ ] All internal links resolve to existing pages
- [ ] All images referenced exist in assets/
- [ ] All code examples have valid language identifier

**Post-Build Checks**:
- [ ] HTML validates (W3C validator)
- [ ] No broken external links (hyperlink checker)
- [ ] All pages have unique titles and descriptions
- [ ] Sitemap.xml generated correctly

### Data Integrity

**NuGet Data Validation**:
```javascript
// In nuget.js
function validatePackageData(data) {
  if (!data.version || !/^\d+\.\d+\.\d+$/.test(data.version)) {
    throw new Error('Invalid version format');
  }
  if (typeof data.totalDownloads !== 'number' || data.totalDownloads < 0) {
    throw new Error('Invalid download count');
  }
  return true;
}
```

**Navigation Validation**:
- No duplicate URLs
- All URLs resolve (internal link check)
- No orphaned pages (not in navigation but exist)
- Maximum nesting level enforced (2 levels)

---

## Future Enhancements

### Versioned Documentation (Out of Scope for v1)

**Data Model Extension**:
```json
{
  "versions": [
    {
      "version": "1.4.0",
      "path": "/docs/v1.4/",
      "status": "current"
    },
    {
      "version": "1.3.0",
      "path": "/docs/v1.3/",
      "status": "archived"
    }
  ]
}
```

### Search Index (Out of Scope for v1)

**Data Structure**:
```json
{
  "searchIndex": [
    {
      "title": "Page Title",
      "url": "/page-url/",
      "content": "Searchable text content...",
      "keywords": ["http", "caching", "resilience"]
    }
  ]
}
```

### Multi-Language Support (Out of Scope for v1)

**Data Extension**:
```yaml
---
title: Getting Started
title_es: Primeros Pasos
description: Quick start guide
description_es: Guía de inicio rápido
---
```

---

## Summary

This data model provides:
- ✅ Clear separation of concerns (content, data, presentation)
- ✅ Eleventy-native patterns (global data, collections, front matter)
- ✅ Scalability for future enhancements
- ✅ Validation rules for data integrity
- ✅ Relationships between entities clearly defined

**Next Steps**: Generate file structure contracts in `/contracts/` directory.
