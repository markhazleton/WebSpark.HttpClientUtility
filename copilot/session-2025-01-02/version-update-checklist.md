# Version Update Checklist

Quick reference checklist for updating version numbers across the repository.

## Pre-Release Preparation

- [ ] Ensure all tests pass (711 test runs across 3 frameworks)
- [ ] Update CHANGELOG.md with new version entry
- [ ] Review code for any hardcoded version strings

## Required File Updates

### Core Version Files
- [ ] **Directory.Build.props** - Update `<Version>`, `<FileVersion>`, `<AssemblyVersion>`
  - Location: Root directory
  - Format: `<Version>X.Y.Z</Version>`

### NuGet Package Metadata
- [ ] **WebSpark.HttpClientUtility.csproj**
  - Location: `WebSpark.HttpClientUtility/`
  - Update `<PackageReleaseNotes>` - Add new version at top
  - Update `<Description>` test count if changed
  - Update `<PackageValidationBaselineVersion>` to previous stable version

- [ ] **WebSpark.HttpClientUtility.Crawler.csproj**
  - Location: `WebSpark.HttpClientUtility.Crawler/`
  - Update `<PackageReleaseNotes>` - Add new version at top
  - Update `<Description>` base package requirement `[X.Y.Z]`
  - Ensure lockstep versioning (same as base package)

- [ ] **WebSpark.HttpClientUtility.Testing.csproj** (if exists)
  - Update version and release notes
  - Update base package requirement

### Website Content
- [ ] **WebSpark.HttpClientUtility.Web/Views/Home/Index.cshtml**
  - Line ~24: Update hero section badge `<span class="badge">vX.Y.Z</span>`
  - Verify framework support badges are current

- [ ] **WebSpark.HttpClientUtility.Web/Views/Shared/_Layout.cshtml**
  - Footer section: Update version badge `<span class="badge bg-primary">vX.Y.Z</span>`

### Repository Documentation
- [ ] **README.md**
  - Update `## 📦 vX.Y - Now in Two Focused Packages!` heading if major version
  - Verify test count is accurate throughout
  - Update Project Stats section
  - Check migration guide version references (should be version-agnostic)

- [ ] **CHANGELOG.md**
  - Add new version section at top with date: `## [X.Y.Z] - YYYY-MM-DD`
  - Follow Keep a Changelog format: Added, Changed, Fixed, Security, etc.
  - Link to comparison: `[X.Y.Z]: https://github.com/.../compare/vPREV...vCURRENT`

- [ ] **WebSpark.HttpClientUtility.Crawler/README-Crawler.md**
  - Line ~9: Update base package requirement
  - Lines ~187-189: Update Requirements section with new version

- [ ] **WebSpark.HttpClientUtility.Testing/README-Testing.md**
  - Update Requirements section minimum version

### Static Documentation Site
- [ ] **src/pages/index.md**
  - Verify test count and framework support
  - Update feature descriptions if changed

- [ ] **src/pages/about.md**
  - Add new version to Version History section
  - Update "Current" designation
  - Update Quality Assurance section if test count changed

- [ ] **src/pages/getting-started.md**
  - Verify all code examples work with new version
  - Update any version-specific instructions

### Security & Policy
- [ ] **documentation/SECURITY.md**
  - Update Supported Versions table
  - Mark old versions as unsupported if deprecating

## Optional Updates (as needed)

### Major Version Changes (X.0.0)
- [ ] Update migration guides
- [ ] Create migration documentation for breaking changes
- [ ] Update API reference documentation
- [ ] Announce deprecations clearly

### Minor Version Changes (X.Y.0)
- [ ] Document new features in feature documentation
- [ ] Add code examples for new features
- [ ] Update API reference if new public APIs added

### Patch Version Changes (X.Y.Z)
- [ ] Document bug fixes in CHANGELOG
- [ ] Update any affected documentation

## Framework Support Changes

### Adding New Target Framework (e.g., .NET 11)
- [ ] **Directory.Build.props** - Add to `<TargetFrameworks>`
- [ ] All .csproj files - Verify multi-targeting works
- [ ] README.md - Update framework support badges
- [ ] Website badges - Update .NET version displays
- [ ] Static docs - Update Technology Stack section
- [ ] Test and verify all X target frameworks = X * unique tests = total test runs

### Removing Target Framework (e.g., dropping .NET 8 support)
- [ ] **Directory.Build.props** - Remove from `<TargetFrameworks>`
- [ ] Update all documentation about minimum supported versions
- [ ] Update security policy
- [ ] Create migration guide for users on old framework
- [ ] Update NuGet package descriptions

## Test Count Updates

If test count changes:
- [ ] README.md - Multiple locations
- [ ] Package .csproj files - Description and release notes
- [ ] Static docs site - index.md and about.md
- [ ] Website homepage - If displayed

Current format: "237 unique tests (711 test runs across 3 frameworks)"
- Unique tests = actual test methods
- Test runs = unique tests × target frameworks

## Post-Update Verification

- [ ] Run `dotnet build` - Zero warnings
- [ ] Run `dotnet test` - All tests pass
- [ ] Review NuGet package contents with `dotnet pack`
- [ ] Check website renders correctly (run demo app)
- [ ] Verify version displayed correctly on all pages
- [ ] Spellcheck all updated documentation
- [ ] Review GitHub Actions will trigger correctly

## Git Workflow

```bash
# 1. Commit version changes
git add .
git commit -m "chore: bump version to X.Y.Z"

# 2. Tag release
git tag vX.Y.Z

# 3. Push with tags
git push origin main --tags
```

## GitHub Actions

After pushing tag starting with `v`:
- [ ] Verify build workflow starts
- [ ] Check all tests pass in CI
- [ ] Verify package published to NuGet.org
- [ ] Check GitHub release created
- [ ] Verify symbol packages (.snupkg) uploaded

## Post-Release

- [ ] Verify packages available on NuGet.org
- [ ] Test installation in clean project: `dotnet add package WebSpark.HttpClientUtility`
- [ ] Check package page displays correct information
- [ ] Announce release in GitHub Discussions
- [ ] Update any external documentation sites
- [ ] Close related GitHub issues with fix milestone

## Common Mistakes to Avoid

❌ **DON'T**: Update version in code files without updating Directory.Build.props
❌ **DON'T**: Forget to update both base and crawler packages (lockstep versioning)
❌ **DON'T**: Leave outdated version numbers in website or README
❌ **DON'T**: Forget to update test count after adding/removing tests
❌ **DON'T**: Claim framework as LTS when it's not (only .NET 8, 10, 12... are LTS)
❌ **DON'T**: Use version-specific installation commands (use version-agnostic)

✅ **DO**: Verify all changes with `dotnet build` before committing
✅ **DO**: Update CHANGELOG.md following Keep a Changelog format
✅ **DO**: Keep lockstep versioning for base and crawler packages
✅ **DO**: Make installation instructions version-agnostic
✅ **DO**: Double-check framework support claims (LTS vs current)

## Quick Checklist Summary

For a typical patch release (X.Y.Z):
1. ✅ Update Directory.Build.props version
2. ✅ Update both .csproj PackageReleaseNotes
3. ✅ Update CHANGELOG.md
4. ✅ Update website badges (2 files)
5. ✅ Update README.md if needed
6. ✅ Build & test
7. ✅ Commit, tag, push

That's it for most releases! Major/minor versions may need more documentation updates.
