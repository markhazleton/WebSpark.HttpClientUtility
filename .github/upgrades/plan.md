# .NET 10 Multi-Targeting Upgrade Plan

## Executive Summary

This plan outlines the upgrade of WebSpark.HttpClientUtility solution to add .NET 10.0 (Preview) support while maintaining backward compatibility with .NET 8.0 and .NET 9.0. The solution consists of 5 projects that will be updated to multi-target all three framework versions.

**Migration Type**: Incremental, dependency-order approach
**Total Projects**: 5
**Estimated Duration**: 2-4 hours
**Risk Level**: Low (additive change, no breaking changes)

---

## Migration Strategy: Incremental Multi-Targeting

**Strategy Selected**: Incremental Migration

**Rationale**:
- Solution has 5 projects with clear dependency relationships
- Published NuGet library requiring careful validation at each step
- Need to verify package compatibility across all three frameworks
- Test projects depend on library projects (leaf-to-root approach)
- Lower risk approach suitable for production library

**Approach**:
- Migrate projects in dependency order (libraries first, then tests, then demo app)
- Each phase results in buildable, testable solution
- Validate multi-targeting works correctly at each step
- Can pause between phases if issues arise

---

## Dependency Analysis

### Migration Order (Bottom-Up)

**Phase 1: Core Libraries** (no project dependencies)
- WebSpark.HttpClientUtility (main library)
- WebSpark.HttpClientUtility.Crawler (crawler extension)

**Phase 2: Test Projects** (depend on Phase 1)
- WebSpark.HttpClientUtility.Test
- WebSpark.HttpClientUtility.Crawler.Test

**Phase 3: Demo Application** (depends on Phase 1)
- WebSpark.HttpClientUtility.Web

### Dependency Graph
```
WebSpark.HttpClientUtility (base library)
├── WebSpark.HttpClientUtility.Crawler (depends on base)
├── WebSpark.HttpClientUtility.Test (tests base)
├── WebSpark.HttpClientUtility.Crawler.Test (tests crawler)
└── WebSpark.HttpClientUtility.Web (demo app)
```

---

## Package Update Strategy

### All Packages Must Be Updated

Following the assessment, these packages will be updated for .NET 10 compatibility:

| Package | Current Version | Target Version | Projects Affected | Priority |
|---------|----------------|----------------|-------------------|----------|
| Microsoft.Extensions.Caching.Abstractions | 8.0.0 | 10.0.0 | WebSpark.HttpClientUtility | High |
| Microsoft.Extensions.Caching.Memory | 8.0.1 | 10.0.0 | WebSpark.HttpClientUtility, WebSpark.HttpClientUtility.Test, WebSpark.HttpClientUtility.Web | High |
| Microsoft.Extensions.Http | 8.0.1 | 10.0.0 | WebSpark.HttpClientUtility | High |

**Note**: These are Microsoft.Extensions packages that move in lockstep with framework versions. They support multi-targeting and will work across net8.0, net9.0, and net10.0.

---

## Phase 1: Core Library Projects

### Phase 1.1: WebSpark.HttpClientUtility (Base Library)

**Risk Level**: Medium (published NuGet package, most critical)

**Current State**:
- Target Frameworks: `net8.0;net9.0`
- Lines of Code: ~2,500
- 3 package updates required

**Changes Required**:

1. **Update TargetFrameworks**
   ```xml
   <!-- Before -->
   <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
   
   <!-- After -->
   <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```

2. **Update Package References**
   ```xml
   <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="10.0.0" />
   <PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.0" />
   <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.0" />
   ```

**Validation Steps**:
- [ ] Project builds for net8.0 target without errors or warnings
- [ ] Project builds for net9.0 target without errors or warnings
- [ ] Project builds for net10.0 target without errors or warnings
- [ ] No package dependency conflicts
- [ ] Assembly signing still works (HttpClientUtility.snk)
- [ ] NuGet package metadata is correct

**Estimated Time**: 30 minutes

---

### Phase 1.2: WebSpark.HttpClientUtility.Crawler (Crawler Extension)

**Risk Level**: Medium (published NuGet package)

**Current State**:
- Target Frameworks: `net8.0;net9.0`
- Depends on: WebSpark.HttpClientUtility (exact version)
- Lines of Code: ~800

**Changes Required**:

1. **Update TargetFrameworks**
   ```xml
   <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```

2. **Verify Base Package Reference**
   ```xml
   <!-- Should reference the multi-targeted base package -->
   <ProjectReference Include="..\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility.csproj" />
   ```

**Validation Steps**:
- [ ] Project builds for all three targets
- [ ] Base package reference resolves correctly for all targets
- [ ] SignalR integration works
- [ ] No package conflicts
- [ ] Assembly signing still works

**Estimated Time**: 20 minutes

---

**Phase 1 Validation** (After both library projects updated):
- [ ] Both projects build together
- [ ] No cross-project dependency issues
- [ ] NuGet pack succeeds for both projects
- [ ] Generated .nupkg contains all three framework targets

---

## Phase 2: Test Projects

### Phase 2.1: WebSpark.HttpClientUtility.Test (Base Library Tests)

**Risk Level**: Low (test project, not published)

**Current State**:
- Target Frameworks: `net8.0;net9.0`
- 252+ MSTest tests
- 1 package update required

**Changes Required**:

1. **Update TargetFrameworks**
   ```xml
   <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```

2. **Update Package Reference**
   ```xml
   <PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.0" />
   ```

**Validation Steps**:
- [ ] Project builds for all targets
- [ ] All tests pass on net8.0
- [ ] All tests pass on net9.0
- [ ] All tests pass on net10.0
- [ ] Code coverage maintained

**Estimated Time**: 30 minutes (includes running tests)

---

### Phase 2.2: WebSpark.HttpClientUtility.Crawler.Test (Crawler Tests)

**Risk Level**: Low (test project)

**Current State**:
- Target Frameworks: `net8.0;net9.0`
- ~130 MSTest tests

**Changes Required**:

1. **Update TargetFrameworks**
   ```xml
   <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```

**Validation Steps**:
- [ ] Project builds for all targets
- [ ] All tests pass on net8.0
- [ ] All tests pass on net9.0
- [ ] All tests pass on net10.0

**Estimated Time**: 20 minutes

---

**Phase 2 Validation** (After both test projects updated):
- [ ] All 380+ tests pass across all framework targets
- [ ] No test flakiness or framework-specific failures
- [ ] Test coverage reports work correctly

---

## Phase 3: Demo Application

### Phase 3.1: WebSpark.HttpClientUtility.Web (ASP.NET Core Demo)

**Risk Level**: Low (demo application, not published)

**Current State**:
- Target Frameworks: `net8.0;net9.0`
- ASP.NET Core application
- 1 package update required

**Changes Required**:

1. **Update TargetFrameworks**
   ```xml
   <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
   ```

2. **Update Package Reference**
   ```xml
   <PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.0" />
   ```

**Validation Steps**:
- [ ] Project builds for all targets
- [ ] Application runs on net8.0
- [ ] Application runs on net9.0
- [ ] Application runs on net10.0
- [ ] All demo features work (HTTP requests, caching, crawler)
- [ ] SignalR hub works correctly

**Estimated Time**: 30 minutes

---

## Testing Strategy

### Multi-Level Testing Required

#### Per-Project Testing
After migrating each project:
- Verify builds succeed for net8.0, net9.0, and net10.0
- No compiler warnings
- No package restore errors
- Assembly signing works (for library projects)

#### Phase Testing
After completing each phase:
- All projects in phase build together
- Integration between projects works
- Test projects pass for all targets
- No dependency conflicts

#### Full Solution Testing
After all projects migrated:
- **Build Validation**:
  ```powershell
  dotnet build --configuration Release
  dotnet pack --configuration Release
  ```
- **Test Validation**:
  ```powershell
  dotnet test --configuration Release --framework net8.0
  dotnet test --configuration Release --framework net9.0
  dotnet test --configuration Release --framework net10.0
  ```
- **Package Validation**:
  - Inspect .nupkg with NuGet Package Explorer
  - Verify all three lib folders exist: lib/net8.0, lib/net9.0, lib/net10.0
  - Check dependencies are correct for each framework
- **Demo App Validation**:
  ```powershell
  cd WebSpark.HttpClientUtility.Web
  dotnet run --framework net10.0
  ```

### Testing Checklist

**For Each Library Project**:
- [ ] Builds without errors
- [ ] Builds without warnings
- [ ] All framework targets compile
- [ ] NuGet pack succeeds
- [ ] Assembly is strong-named
- [ ] Package metadata correct

**For Each Test Project**:
- [ ] All unit tests pass on net8.0
- [ ] All unit tests pass on net9.0
- [ ] All unit tests pass on net10.0
- [ ] No test framework compatibility issues
- [ ] Code coverage maintained

**For Demo App**:
- [ ] Runs successfully on all targets
- [ ] HTTP requests work
- [ ] Caching works
- [ ] Authentication providers work
- [ ] Resilience policies work
- [ ] Crawler features work
- [ ] SignalR works

---

## Risk Assessment

### Project Risk Levels

| Project | Risk Level | Rationale | Mitigation |
|---------|-----------|-----------|------------|
| WebSpark.HttpClientUtility | Medium | Published NuGet, most critical | Thorough testing, validate NuGet package structure |
| WebSpark.HttpClientUtility.Crawler | Medium | Published NuGet, depends on base | Verify base package reference, test SignalR |
| WebSpark.HttpClientUtility.Test | Low | Test project, extensive coverage | Run all tests for each target |
| WebSpark.HttpClientUtility.Crawler.Test | Low | Test project | Run all tests for each target |
| WebSpark.HttpClientUtility.Web | Low | Demo app, not published | Manual testing |

### Overall Risk Factors

**Low Risk Factors** ✅:
- Additive change (adding target, not removing)
- No breaking API changes
- Microsoft.Extensions packages are stable and multi-targeting friendly
- Existing net8.0 and net9.0 functionality unchanged
- Strong test coverage (380+ tests)
- .NET 10 is compatible with .NET 8/9 code patterns

**Medium Risk Factors** ⚠️:
- .NET 10 is in Preview (not RTM yet)
- Published NuGet package (needs careful validation)
- Multi-targeting can expose framework-specific issues
- Package version jumps (8.x → 10.0)

### Risk Mitigation Strategies

1. **Preview Framework Risk**:
   - Clearly document in README that net10.0 target requires .NET 10 Preview SDK
   - Keep net8.0 and net9.0 as stable, production-ready targets
   - Test thoroughly before publishing NuGet update

2. **NuGet Package Risk**:
   - Validate package structure with NuGet Package Explorer
   - Test package installation in fresh project
   - Verify backward compatibility (net8.0/net9.0 consumers unaffected)

3. **Multi-Targeting Issues**:
   - Build and test each framework target independently
   - Check for compiler directives if needed
   - Verify no framework-specific APIs used without guards

### Rollback Procedure

If critical issues are discovered at any phase:

1. **Before Committing**:
   ```powershell
   git checkout main
   git branch -D upgrade-to-NET10
   ```

2. **After Committing (revert last commit)**:
   ```powershell
   git revert HEAD
   git push origin upgrade-to-NET10
   ```

3. **After Merging to Main**:
   - Revert the merge commit
   - Publish hotfix NuGet package with net8.0;net9.0 only
   - Create incident report

---

## Breaking Changes & Code Modifications

### Expected Breaking Changes: NONE

**Good News**: Multi-targeting by adding net10.0 is a **non-breaking change** for consumers.

- Existing net8.0 consumers: Continue using net8.0 build (no changes)
- Existing net9.0 consumers: Continue using net9.0 build (no changes)
- New net10.0 consumers: Can now use net10.0 build

### Code Modifications: MINIMAL

**Project Files Only**:
- No C# code changes expected
- Only .csproj file updates (TargetFrameworks and PackageReferences)
- No API changes
- No configuration changes

### Potential Compiler Warnings

If any warnings appear for net10.0 target:
- Address obsolete API warnings
- Add framework-specific directives if needed:
  ```csharp
  #if NET10_0_OR_GREATER
  // .NET 10+ specific code
  #else
  // .NET 8/9 code
  #endif
  ```

---

## Success Criteria

### Technical Criteria ✅

The migration is complete when:

1. **Framework Targets**:
   - All projects target `net8.0;net9.0;net10.0`
   - All three targets build without errors
   - All three targets build without warnings

2. **Package Updates**:
   - All Microsoft.Extensions packages updated to 10.0.0
   - No package dependency conflicts
   - No security vulnerabilities

3. **Build Success**:
   - `dotnet build --configuration Release` succeeds
   - `dotnet pack --configuration Release` succeeds
   - Generated .nupkg contains all three framework targets

4. **Test Success**:
   - All 380+ tests pass on net8.0
   - All 380+ tests pass on net9.0
   - All 380+ tests pass on net10.0
   - No framework-specific test failures

5. **Package Integrity**:
   - NuGet package structure is correct
   - lib/net8.0, lib/net9.0, lib/net10.0 folders present
   - Dependencies correctly specified per framework
   - Strong-name signature intact

### Quality Criteria ✅

1. **Documentation**:
   - README.md updated with .NET 10 support
   - CHANGELOG.md entry created
   - NuGet release notes prepared
   - .NET 10 SDK requirement documented

2. **Code Quality**:
   - No new compiler warnings
   - Code analysis passes
   - Nullable reference handling correct

3. **Testing**:
   - Code coverage maintained (>80%)
   - Performance benchmarks stable
   - Integration tests pass

### Operational Criteria ✅

1. **CI/CD**:
   - GitHub Actions workflow builds all targets
   - Test execution works for all targets
   - NuGet package publishing workflow ready

2. **Deployment**:
   - Version number incremented (follow SemVer)
   - Git tag created
   - GitHub release prepared

3. **Communication**:
   - Team notified of .NET 10 Preview requirement
   - Release notes clearly state Preview status
   - Migration guide prepared for consumers

---

## Timeline & Milestones

### Estimated Timeline: 2-4 Hours

| Phase | Tasks | Estimated Time | Milestone |
|-------|-------|---------------|-----------|
| **Phase 1** | Core library projects | 50 minutes | Libraries multi-target |
| **Phase 2** | Test projects | 50 minutes | All tests pass |
| **Phase 3** | Demo application | 30 minutes | Demo app works |
| **Validation** | Full solution testing | 30 minutes | Solution complete |
| **Documentation** | Update docs & CHANGELOG | 20 minutes | Ready to commit |
| **Buffer** | Unexpected issues | 30-90 minutes | - |

### Key Milestones

1. ✅ **Analysis Complete** (Done)
2. ⏳ **Planning Complete** (Current)
3. 🎯 **Phase 1 Complete**: Both library projects multi-target
4. 🎯 **Phase 2 Complete**: All tests pass on all targets
5. 🎯 **Phase 3 Complete**: Demo app works on all targets
6. 🎯 **Validation Complete**: Full solution builds, tests, and packages
7. 🎯 **Documentation Complete**: README, CHANGELOG updated
8. 🎯 **Ready for Merge**: Branch ready for PR

---

## Post-Migration Checklist

### Before Creating Pull Request

- [ ] All projects build successfully
- [ ] All tests pass on all framework targets
- [ ] NuGet package structure validated
- [ ] CHANGELOG.md updated
- [ ] README.md updated with .NET 10 info
- [ ] Version number incremented in .csproj files
- [ ] Git commit messages are clear
- [ ] No debug code or temporary changes left

### Pull Request Requirements

- [ ] PR title: "Add .NET 10.0 multi-targeting support"
- [ ] PR description includes:
  - Summary of changes
  - Testing performed
  - Breaking changes: NONE
  - .NET 10 SDK requirement
  - Link to CHANGELOG entry
- [ ] All CI checks pass
- [ ] Code review completed

### Post-Merge Actions

- [ ] Create Git tag (e.g., v2.1.0)
- [ ] Verify GitHub Actions builds all targets
- [ ] Verify GitHub Actions publishes NuGet package
- [ ] Test NuGet package installation in fresh project
- [ ] Announce release (GitHub Releases, social media)
- [ ] Monitor for issues in first 48 hours

---

## Appendix

### Package Version Matrix

| Package | .NET 8 | .NET 9 | .NET 10 | Notes |
|---------|--------|--------|---------|-------|
| Microsoft.Extensions.Caching.Abstractions | 10.0.0 | 10.0.0 | 10.0.0 | Multi-targeting compatible |
| Microsoft.Extensions.Caching.Memory | 10.0.0 | 10.0.0 | 10.0.0 | Multi-targeting compatible |
| Microsoft.Extensions.Http | 10.0.0 | 10.0.0 | 10.0.0 | Multi-targeting compatible |

### Required SDK Versions

- **.NET 8 SDK**: 8.0.x (LTS)
- **.NET 9 SDK**: 9.0.x (STS)
- **.NET 10 SDK**: 10.0.x-preview.x (Preview)

**Developer Setup**: Install .NET 10 Preview SDK from https://dotnet.microsoft.com/download/dotnet/10.0

### Reference Documentation

- [.NET 10 Preview Announcement](https://devblogs.microsoft.com/dotnet/)
- [Multi-Targeting Guide](https://docs.microsoft.com/dotnet/standard/frameworks)
- [Microsoft.Extensions 10.0.0 Release Notes](https://github.com/dotnet/extensions)
- [Breaking Changes in .NET 10](https://docs.microsoft.com/dotnet/core/compatibility/10.0)

### Commands Reference

```powershell
# Build all targets
dotnet build --configuration Release

# Test specific target
dotnet test --framework net10.0

# Pack NuGet package
dotnet pack --configuration Release --output ./nupkg

# Inspect package
# Use NuGet Package Explorer or:
unzip WebSpark.HttpClientUtility.X.Y.Z.nupkg -d temp/

# Run demo app
dotnet run --project WebSpark.HttpClientUtility.Web --framework net10.0
```

---

## Next Steps

This plan is now ready for execution. The next stage will be **Execution**, where each phase will be implemented step-by-step following this plan.

**Recommended Approach**: Execute phases sequentially, validating thoroughly at each step before proceeding.

---

*Plan generated by .NET Upgrade Assistant - Planning Agent*
*Based on assessment: C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility\.github\upgrades\assessment.md*