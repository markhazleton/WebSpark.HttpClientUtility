# Implementation Plan: Static Documentation Website for GitHub Pages

**Branch**: `001-static-docs-site` | **Date**: 2025-11-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-static-docs-site/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Create a static documentation website using Eleventy (11ty) that generates from `/src` to `/docs` folder for GitHub Pages hosting. The site will promote the WebSpark.HttpClientUtility NuGet package with live package statistics fetched from NuGet API during build time, comprehensive feature documentation, API reference, and seamless bidirectional navigation between the NuGet package page and documentation site. The site uses minimal dependencies (custom CSS with CSS custom properties, Prism.js for syntax highlighting) and implements progressive enhancement to work without JavaScript while providing enhanced experience when available.

## Technical Context

**Language/Version**: Node.js 20.x LTS (required for Eleventy 3.x), JavaScript ES2022, HTML5, CSS3  
**Primary Dependencies**: 
- Eleventy (11ty) 3.0+ - Static site generator
- Prism.js - Syntax highlighting for C#, JavaScript, JSON, PowerShell
- node-fetch or axios - NuGet API client for build-time data fetching
- eleventy-plugin-vue (optional) - For advanced templating if needed

**Storage**: 
- Source files: `/src` folder (Markdown content, templates, assets)
- Build output: `/docs` folder (GitHub Pages publish target)
- NuGet cache: `/src/_data/nuget-cache.json` (last known good API data)

**Testing**: 
- Eleventy build validation (successful generation to /docs)
- Link checker for broken internal/external links
- HTML validation (W3C validator or similar)
- Lighthouse CI for performance/accessibility/SEO audits
- Manual testing across browsers (Chrome, Firefox, Safari, Edge)

**Target Platform**: 
- GitHub Pages (static HTML hosting from /docs folder)
- Browsers: Modern evergreen browsers (Chrome 90+, Firefox 88+, Safari 14+, Edge 90+)
- Mobile devices: iOS Safari, Android Chrome (responsive design required)

**Project Type**: Static website (documentation site with build pipeline)

**Performance Goals**: 
- Page load: First Contentful Paint <1.2s, Time to Interactive <2.0s on Lighthouse "Slow 4G" throttling
- Build time: <30 seconds for clean rebuild (clean + build)
- Lighthouse scores: 90+ for Performance, Accessibility, SEO (Lighthouse v11+ with Chrome 120+, default settings, mobile emulation)
- Bundle size: CSS <15KB gzipped, JS <40KB gzipped

**Constraints**: 
- GitHub Pages limitations: Static HTML only, no server-side processing
- /docs folder requirement: GitHub Pages source directory
- /src folder requirement: All source files must live here per user specification
- Minimal dependencies: Avoid framework bloat, keep bundle size small
- Progressive enhancement: Core content must work without JavaScript
- NuGet API: Public endpoint, no authentication, rate limiting possible

**Scale/Scope**: 
- Pages: ~10-15 documentation pages (homepage, features, getting started, API reference, etc.)
- Code examples: 20-30 syntax-highlighted snippets across all pages
- Assets: Minimal (logo, maybe 5-10 optimized images/diagrams)
- Navigation items: 5-7 top-level menu items
- Target audience: .NET developers evaluating/using the NuGet package

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Evaluated Against**: WebSpark.HttpClientUtility Constitution v1.0.0

### ✅ Passes All Gates

This feature does NOT modify the core library and therefore:

- **✅ Library-First Design**: Not applicable - this is documentation infrastructure, not library code
- **✅ Test Coverage**: Not applicable - no library code changes, static site has build validation instead
- **✅ Multi-Targeting**: Not applicable - Node.js build system separate from .NET library
- **✅ One-Line Developer Experience**: Not applicable - no API changes
- **✅ Observability**: Not applicable - no library code changes
- **✅ Versioning**: Not applicable - documentation version tracks library version
- **✅ Decorator Pattern**: Not applicable - no library architecture changes
- **✅ Code Analysis**: Not applicable - no C# code changes
- **✅ XML Documentation**: Not applicable - manual documentation, not auto-generated
- **✅ Async/Await**: Not applicable - Node.js JavaScript, not .NET
- **✅ Dependency Management**: ✅ PASS - Eleventy + Prism.js are minimal, justified dependencies for static site generation
- **✅ AI Output Organization**: ✅ PASS - All plan/research/design docs stored in `/specs/001-static-docs-site/`

### Constitution Compliance Summary

**Status**: ✅ FULLY COMPLIANT

**Rationale**: This feature adds documentation infrastructure without touching library code. It enhances the "One-Line Developer Experience" principle by providing clear documentation that helps developers understand and adopt the library faster. The documentation will showcase the library's decorator pattern architecture, testing standards, and development workflow from the constitution.

**Documentation Impact**: The static site will reference constitutional principles:
- Feature pages will explain the decorator pattern (Constitution VII)
- Getting started guide will demonstrate one-line registration (Constitution IV)
- API reference will show XML documentation examples (Constitution Technical Standards)
- Contributing page will link to constitution and development workflow

### No Violations Requiring Justification

This feature introduces no architectural complexity, no new library dependencies, and maintains all existing principles. The Complexity Tracking table is not needed.

## Project Structure

### Documentation (this feature)

```text
specs/001-static-docs-site/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output - Technology research and decisions
├── data-model.md        # Phase 1 output - Content model and navigation structure
├── quickstart.md        # Phase 1 output - Quick implementation guide
└── contracts/           # Phase 1 output - File structure contracts
    ├── eleventy-config.md      # .eleventy.js configuration contract
    ├── package-json.md         # package.json dependencies contract
    └── nuget-api-schema.json   # NuGet API response structure
```

### Source Code (repository root)

```text
# Static Site Structure (NEW - to be created)
src/                              # Eleventy source files (NEW)
├── _data/                        # Global data files for Eleventy
│   ├── nuget.js                  # NuGet API fetcher (build-time)
│   ├── nuget-cache.json          # Cached NuGet data (fallback)
│   ├── site.json                 # Site metadata (title, description, URLs)
│   └── navigation.json           # Navigation menu structure
├── _includes/                    # Eleventy templates and partials
│   ├── layouts/
│   │   ├── base.njk              # Base layout (HTML structure, head, footer)
│   │   ├── page.njk              # Standard page layout
│   │   └── home.njk              # Homepage layout
│   ├── components/
│   │   ├── header.njk            # Site header with navigation
│   │   ├── footer.njk            # Site footer
│   │   ├── code-example.njk     # Syntax-highlighted code block
│   │   └── nuget-badge.njk       # NuGet stats display
│   └── partials/
│       ├── meta-tags.njk         # SEO meta tags
│       └── analytics.njk         # Analytics scripts (if needed)
├── assets/                       # Static assets
│   ├── css/
│   │   ├── main.css              # Main stylesheet (custom CSS with variables)
│   │   ├── prism-theme.css       # Prism.js theme
│   │   └── responsive.css        # Mobile/responsive styles
│   ├── js/
│   │   ├── prism.min.js          # Prism.js syntax highlighting
│   │   └── nav-toggle.js         # Optional: Enhanced mobile nav (progressive)
│   └── images/
│       ├── logo.png              # WebSpark logo
│       └── favicon.ico           # Site favicon
├── pages/                        # Markdown content pages
│   ├── index.md                  # Homepage
│   ├── features.md               # Features overview
│   ├── getting-started.md        # Getting started guide
│   ├── api-reference.md          # API documentation
│   ├── examples/                 # Example code pages
│   │   ├── basic-usage.md
│   │   ├── caching.md
│   │   ├── resilience.md
│   │   └── web-crawling.md
│   └── about/
│       ├── contributing.md       # Contributing guide
│       └── changelog.md          # Changelog display
├── .eleventy.js                  # Eleventy configuration
├── package.json                  # NPM dependencies and scripts
├── package-lock.json             # NPM lock file
└── README-SRC.md                 # Documentation for working with the site

docs/                             # Build output (GitHub Pages publishes from here)
├── index.html                    # Generated homepage
├── features/                     # Generated feature pages
├── assets/                       # Copied/processed assets
│   ├── css/                      # Minified CSS
│   ├── js/                       # Copied JS
│   └── images/                   # Optimized images
└── [other generated HTML files]

# Existing Repository Structure (UNCHANGED)
WebSpark.HttpClientUtility/       # Main library (no changes)
WebSpark.HttpClientUtility.Test/  # Tests (no changes)
WebSpark.HttpClientUtility.Web/   # Demo app (no changes)
README.md                          # Main README (minor update to link to docs site)
CHANGELOG.md                       # Changelog (no changes)
.github/
└── workflows/
    └── publish-docs.yml          # NEW: GitHub Actions workflow for auto-building docs
```

**Structure Decision**: 

This feature adds a new `/src` folder for Eleventy source files and uses the existing `/docs` folder as the build output target for GitHub Pages. This keeps documentation completely separate from the main .NET solution while maintaining the repository's existing structure.

**Key Architectural Decisions**:
1. **Separation of Concerns**: Documentation lives in `/src`, library code unchanged
2. **GitHub Pages Integration**: `/docs` folder as publish source (GitHub standard)
3. **Build Pipeline**: NPM scripts in `/src/package.json` handle clean/build/dev
4. **Data Fetching**: Build-time NuGet API calls via `/src/_data/nuget.js` (Eleventy global data)
5. **Progressive Enhancement**: HTML/CSS first, JavaScript optional for enhancements

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

Not applicable - no constitutional violations identified. This feature adds documentation infrastructure without modifying library architecture or introducing unjustified complexity.

---

## Phase 0: Research - COMPLETE ✅

**Status**: All technology decisions validated and documented

**Deliverables**:
- ✅ [research.md](./research.md) - Comprehensive technology research
  - Eleventy best practices and project structure
  - NuGet API integration patterns with caching
  - Prism.js configuration for syntax highlighting
  - GitHub Actions deployment workflow
  - CSS-only hamburger menu implementation
  - Custom CSS with design tokens approach
  - Performance optimization strategies
  - Testing and validation approaches

**Key Decisions Made**:
1. **Static Site Generator**: Eleventy 3.0+ with Nunjucks templates
2. **Syntax Highlighting**: Prism.js (self-hosted, custom build for C#/JS/JSON/PowerShell)
3. **Styling**: Custom CSS with CSS custom properties (no frameworks)
4. **Mobile Navigation**: CSS-only hamburger menu (checkbox hack)
5. **NuGet Integration**: Build-time fetch with cache fallback
6. **Deployment**: GitHub Actions workflow with automated builds

---

## Phase 1: Design & Contracts - COMPLETE ✅

**Status**: All design documents and contracts completed

**Deliverables**:
- ✅ [data-model.md](./data-model.md) - Content structure and entity relationships
  - Site metadata model (global configuration)
  - NuGet package data structure (dynamic API data)
  - Navigation structure (menu hierarchy)
  - Documentation page model (Markdown + front matter)
  - Code example model (syntax-highlighted snippets)
  - Feature description model (detailed feature docs)
  - API reference entity model (class/interface documentation)
  - Comparison table data structure
  - Collections and aggregations
  - SEO and metadata patterns
  - Validation rules

- ✅ [contracts/](./contracts/) - Technical specifications
  - ✅ [package-json.md](./contracts/package-json.md) - NPM configuration contract
    - Required dependencies and versions
    - NPM scripts for build/dev/test
    - Node.js version requirements
  - ✅ [eleventy-config.md](./contracts/eleventy-config.md) - Eleventy configuration contract
    - Directory structure
    - Filters and shortcodes
    - Collections definition
    - Markdown configuration
    - BrowserSync setup
  - ✅ [nuget-api-schema.md](./contracts/nuget-api-schema.md) - NuGet API integration contract
    - API endpoint and response structure
    - Field definitions and validation rules
    - Caching strategy
    - Error handling patterns
    - Rate limiting considerations

- ✅ [quickstart.md](./quickstart.md) - Implementation guide
  - Step-by-step setup instructions
  - Phase-by-phase implementation plan
  - Code examples and templates
  - Troubleshooting guide
  - Success criteria checklist

**Agent Context Updated**: ✅
- GitHub Copilot instructions updated with Node.js 20.x LTS, Eleventy, and JavaScript ES2022 stack

---

## Phase 2: Task Breakdown - NOT STARTED ⏭️

**Note**: This phase is handled by the `/speckit.tasks` command (separate from `/speckit.plan`).

**Expected Output**: `tasks.md` with:
- Granular implementation tasks
- Task dependencies and ordering
- Effort estimates
- Acceptance criteria per task
- Test requirements

**Next Command**: Run `/speckit.tasks` to generate task breakdown for implementation.

---

## Implementation Summary

### What's Ready

**Planning & Research** (100% Complete):
- ✅ Technology stack validated (Eleventy, Prism.js, Node.js 20 LTS)
- ✅ Architecture decisions documented
- ✅ Data model designed
- ✅ File structure contracts defined
- ✅ Quick start implementation guide created
- ✅ Constitution compliance verified (no violations)

**Artifacts Generated**:
1. `plan.md` - This file (implementation plan)
2. `research.md` - Technology research and decisions (26 pages)
3. `data-model.md` - Content structure and relationships (15 pages)
4. `quickstart.md` - Step-by-step implementation guide (10 pages)
5. `contracts/package-json.md` - NPM configuration specification
6. `contracts/eleventy-config.md` - Eleventy setup specification
7. `contracts/nuget-api-schema.md` - API integration specification

### What's Next

**Implementation Tasks** (via `/speckit.tasks`):
- Create directory structure
- Initialize NPM project
- Configure Eleventy
- Implement global data files (site.json, nuget.js, navigation.json)
- Create base templates (layouts, components)
- Implement styling (main.css, responsive.css, Prism theme)
- Create content pages (homepage, features, getting started, API reference)
- Set up GitHub Actions workflow
- Deploy to GitHub Pages
- Content migration from existing docs
- Performance optimization (minification, image optimization)
- Accessibility audit and fixes
- SEO optimization

**Estimated Timeline**:
- Phase 1 (Infrastructure): 1-2 days
- Phase 2 (Content): 3-5 days
- Phase 3 (Polish): 1-2 days
- **Total**: 5-9 days (1-2 weeks)

### Success Metrics

From spec.md Success Criteria:
- **SC-001**: ✅ Mockup ready - Value proposition identifiable in <10s
- **SC-002**: ✅ Guide ready - Getting started within 5 minutes
- **SC-003**: 🔜 Pending - Site loads in <2s on 3G (measure post-build)
- **SC-004**: 🔜 Pending - Lighthouse 90+ scores (audit post-deploy)
- **SC-005**: 🔜 Pending - Build completes in <30s (measure during implementation)
- **SC-006**: 🔜 Pending - Code examples valid (implement validation tests)
- **SC-007**: ✅ Design ready - Responsive down to 320px width
- **SC-008**: ✅ SEO ready - Titles and meta descriptions in data model
- **SC-009**: ✅ Architecture ready - NuGet data fetches on every build
- **SC-010**: ✅ Architecture ready - Graceful degradation with cache
- **SC-011**: ✅ Content ready - Bidirectional links in navigation
- **SC-012**: ✅ Tooling ready - Dev server starts in <1 minute

### Risk Assessment

**Low Risk** ✅:
- Technology choices (Eleventy, Prism.js are mature and well-documented)
- Build pipeline (Standard NPM + GitHub Actions patterns)
- Data fetching (NuGet API is stable with cache fallback)
- Constitution compliance (No library code changes)

**Medium Risk** ⚠️:
- Content migration effort (Depends on existing docs quality)
- Performance targets (May need iteration to hit 2s on 3G)
- Lighthouse scores (May need optimization passes)

**Mitigation Strategies**:
- Content: Start with most important pages (homepage, features, getting started)
- Performance: Use lighthouse-ci in GitHub Actions for continuous monitoring
- Iterative approach: Ship MVP, then optimize based on metrics

---

## Repository State After Planning

**New Directories**:
```
specs/001-static-docs-site/
├── spec.md                        # ✅ Feature specification (Phase 0)
├── plan.md                        # ✅ This file (Phase 0-1)
├── research.md                    # ✅ Technology research (Phase 0)
├── data-model.md                  # ✅ Content model (Phase 1)
├── quickstart.md                  # ✅ Implementation guide (Phase 1)
└── contracts/                     # ✅ Technical contracts (Phase 1)
    ├── package-json.md
    ├── eleventy-config.md
    └── nuget-api-schema.md
```

**Modified Files**:
- ✅ `.github/copilot-instructions.md` - Updated with Node.js/Eleventy stack

**Ready to Create** (via `/speckit.tasks` then implementation):
- `/src/` - Eleventy source files
- `/docs/` - Build output (GitHub Pages publish target)
- `.github/workflows/publish-docs.yml` - CI/CD workflow

---

## Constitution Re-Check (Post-Design)

**Status**: ✅ STILL FULLY COMPLIANT

**Verification**:
- ✅ No library code modified
- ✅ No .NET dependencies added
- ✅ Documentation structure aligns with constitution principles
- ✅ No architectural complexity introduced
- ✅ Agent context updated following AI output organization rules

**Documentation Alignment**:
The static site will actively promote constitutional principles:
- **One-Line Developer Experience** (Constitution IV): Homepage will highlight `services.AddHttpClientUtility()`
- **Decorator Pattern Architecture** (Constitution VII): Features page will explain composable decorator layers
- **Test Coverage** (Constitution II): Examples will reference 252+ passing tests
- **Multi-Targeting** (Constitution III): Getting started will mention .NET 8 LTS + .NET 9 support
- **Observability** (Constitution V): Telemetry feature page will showcase correlation IDs and structured logging

---

## Final Checklist

**Planning Phase**:
- ✅ Spec reviewed and clarified (5 questions answered)
- ✅ Research completed (all technology unknowns resolved)
- ✅ Data model designed (entities, relationships, validation rules)
- ✅ Contracts defined (package.json, .eleventy.js, NuGet API)
- ✅ Quickstart guide created (step-by-step implementation)
- ✅ Constitution compliance verified (no violations)
- ✅ Agent context updated (GitHub Copilot instructions)

**Ready for Next Phase**:
- ⏭️ Run `/speckit.tasks` to generate task breakdown
- ⏭️ Begin implementation following quickstart.md
- ⏭️ Iterate based on metrics and feedback

---

## Related Documentation

**Planning Artifacts**:
- [spec.md](./spec.md) - Feature specification (user stories, requirements, success criteria)
- [research.md](./research.md) - Technology decisions and best practices
- [data-model.md](./data-model.md) - Content structure and entity relationships
- [quickstart.md](./quickstart.md) - Step-by-step implementation guide

**Contracts**:
- [contracts/package-json.md](./contracts/package-json.md) - NPM configuration
- [contracts/eleventy-config.md](./contracts/eleventy-config.md) - Eleventy setup
- [contracts/nuget-api-schema.md](./contracts/nuget-api-schema.md) - NuGet API integration

**Constitution**:
- [.specify/memory/constitution.md](../../.specify/memory/constitution.md) - Project constitution
- [.github/copilot-instructions.md](../../.github/copilot-instructions.md) - AI coding agent instructions

---

**Status**: ✅ Planning Complete - Ready for Task Generation

**Branch**: `001-static-docs-site`  
**Next Command**: `/speckit.tasks`  
**Generated**: 2025-11-02
