# Next Steps for Static Documentation Site

**Current Status**: MVP Complete - Infrastructure and homepage ready  
**Branch**: `001-static-docs-site`

## Immediate Next Steps (Required for Launch)

### 1. Enable GitHub Pages (5 minutes)
Once this branch is merged to `main`:

1. Go to repository **Settings** → **Pages**
2. Set **Source** to "Deploy from a branch"
3. Set **Branch** to `main` and folder to `/docs`
4. Click **Save**
5. Wait 1-2 minutes for deployment
6. Visit `https://markhazleton.github.io/WebSpark.HttpClientUtility/`

### 2. Upgrade Prism.js (15 minutes)
Current placeholder needs real syntax highlighting:

1. Visit https://prismjs.com/download.html
2. **Configuration**:
   - Version: Latest (1.29.0+)
   - Compression: Minified
   - Theme: Tomorrow Night
3. **Languages** (select these):
   - Markup (HTML/XML)
   - CSS
   - C-like
   - C# (csharp)
   - JavaScript
   - JSON
   - PowerShell
4. **Plugins** (select these):
   - Line Numbers
   - Copy to Clipboard Button (optional but nice)
5. Download JS and CSS
6. Replace files:
   - Save JS to `src/assets/js/prism.min.js`
   - Save CSS to `src/assets/css/prism-tomorrow.css`
7. Rebuild: `cd src && npm run build`

### 3. Test Live Site (10 minutes)
After GitHub Pages is enabled:

- [ ] Homepage loads correctly
- [ ] NuGet stats display (version + downloads)
- [ ] Navigation menu works on desktop
- [ ] Mobile menu toggles correctly (test on phone or responsive mode)
- [ ] Footer links work and open in new tabs
- [ ] Code example displays with syntax highlighting
- [ ] CTA buttons link to correct pages (will 404 until created)

## Content Creation (Remaining Work)

### Phase 1: Core Pages (4-6 hours)

#### Getting Started Page
Create `src/pages/getting-started.md`:
- Prerequisites (Node.js, .NET 8+)
- Installation instructions
- Basic configuration
- First HTTP request example
- Enabling optional features (caching, resilience, telemetry)
- Troubleshooting common issues

#### Features Overview
Create `src/pages/features/index.md`:
- Introduction to features
- Feature comparison table
- Quick links to individual feature pages

#### Individual Feature Pages
Create these in `src/pages/features/`:
- `caching.md` - Response caching feature
- `resilience.md` - Polly resilience policies
- `telemetry.md` - OpenTelemetry integration
- `web-crawling.md` - Site crawler capabilities
- `authentication.md` - Auth providers

### Phase 2: API Reference (6-8 hours)

Create pages in `src/pages/api-reference/`:
- `index.md` - API reference overview
- `http-request-result.md` - Core request/response model
- `http-request-result-service.md` - Main service interface
- `resilience-options.md` - Polly configuration
- `cache-options.md` - Caching configuration
- `authentication-providers.md` - Auth interfaces

### Phase 3: Examples (3-4 hours)

Create pages in `src/pages/examples/`:
- `index.md` - Examples overview
- `basic-usage.md` - Simple GET/POST requests
- `caching.md` - Caching examples
- `resilience.md` - Retry and circuit breaker examples
- `web-crawling.md` - Crawler examples
- `authentication.md` - Auth provider examples

### Phase 4: About Pages (2-3 hours)

Create pages in `src/pages/about/`:
- `index.md` - About section overview
- `contributing.md` - Contribution guidelines (source from `/documentation/CONTRIBUTING.md`)
- `changelog.md` - Release history (source from `/CHANGELOG.md`)

## Enhancements (Nice to Have)

### Visual Improvements
1. **Logo**: Replace placeholder favicon with actual WebSpark logo
2. **Screenshots**: Add visual examples to documentation
3. **Diagrams**: Create architecture diagrams for decorator pattern
4. **Comparison Table**: Add visual feature comparison on homepage

### Performance Optimization
1. **HTML Minification**: Add `html-minifier-terser` to build pipeline
2. **CSS Minification**: Add `cssnano` for production builds
3. **Image Optimization**: Convert images to WebP format
4. **Critical CSS**: Inline above-the-fold CSS

### SEO & Accessibility
1. **Sitemap**: Add `@quasibit/eleventy-plugin-sitemap`
2. **Search**: Implement client-side search (Lunr.js or Pagefind)
3. **Accessibility Audit**: Run axe DevTools, fix any issues
4. **Lighthouse CI**: Add to GitHub Actions for continuous monitoring

### Developer Experience
1. **Hot Reload**: Ensure `npm run dev` works for local development
2. **Documentation**: Add README in `/src` folder for contributors
3. **Link Checker**: Run `npm run test:links` in CI/CD
4. **HTML Validator**: Run `npm run test:html` in CI/CD

## Content Migration Strategy

### Source Content Locations
1. **README.md** (root) → Homepage content
2. **CHANGELOG.md** (root) → About/Changelog page
3. **documentation/CONTRIBUTING.md** → About/Contributing page
4. **documentation/GettingStarted.md** → Getting Started page (merge with new content)
5. **Test files** → Extract code examples for Examples pages

### Migration Process
For each source file:
1. Review existing content for accuracy (verify API examples match v1.4.0)
2. Reformat as Markdown with proper front matter
3. Add code examples with Prism.js syntax highlighting
4. Link related pages together
5. Test locally with `npm run dev`
6. Build and verify output

## Testing Checklist

### Browser Testing
- [ ] Chrome (latest) - Desktop
- [ ] Chrome (latest) - Mobile
- [ ] Firefox (latest) - Desktop
- [ ] Safari (macOS/iOS) - Desktop and Mobile
- [ ] Edge (latest) - Desktop

### Responsive Testing
- [ ] 320px width (iPhone SE)
- [ ] 768px width (iPad)
- [ ] 1024px width (Desktop)
- [ ] 1920px width (Large desktop)

### Functionality Testing
- [ ] All internal links work (no 404s)
- [ ] All external links work and open in new tabs
- [ ] Mobile menu toggles correctly
- [ ] Code examples display with syntax highlighting
- [ ] NuGet stats update on rebuild
- [ ] Cache fallback works when API fails

### Performance Testing
- [ ] Lighthouse audit (Performance 90+)
- [ ] Lighthouse audit (Accessibility 90+)
- [ ] Lighthouse audit (SEO 90+)
- [ ] Page load time <2s on 3G throttling

### Accessibility Testing
- [ ] Keyboard navigation works (Tab, Enter, Esc)
- [ ] Screen reader announces content correctly (NVDA/VoiceOver)
- [ ] Focus indicators visible on all interactive elements
- [ ] Color contrast meets WCAG AA (4.5:1 for normal text)
- [ ] All images have alt text

## Success Metrics to Track

After launch, monitor these metrics:

1. **Traffic**: Google Analytics or GitHub traffic stats
2. **Engagement**: Time on page, bounce rate
3. **Performance**: Lighthouse scores over time
4. **SEO**: Google Search Console impressions/clicks
5. **Errors**: 404s, broken links, build failures

## Questions to Answer

Before full launch:
1. Should we add a search feature? (Lunr.js, Pagefind, Algolia)
2. Do we need versioned documentation? (e.g., v1.4, v1.3, etc.)
3. Should we add a blog/changelog RSS feed?
4. Do we want analytics? (Google Analytics, Plausible, Umami)
5. Should we add a feedback widget? (GitHub Discussions link)

## Estimated Timeline

- **Immediate (Today)**: Enable GitHub Pages, test live site
- **Week 1**: Upgrade Prism.js, create Getting Started + Features pages
- **Week 2**: Create API Reference + Examples pages
- **Week 3**: Create About pages, performance optimization
- **Week 4**: Final testing, accessibility audit, SEO optimization

**Total Estimated Time**: 20-30 hours of work over 4 weeks

## Resources

- [Eleventy Documentation](https://www.11ty.dev/docs/)
- [Nunjucks Templating](https://mozilla.github.io/nunjucks/)
- [Prism.js](https://prismjs.com/)
- [GitHub Pages Docs](https://docs.github.com/en/pages)
- [Web Content Accessibility Guidelines (WCAG)](https://www.w3.org/WAI/WCAG21/quickref/)
- [Lighthouse CI](https://github.com/GoogleChrome/lighthouse-ci)

---

**Status**: Ready to launch MVP → Add content incrementally → Optimize based on metrics
