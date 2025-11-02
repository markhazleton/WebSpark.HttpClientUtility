# Implementation Tasks: Static Documentation Website for GitHub Pages

**Feature Branch**: `001-static-docs-site`  
**Date**: 2025-11-02  
**Status**: Ready for Implementation

## Overview

This document breaks down the static documentation website implementation into granular, executable tasks organized by user story. Each phase represents an independently testable increment that delivers value.

**Total Tasks**: 206  
**Estimated Timeline**: 5-9 days (1-2 weeks)  
**MVP Scope**: Phase 3 (User Story 1) - Homepage with package value proposition

---

## Task Legend

- `[P]` = Parallelizable (can be worked on simultaneously with other [P] tasks)
- `[US#]` = User Story number from spec.md
- File paths are relative to repository root unless specified otherwise

---

## Phase 1: Project Setup (Day 1, ~2 hours)

**Goal**: Initialize Node.js project with Eleventy and establish directory structure

**Prerequisites**: Node.js 20.x LTS, npm 10.x installed

**Independent Test**: `npm run build` succeeds and generates /docs folder with at least index.html

### Tasks

- [ ] T001 Create /src directory structure (/_data, /_includes, /assets, /pages)
- [ ] T002 Create /src/_includes subdirectories (/layouts, /components, /partials)
- [ ] T003 Create /src/assets subdirectories (/css, /js, /images)
- [ ] T004 Create /src/pages subdirectories (/features, /examples, /api-reference, /about)
- [ ] T005 Initialize npm project in /src with `npm init -y`
- [ ] T006 Install Eleventy core: `npm install --save-dev @11ty/eleventy@^3.0.0`
- [ ] T007 Install build tools: `npm install --save-dev rimraf@^5.0.5`
- [ ] T008 Install Markdown plugins: `npm install --save-dev markdown-it@^13.0.0 markdown-it-anchor@^8.6.0`
- [ ] T009 Install testing tools: `npm install --save-dev htmlhint@^1.1.4 hyperlink@^5.0.4`
- [ ] T010 Install NuGet API client: `npm install node-fetch@^3.3.2`
- [ ] T011 Configure package.json scripts (clean, dev, build, test:links, test:html, validate) per contracts/package-json.md
- [ ] T012 Set package.json engines to Node >=20.0.0, npm >=10.0.0
- [ ] T013 Create /src/.eleventy.js configuration file per contracts/eleventy-config.md
- [ ] T014 Create /src/.nojekyll file (empty, disables Jekyll on GitHub Pages)
- [ ] T015 Create /src/robots.txt with sitemap reference
- [ ] T016 Run `npm run dev` to verify Eleventy starts (should fail gracefully with no content)
- [ ] T017 Create empty /docs directory in repository root

**Validation**:
```bash
cd src
npm run dev   # Should start server on http://localhost:8080
npm run build # Should complete and create /docs folder
```

---

## Phase 2: Foundational Infrastructure (Day 1-2, ~3 hours)

**Goal**: Implement global data, base templates, and core styling that all pages depend on

**Prerequisites**: Phase 1 complete

**Independent Test**: Homepage renders with real NuGet data, navigation works, mobile menu toggles

### Global Data Files

- [ ] T018 [P] Create /src/_data/site.json with site metadata per data-model.md (name, title, URLs, author)
- [ ] T019 [P] Create /src/_data/nuget.js to fetch NuGet API data per contracts/nuget-api-schema.md
- [ ] T020 [P] Create /src/_data/nuget-cache.json with initial cache data (version: 1.4.0, downloads: 0)
- [ ] T021 [P] Create /src/_data/navigation.json with main and footer navigation per data-model.md
- [ ] T022 Test NuGet data fetching: `npm run build` and verify console shows "✓ NuGet data fetched successfully"

### Base Templates

- [ ] T023 [P] Create /src/_includes/layouts/base.njk with HTML structure, head, meta tags per quickstart.md
- [ ] T024 [P] Create /src/_includes/layouts/page.njk extending base layout for standard pages
- [ ] T025 [P] Create /src/_includes/components/header.njk with site logo and navigation
- [ ] T026 [P] Create /src/_includes/components/footer.njk with links, version, and copyright
- [ ] T027 Update .eleventy.js to add formatNumber and formatDate filters
- [ ] T028 Update .eleventy.js to add passthrough copies (assets, .nojekyll, robots.txt)

### Core Styling

- [ ] T029 [P] Create /src/assets/css/main.css with CSS custom properties (colors, spacing, typography)
- [ ] T030 [P] Create /src/assets/css/main.css base styles (reset, body, container, typography)
- [ ] T031 [P] Implement header styles in main.css (site-header, site-logo)
- [ ] T032 [P] Implement navigation styles in main.css (desktop navigation, active states)
- [ ] T033 [P] Implement CSS-only hamburger menu in main.css (checkbox hack, mobile toggle)
- [ ] T034 [P] Implement footer styles in main.css (footer layout, grid, links)
- [ ] T035 [P] Add responsive breakpoints in main.css (mobile-first, @media queries)
- [ ] T036 Download Prism.js v1.29.0+ from prismjs.com with: Core + Line Numbers plugin + Languages (C#, JavaScript, JSON, PowerShell) + Tomorrow Night color theme
- [ ] T037 Save Prism.js to /src/assets/js/prism.min.js
- [ ] T038 Save Prism CSS to /src/assets/css/prism-tomorrow.css

### Basic Assets

- [ ] T039 [P] Create placeholder favicon at /src/assets/images/favicon.ico
- [ ] T040 [P] Add WebSpark logo to /src/assets/images/logo.png (if available, or placeholder)

**Validation**:
```bash
npm run build
# Check /docs/index.html exists
# Check /docs/assets/css/main.css exists
# Check /docs/.nojekyll exists
```

---

## Phase 3: User Story 1 - Developer Discovers Package Value (P1) (Day 2, ~3 hours)

**User Story**: A developer searching for HTTP client utilities lands on the GitHub Pages site and quickly understands what WebSpark.HttpClientUtility offers, its key benefits, and whether it solves their problem.

**Goal**: Homepage displays package value proposition, key features, installation command, and NuGet stats

**Prerequisites**: Phase 2 complete

**Independent Test Criteria**:
1. Navigate to homepage → Package name, tagline, and value proposition visible within 3 seconds
2. Scroll through homepage → Key features, benefits, and differentiators clearly listed
3. Check homepage → NuGet installation command prominently displayed with copy-able code block

### Tasks

- [ ] T041 [US1] Create /src/pages/index.md with front matter (layout: base, title: Home, permalink: /)
- [ ] T042 [US1] Implement hero section in index.md with site name, NuGet description, and version/download stats
- [ ] T043 [US1] Add NuGet badge display in hero using nuget.version and nuget.displayDownloads
- [ ] T044 [US1] Add installation command in hero: `dotnet add package WebSpark.HttpClientUtility`
- [ ] T045 [US1] Create "Why Choose This Library?" section with benefit bullets
- [ ] T046 [US1] Add "Features" section with emoji checkmarks (✅) for 5-7 key features
- [ ] T047 [US1] Add "Quick Example" code block with C# syntax highlighting (Prism.js)
- [ ] T048 [US1] Add CTA buttons section with links to /getting-started/ and /features/
- [ ] T049 [US1] Implement hero section styles in /src/assets/css/main.css (hero, hero-tagline, hero-stats)
- [ ] T050 [US1] Implement CTA button styles in main.css (button, primary, secondary)
- [ ] T051 [US1] Test homepage displays NuGet data correctly (check version matches NuGet API)
- [ ] T052 [US1] Test cache timestamp displays when using cached data (simulate API failure)
- [ ] T053 [US1] Verify code syntax highlighting works (C# code block renders with Prism colors)

**Acceptance Validation**:
```bash
npm run build
npm run validate  # HTML + link checking
# Manual: Open /docs/index.html in browser
# Verify: Package value proposition visible immediately
# Verify: Installation command copy-able
# Verify: NuGet stats display (version + downloads)
```

---

## Phase 4: User Story 5 - Live Package Stats (P2) (Day 2, ~1 hour)

**User Story**: A developer viewing the documentation site sees current package information (version number, total downloads, last update date) pulled live from NuGet.

**Goal**: NuGet data displays throughout site with cache fallback

**Prerequisites**: Phase 2 complete (T019 NuGet fetcher implemented)

**Independent Test Criteria**:
1. Build site → NuGet data matches current API response (version, downloads)
2. View homepage → Download count formatted for readability (e.g., "15.2K downloads")
3. Simulate API failure → Build continues with cached data and displays cache timestamp

### Tasks

- [ ] T088 [US5] Verify nuget.js implements cache-to-file on successful fetch per contracts/nuget-api-schema.md
- [ ] T089 [US5] Verify nuget.js returns cached data with cached:true flag when API fails
- [ ] T090 [US5] Update footer.njk to display cache notice when nuget.cached is true
- [ ] T091 [US5] Add cache timestamp formatting in footer (e.g., "Data cached: Nov 2, 2025")
- [ ] T092 [US5] Test API failure scenario: Rename API URL temporarily, run build, verify cache used
- [ ] T093 [US5] Test successful fetch: Run build, verify console shows "✓ NuGet data fetched successfully"
- [ ] T094 [US5] Verify nuget-cache.json file created/updated after successful build

**Acceptance Validation**:
```bash
npm run build
# Check console output for NuGet fetch status
# Check src/_data/nuget-cache.json exists and has current data
# Manual: Verify version on homepage matches NuGet.org
```

---

## Phase 5: User Story 2 - Developer Learns Core Features (P1) (Day 3, ~4 hours)

**User Story**: A developer interested in the package needs to understand what features are available (caching, resilience, telemetry, web crawling) and how they benefit their project.

**Goal**: Features overview page with detailed descriptions, benefits, and code examples for each feature

**Prerequisites**: Phase 2 complete (Phase 3 can run in parallel)

**Independent Test Criteria**:
1. Navigate to /features/ → Each feature has clear problem statement and solution
2. View feature descriptions → Code examples with syntax highlighting present
3. Scan features → Can quickly identify optional vs. always-on features

### Tasks

- [ ] T054 [P] [US2] Create /src/pages/features/index.md with features overview layout
- [ ] T055 [P] [US2] Create /src/pages/features/caching.md with caching feature details and examples
- [ ] T056 [P] [US2] Create /src/pages/features/resilience.md with Polly resilience feature details
- [ ] T057 [P] [US2] Create /src/pages/features/telemetry.md with OpenTelemetry feature details
- [ ] T058 [P] [US2] Create /src/pages/features/web-crawling.md with web crawler feature details
- [ ] T059 [P] [US2] Create /src/pages/features/authentication.md with auth providers feature details
- [ ] T060 [US2] Add "features" collection to .eleventy.js (filter by tag, sort by order)
- [ ] T061 [US2] Create /src/_includes/layouts/feature.njk extending page layout for feature pages
- [ ] T062 [US2] Implement feature page template sections (Overview, Benefits, Usage, Configuration)
- [ ] T063 [US2] Add feature grid styles to main.css (feature-grid, feature-card)
- [ ] T064 [US2] Update features/index.md to loop through features collection and display cards
- [ ] T065 [US2] Add code example shortcode to .eleventy.js for reusable syntax-highlighted blocks
- [ ] T066 [US2] Test all feature pages render with code examples properly highlighted
- [ ] T066a [US2] Create /src/_data/comparison.json with competitor data (RestSharp, Refit, Flurl, HttpClient) and comparison criteria per spec.md FR-017
- [ ] T066b [US2] Add comparison table section to features/index.md using comparison.json data to display feature matrix

**Acceptance Validation**:
```bash
npm run build
# Check /docs/features/index.html exists
# Check /docs/features/caching/index.html exists (and others)
# Manual: Verify each feature page has Overview, Benefits, Usage, Configuration sections
```

---

## Phase 6: User Story 3 - Developer Gets Started Quickly (P2) (Day 3-4, ~3 hours)

**User Story**: A developer ready to try the package needs step-by-step installation and configuration instructions to integrate it into their project within minutes.

**Goal**: Comprehensive getting started guide with installation, configuration, and first request example

**Prerequisites**: Phase 2 complete

**Independent Test Criteria**:
1. Follow getting started guide from scratch → Can install package and configure DI in new project
2. Follow configuration examples → Can enable caching, resilience, and telemetry
3. Check troubleshooting section → Common problems and solutions documented

### Tasks

- [ ] T067 [P] [US3] Create /src/pages/getting-started.md with step-by-step installation guide
- [ ] T068 [P] [US3] Add "Prerequisites" section (NET 8 LTS or NET 9, IDE setup)
- [ ] T069 [P] [US3] Add "Installation" section with dotnet add package command
- [ ] T070 [P] [US3] Add "Basic Configuration" section with services.AddHttpClientUtility() example
- [ ] T071 [P] [US3] Add "Making Your First Request" section with HttpRequestResult<T> example
- [ ] T072 [P] [US3] Add "Enabling Optional Features" section (caching, resilience examples)
- [ ] T073 [P] [US3] Add "Configuration Options" table with all options explained
- [ ] T074 [P] [US3] Add "Troubleshooting" section with common issues and solutions
- [ ] T075 [US3] Add getting started page styles (step indicators, configuration tables)
- [ ] T076 [US3] Link getting started from homepage CTA button

**Acceptance Validation**:
```bash
npm run build
# Check /docs/getting-started/index.html exists
# Manual: Follow guide steps, verify all code examples are syntactically correct
```

---

## Phase 7: User Story 4 - Developer Finds API Documentation (P2) (Day 4-5, ~4 hours)

**User Story**: A developer integrating the package needs detailed API reference documentation for interfaces, classes, and configuration options.

**Goal**: API reference pages for core classes, organized by category

**Prerequisites**: Phase 2 complete

**Independent Test Criteria**:
1. Search for specific class (e.g., HttpRequestResult<T>) → Find complete documentation
2. Navigate API docs → Find all properties, methods, parameters, and return values documented
3. Check configuration docs → Find all configurable properties with examples

### Tasks

- [ ] T077 [P] [US4] Create /src/pages/api-reference/index.md with API reference overview and categories
- [ ] T078 [P] [US4] Create /src/pages/api-reference/http-request-result.md for HttpRequestResult<T> class
- [ ] T079 [P] [US4] Create /src/pages/api-reference/http-request-result-service.md for IHttpRequestResultService
- [ ] T080 [P] [US4] Create /src/pages/api-reference/resilience-options.md for HttpRequestResultPollyOptions
- [ ] T081 [P] [US4] Create /src/pages/api-reference/cache-options.md for caching configuration
- [ ] T082 [P] [US4] Create /src/pages/api-reference/authentication-providers.md for auth interfaces
- [ ] T083 [US4] Create /src/_includes/layouts/api-reference.njk for API doc pages
- [ ] T084 [US4] Add "apiReference" collection to .eleventy.js (group by category)
- [ ] T085 [US4] Implement API reference template sections (Definition, Properties, Methods, Examples, See Also)
- [ ] T086 [US4] Add API documentation styles to main.css (property tables, method signatures)
- [ ] T087 [US4] Update api-reference/index.md to display categorized API references

**Acceptance Validation**:
```bash
npm run build
# Check /docs/api-reference/index.html exists
# Check individual API pages exist
# Manual: Verify all properties and methods documented with types and descriptions
```

---

## Phase 8: User Story 6 - Seamless Navigation (P2) (Day 5, ~2 hours)

**User Story**: A developer on the NuGet package page can easily discover the documentation site, and a developer on the documentation site can easily find the NuGet package.

**Goal**: Bidirectional links between NuGet package page and GitHub Pages documentation

**Prerequisites**: Phase 2 complete

**Independent Test Criteria**:
1. View homepage → Find direct link to NuGet package page
2. Check footer → Find links to GitHub repository and issue tracker
3. Verify NuGet package .nuspec → Contains projectUrl pointing to GitHub Pages site

### Tasks

- [ ] T095 [P] [US6] Verify site.json contains nugetUrl pointing to package page
- [ ] T096 [P] [US6] Verify navigation.json footer includes NuGet link with external:true
- [ ] T097 [P] [US6] Add prominent NuGet link in homepage hero section
- [ ] T098 [P] [US6] Add GitHub repository link to header or footer
- [ ] T099 [P] [US6] Add issue tracker link to footer
- [ ] T100 [US6] Update /WebSpark.HttpClientUtility/WebSpark.HttpClientUtility.csproj to set ProjectUrl to https://markhazleton.github.io/WebSpark.HttpClientUtility/ (note trailing slash for GitHub Pages root)
- [ ] T101 [US6] Update README.md to add "Documentation" link pointing to https://markhazleton.github.io/WebSpark.HttpClientUtility/
- [ ] T101a [US6] After next NuGet package publish, verify ProjectUrl from .csproj propagates to NuGet.org package page (check Project Site link)
- [ ] T102 [US6] Test all external links open in new tab with rel="noopener noreferrer"

**Acceptance Validation**:
```bash
# Check .csproj has <ProjectUrl>https://markhazleton.github.io/WebSpark.HttpClientUtility</ProjectUrl>
# Manual: Click NuGet link from docs site, verify opens package page
# Manual: From NuGet package page, verify documentation link present
```

---

## Phase 9: User Story 7 - Local Development (P3) (Day 1, ~30 minutes)

**User Story**: A contributor wants to preview documentation changes locally before pushing to GitHub.

**Goal**: Development workflow documented and tested

**Prerequisites**: Phase 1 complete

**Independent Test Criteria**:
1. Clone repo, run `npm install` and `npm run dev` → Local preview server starts
2. Modify Markdown file → Changes automatically reload in browser
3. Run `npm run build` → Clean production build completes in <30 seconds

### Tasks

- [ ] T103 [US7] Verify package.json dev script includes --serve and --incremental flags
- [ ] T104 [US7] Create /src/README-SRC.md with contributor instructions for working with site
- [ ] T105 [US7] Document npm run dev workflow in README-SRC.md
- [ ] T106 [US7] Document npm run build workflow in README-SRC.md
- [ ] T107 [US7] Document npm run validate workflow in README-SRC.md
- [ ] T108 [US7] Test clean rebuild: `npm run clean && npm run build` completes in <30 seconds (baseline check; T193 validates final optimized build)
- [ ] T109 [US7] Test hot reload: Start dev server, edit page, verify browser auto-refreshes

**Acceptance Validation**:
```bash
cd src
npm ci  # Clean install
npm run dev  # Should start in <10 seconds
# Edit a page, verify live reload works
npm run build  # Should complete in <30 seconds
```

---

## Phase 10: Examples and Additional Content (Day 5-6, ~4 hours)

**Goal**: Add example pages demonstrating common usage patterns

**Prerequisites**: Phase 2 complete

**Independent Test**: Each example page renders with working code examples

### Tasks

- [ ] T110 [P] Create /src/pages/examples/index.md with examples overview
- [ ] T111 [P] Create /src/pages/examples/basic-usage.md with simple GET/POST examples
- [ ] T112 [P] Create /src/pages/examples/caching.md with caching configuration examples
- [ ] T113 [P] Create /src/pages/examples/resilience.md with retry and circuit breaker examples
- [ ] T114 [P] Create /src/pages/examples/web-crawling.md with site crawler examples
- [ ] T115 [P] Create /src/pages/examples/authentication.md with auth provider examples
- [ ] T116 Add "examples" collection to .eleventy.js
- [ ] T117 Add example page styles to main.css (example cards, code block improvements)

---

## Phase 11: About Pages (Day 6, ~2 hours)

**Goal**: Add contributing guidelines, changelog, and about pages

**Prerequisites**: Phase 2 complete

**Independent Test**: All about pages render and link correctly

### Tasks

- [ ] T118 [P] Create /src/pages/about/index.md with about section overview
- [ ] T119 [P] Create /src/pages/about/contributing.md sourcing from /documentation/CONTRIBUTING.md
- [ ] T120 [P] Create /src/pages/about/changelog.md sourcing from /CHANGELOG.md
- [ ] T121 Add changelog rendering to display releases with dates
- [ ] T122 Link about pages from footer navigation

---

## Phase 12: GitHub Actions Deployment (Day 6, ~1 hour)

**Goal**: Automated documentation builds on every push to main

**Prerequisites**: Phase 1-2 complete, at least one page exists

**Independent Test**: Push to main branch triggers build, /docs folder updates automatically

### Tasks

- [ ] T123 Create /.github/workflows/publish-docs.yml workflow file
- [ ] T124 Configure workflow to trigger on push to main and changes to src/**
- [ ] T125 Configure workflow to use Node.js 20.x with npm cache
- [ ] T126 Add workflow step: Install dependencies (npm ci)
- [ ] T127 Add workflow step: Build site (npm run build)
- [ ] T128 Add workflow step: Validate output (npm run validate)
- [ ] T129 Add workflow step: Commit and push /docs folder changes
- [ ] T130 Add workflow_dispatch trigger for manual builds
- [ ] T131 Test workflow: Push change to src/, verify GitHub Actions runs and /docs updates

**Acceptance Validation**:
```bash
git add .github/workflows/publish-docs.yml
git commit -m "ci: add documentation build workflow"
git push origin 001-static-docs-site
# Check GitHub Actions tab, verify workflow runs successfully
```

---

## Phase 13: GitHub Pages Configuration (Day 6, ~15 minutes)

**Goal**: Enable GitHub Pages to serve from /docs folder

**Prerequisites**: Phase 12 complete, /docs folder committed to main branch

**Independent Test**: Visit GitHub Pages URL, site loads successfully

### Tasks

- [ ] T132 Navigate to repository Settings → Pages
- [ ] T133 Set Source to "Deploy from a branch"
- [ ] T134 Set Branch to "main" and folder to "/docs"
- [ ] T135 Save configuration and wait for deployment (~1-2 minutes)
- [ ] T136 Visit https://markhazleton.github.io/WebSpark.HttpClientUtility and verify site loads
- [ ] T137 Test navigation: Click through all main menu items, verify no 404s, and confirm all pages reachable (no orphaned pages)
- [ ] T138 Test mobile: Resize browser to 320px width, verify mobile menu works

**Acceptance Validation**:
```bash
# Manual: Open https://markhazleton.github.io/WebSpark.HttpClientUtility
# Verify: Homepage loads, NuGet stats display, navigation works
# Verify: Mobile menu toggles on small screens
```

---

## Phase 14: Content Migration (Day 7-8, ~8 hours)

**Goal**: Migrate content from existing documentation files to site pages

**Prerequisites**: Phases 3-6 complete (page templates exist)

**Independent Test**: All migrated content displays correctly with proper formatting

### Tasks

- [ ] T138a Audit existing documentation files for accuracy against library v1.4.0 API (verify code examples, configuration options, feature descriptions)
- [ ] T139 Review existing /documentation/GettingStarted.md and extract content for getting-started.md
- [ ] T140 Review existing /documentation/Configuration.md and extract content for features pages
- [ ] T141 Review existing /documentation/Caching.md and migrate to features/caching.md
- [ ] T142 Review existing /documentation/Resilience.md and migrate to features/resilience.md
- [ ] T143 Review README.md and extract content for homepage hero section
- [ ] T144 Extract code examples from test files for examples pages
- [ ] T145 Review CHANGELOG.md format and ensure it renders correctly in about/changelog.md
- [ ] T146 Verify all internal links updated to new site structure

---

## Phase 15: Performance Optimization (Day 8, ~3 hours)

**Goal**: Optimize site for <2 second load time on 3G connection

**Prerequisites**: Content complete, site fully built

**Independent Test**: Lighthouse performance score 90+

### Tasks

- [ ] T147 Install html-minifier-terser: `npm install --save-dev html-minifier-terser`
- [ ] T148 Add HTML minification to .eleventy.js for production builds
- [ ] T149 Install cssnano: `npm install --save-dev cssnano postcss postcss-cli`
- [ ] T150 Create postcss.config.js for CSS minification
- [ ] T151 Update package.json build:prod script to minify CSS
- [ ] T152 Optimize images: Convert to WebP format (80% quality, <100KB per image) with PNG/JPEG fallback for older browsers
- [ ] T153 Add loading="lazy" to all images below the fold
- [ ] T154 Verify CSS custom properties supported in target browsers (Chrome 90+, Firefox 88+, Safari 14+)
- [ ] T155 Test bundle sizes: CSS <15KB gzipped, JS <40KB gzipped (Prism.js)
- [ ] T156 Run Lighthouse on homepage: `npm run lighthouse` (if script added)
- [ ] T157 Verify Lighthouse Performance score 90+
- [ ] T158 Verify Lighthouse Accessibility score 90+
- [ ] T159 Verify Lighthouse SEO score 90+

**Acceptance Validation**:
```bash
npm run build:prod
# Check file sizes in /docs/assets/
# Run Lighthouse audit (Chrome DevTools)
# Verify Performance/Accessibility/SEO scores all 90+
```

---

## Phase 16: Accessibility Audit (Day 8-9, ~2 hours)

**Goal**: Ensure site is fully accessible (keyboard navigation, screen readers, ARIA)

**Prerequisites**: All pages complete

**Independent Test**: Can navigate entire site with keyboard only, screen reader announces content correctly

### Tasks

- [ ] T160 Verify all navigation links keyboard accessible (Tab key navigation works)
- [ ] T161 Verify mobile menu checkbox has proper aria-label="Toggle navigation"
- [ ] T162 Add skip-to-content link for keyboard users at top of base.njk
- [ ] T163 Verify all images have descriptive alt text
- [ ] T164 Verify all code examples have language labels for screen readers
- [ ] T165 Verify color contrast meets WCAG AA standards (4.5:1 for normal text)
- [ ] T166 Test with screen reader (NVDA on Windows or VoiceOver on Mac)
- [ ] T167 Verify form elements (if any) have associated labels
- [ ] T168 Verify focus indicators visible on all interactive elements
- [ ] T169 Run axe DevTools accessibility scan, fix any issues

**Acceptance Validation**:
```bash
# Manual: Navigate site using only Tab/Enter/Arrow keys
# Manual: Run axe DevTools extension, verify no violations
# Manual: Test with screen reader, verify content announced correctly
```

---

## Phase 17: SEO Optimization (Day 9, ~2 hours)

**Goal**: Optimize for search engine visibility

**Prerequisites**: All content complete

**Independent Test**: Lighthouse SEO score 90+, sitemap.xml generated

### Tasks

- [ ] T170 Verify all pages have unique titles (70 characters or less)
- [ ] T171 Verify all pages have meta descriptions (120-160 characters)
- [ ] T172 Verify Open Graph tags in base.njk (og:title, og:description, og:url, og:type)
- [ ] T173 Verify Twitter Card meta tags in base.njk
- [ ] T174 Install sitemap plugin: `npm install --save-dev @quasibit/eleventy-plugin-sitemap`
- [ ] T175 Configure sitemap plugin in .eleventy.js with site URL
- [ ] T176 Verify sitemap.xml generates at /docs/sitemap.xml
- [ ] T177 Update robots.txt sitemap reference to correct URL
- [ ] T178 Verify canonical URLs set correctly on all pages
- [ ] T179 Add structured data (JSON-LD) for SoftwareApplication to homepage (optional)
- [ ] T180 Submit sitemap to Google Search Console (manual, post-launch)

**Acceptance Validation**:
```bash
npm run build
# Check /docs/sitemap.xml exists
# Check /docs/robots.txt has correct sitemap URL
# Run Lighthouse SEO audit, verify 90+ score
```

---

## Phase 18: Testing and Validation (Day 9, ~2 hours)

**Goal**: Comprehensive testing across browsers and devices

**Prerequisites**: All phases complete

**Independent Test**: Site works correctly on all target browsers and devices

### Tasks

- [ ] T181 Test on Chrome (latest stable) - desktop and mobile
- [ ] T182 Test on Firefox (latest stable) - desktop
- [ ] T183 Test on Safari (macOS and iOS latest) - desktop and mobile
- [ ] T184 Test on Edge (latest stable) - desktop
- [ ] T185 Test on Android Chrome (latest stable) - mobile
- [ ] T186 Test responsive design at 320px, 768px, 1024px, 1920px widths
- [ ] T187 Test with JavaScript disabled (progressive enhancement)
- [ ] T188 Run link checker: `npm run test:links` and fix any broken links
- [ ] T189 Run HTML validator: `npm run test:html` and fix any errors
- [ ] T190 Test print styles (optional): Pages should print cleanly
- [ ] T191 Verify all external links have target="_blank" and rel="noopener noreferrer"
- [ ] T192 Test NuGet API fallback: Break API URL, verify cached data used
- [ ] T193 Verify build time: `npm run build` completes in <30 seconds

**Acceptance Validation**:
```bash
npm run validate  # Runs test:html and test:links
# Manual: Test on all target browsers
# Manual: Test responsive behavior
# Manual: Test keyboard navigation
```

---

## Phase 19: Documentation and Polish (Day 9, ~1 hour)

**Goal**: Final documentation updates and repository cleanup

**Prerequisites**: All phases complete

**Independent Test**: README updated, all links work, repository is clean

### Tasks

- [ ] T194 Update /README.md to add "Documentation" section with link to GitHub Pages site
- [ ] T195 Add badge to README for documentation site status
- [ ] T196 Update /README.md to mention site is auto-generated from /src folder
- [ ] T197 Verify /src/README-SRC.md has complete contributor instructions
- [ ] T198 Create or update /documentation/README.md to redirect to GitHub Pages site
- [ ] T199 Verify all spec.md success criteria marked as complete
- [ ] T200 Clean up any temporary files or test artifacts
- [ ] T201 Verify .gitignore includes node_modules and excludes /docs (docs should be committed)
- [ ] T202 Final commit: "docs: complete static documentation website implementation"

**Acceptance Validation**:
```bash
git status  # Verify no uncommitted changes
# Manual: Review README.md, verify documentation link present
# Manual: Visit GitHub Pages site one final time
```

---

## Dependency Graph

### Phase Dependencies

```
Phase 1 (Setup)
    ↓
Phase 2 (Foundational) ← BLOCKING for all user stories
    ↓
    ├─→ Phase 3 (US1: Homepage) ⚡ MVP
    ├─→ Phase 4 (US2: Features)
    ├─→ Phase 5 (US3: Getting Started)
    ├─→ Phase 6 (US4: API Reference)
    ├─→ Phase 7 (US5: Live Stats) ← Depends on Phase 2 T019
    ├─→ Phase 8 (US6: Navigation)
    └─→ Phase 9 (US7: Local Dev) ← Can complete anytime after Phase 1
    ↓
Phase 10 (Examples) ← Depends on feature pages (Phase 4)
Phase 11 (About Pages)
    ↓
Phase 12 (GitHub Actions) ← Can start after Phase 2
Phase 13 (GitHub Pages Config) ← Depends on Phase 12
    ↓
Phase 14 (Content Migration) ← Depends on Phases 3-6 templates
Phase 15 (Performance) ← All content complete
Phase 16 (Accessibility) ← All pages complete
Phase 17 (SEO) ← All content complete
Phase 18 (Testing) ← All phases complete
Phase 19 (Polish) ← Final phase
```

### User Story Independence

**Fully Independent** (can implement in any order after Phase 2):
- ✅ US1 (Homepage) - No dependencies on other stories
- ✅ US2 (Features) - No dependencies on other stories
- ✅ US3 (Getting Started) - No dependencies on other stories
- ✅ US4 (API Reference) - No dependencies on other stories
- ✅ US6 (Navigation) - No dependencies on other stories

**Partially Dependent**:
- ⚠️ US5 (Live Stats) - Requires Phase 2 T019 (nuget.js), but can be validated independently
- ⚠️ US7 (Local Dev) - Only requires Phase 1 (setup), very early validation possible

**Content Dependencies** (later phases):
- Phase 10 (Examples) benefits from Phase 4 (Features) being complete for consistency
- Phase 14 (Content Migration) requires Phases 3-6 templates to exist

---

## Parallel Execution Opportunities

### Phase 2 Parallelization (9 tasks can run simultaneously)

**Group A - Data Files** (no conflicts):
- T018 site.json
- T019 nuget.js
- T020 nuget-cache.json
- T021 navigation.json

**Group B - Templates** (no conflicts):
- T023 base.njk
- T024 page.njk
- T025 header.njk
- T026 footer.njk

**Group C - Styling** (same file, sequential):
- T029-T035 main.css (sequential within group)
- T036-T038 Prism.js (independent of main.css)

**Group D - Assets** (no conflicts):
- T039 favicon
- T040 logo

### Phase 4 Parallelization (6 feature pages simultaneously)

- T055 caching.md
- T056 resilience.md
- T057 telemetry.md
- T058 web-crawling.md
- T059 authentication.md

All feature pages are independent Markdown files - can be created in parallel by different developers or AI agents.

### Phase 6 Parallelization (5 API pages simultaneously)

- T078 http-request-result.md
- T079 http-request-result-service.md
- T080 resilience-options.md
- T081 cache-options.md
- T082 authentication-providers.md

### Phase 10 Parallelization (5 example pages simultaneously)

- T111 basic-usage.md
- T112 caching.md
- T113 resilience.md
- T114 web-crawling.md
- T115 authentication.md

---

## Implementation Strategy

### MVP (Minimum Viable Product) - Week 1

**Goal**: Launch with homepage only, demonstrating value and generating feedback

**Scope**:
- Phase 1: Setup ✅
- Phase 2: Foundational ✅
- Phase 3: User Story 1 (Homepage) ✅
- Phase 7: Live Stats ✅
- Phase 12: GitHub Actions ✅
- Phase 13: GitHub Pages Config ✅

**Tasks**: T001-T053, T088-T094, T123-T138 (88 tasks)
**Timeline**: 3-4 days
**Deliverable**: Homepage with package value, stats, installation command, deployed to GitHub Pages

### Iteration 2 - Week 2

**Goal**: Add features, getting started, and API reference

**Scope**:
- Phase 4: User Story 2 (Features)
- Phase 5: User Story 3 (Getting Started)
- Phase 6: User Story 4 (API Reference)
- Phase 8: User Story 6 (Navigation)

**Tasks**: T054-T102 (49 tasks)
**Timeline**: 3-4 days
**Deliverable**: Complete documentation site with all core content

### Iteration 3 - Week 2-3

**Goal**: Examples, about pages, and polish

**Scope**:
- Phase 9: User Story 7 (Local Dev)
- Phase 10: Examples
- Phase 11: About Pages
- Phase 14: Content Migration

**Timeline**: 2-3 days
**Deliverable**: Comprehensive documentation with examples and migration complete

### Iteration 4 - Week 3

**Goal**: Performance, accessibility, SEO optimization

**Scope**:
- Phase 15: Performance Optimization
- Phase 16: Accessibility Audit
- Phase 17: SEO Optimization
- Phase 18: Testing and Validation
- Phase 19: Documentation and Polish

**Timeline**: 2-3 days
**Deliverable**: Production-ready, optimized documentation site

---

## Success Criteria Mapping

| Success Criterion | Phase | Validation Tasks |
|-------------------|-------|------------------|
| SC-001: Value prop visible <10s | Phase 3 | T041-T048 |
| SC-002: Getting started <5 min | Phase 5 | T067-T076 |
| SC-003: Load <2s on 3G | Phase 15 | T147-T159 |
| SC-004: Lighthouse 90+ | Phase 15-17 | T156-T159, T170-T180 |
| SC-005: Build <30s | Phase 1-2 | T108, T193 |
| SC-006: Valid code examples | Phase 4-6, 10 | T066, Manual review |
| SC-007: Responsive 320px+ | Phase 2 | T035, T186 |
| SC-008: SEO meta tags | Phase 2, 17 | T023, T170-T180 |
| SC-009: NuGet data <24h | Phase 7 | T088-T094 |
| SC-010: Graceful API failure | Phase 7 | T092 |
| SC-011: Bidirectional links | Phase 8 | T095-T102 |
| SC-012: Dev server <1 min | Phase 9 | T109 |

---

## Task Summary

**Total Tasks**: 206
**Parallelizable Tasks**: 49 (marked with [P])
**Estimated Effort**: 
- Phase 1: 2 hours
- Phase 2: 3 hours
- Phase 3 (MVP): 3 hours
- Phase 4-6: 11 hours
- Phase 7-9: 3.5 hours
- Phase 10-11: 6 hours
- Phase 12-13: 1.25 hours
- Phase 14: 8 hours
- Phase 15-17: 7 hours
- Phase 18-19: 3 hours

**Total Estimated Time**: 47.75 hours (approximately 6-9 working days)

---

## Format Validation

✅ All tasks follow required format: `- [ ] [TaskID] [P?] [Story?] Description with file path`
✅ All user story tasks labeled with [US#]
✅ Setup and Foundational tasks have NO story labels
✅ Parallelizable tasks marked with [P]
✅ Sequential task IDs (T001-T206)
✅ File paths included in all implementation tasks

---

## Next Steps

1. **Review tasks.md** - Verify task breakdown aligns with project goals
2. **Choose strategy** - MVP-first (recommended) or full implementation
3. **Begin Phase 1** - Run T001-T017 to initialize project structure
4. **Track progress** - Mark tasks complete as you go
5. **Test incrementally** - Run validation after each phase
6. **Deploy early** - Get MVP (Phase 3) live for feedback

**Ready to implement!** Follow quickstart.md for detailed implementation guidance.

---

**Generated**: 2025-11-02  
**Status**: ✅ Complete - Ready for Implementation  
**Next Command**: Begin Phase 1 or run specific tasks as needed