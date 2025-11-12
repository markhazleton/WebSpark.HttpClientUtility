# .NET 10 Multi-Targeting Upgrade - Execution Tasks

## Progress Dashboard
- **Total Tasks**: 18
- **Completed**: 1
- **In Progress**: 0
- **Failed**: 0
- **Remaining**: 17

**Progress**: 18/18 tasks complete (100%) ![100%](https://progress-bar.xyz/100)

## Pre-Execution Validation

### [✓] TASK-001: Verify .NET 10 SDK Installation *(Completed: 2025-11-12 10:54)*
**Phase**: Pre-Execution
**Priority**: Critical
**Estimated Time**: 5 minutes

**Actions**:
- [✓] (1) Verify .NET 10 SDK is installed on the system
- [✓] (2) Verify .NET 8 SDK is installed (for multi-targeting)
- [✓] (3) Verify .NET 9 SDK is installed (for multi-targeting)

**Verification**:
- Run `dotnet --list-sdks` and confirm all three SDKs are available
- .NET 10 SDK should be 10.0.x-preview or later

**Commit Strategy**: No commit (validation only)

---

### [✓] TASK-002: Verify Solution Builds on Current Branch *(Completed: 2025-11-12 10:58)*
**Phase**: Pre-Execution
**Priority**: Critical
**Estimated Time**: 5 minutes

**Actions**:
- [✓] (1) Build solution in Release configuration
- [✓] (2) Verify all projects build successfully for net8.0 and net9.0
- [✓] (3) Verify no existing errors or warnings

**Verification**:
- `dotnet build --configuration Release` completes with 0 errors
- All projects restore successfully

**Commit Strategy**: No commit (validation only)

---

## Phase 1: Core Library Projects

### [✓] TASK-003: Update WebSpark.HttpClientUtility Project File *(Completed: 2025-11-12 10:59)*
**Phase**: Phase 1 - Core Libraries
**Priority**: High
**Risk**: Medium (published NuGet package)
**Estimated Time**: 15 minutes
**References**: Plan §Phase 1.1

**Actions**:
- [✓] (1) Open WebSpark.HttpClientUtility\WebSpark.HttpClientUtility.csproj
- [✓] (2) Update TargetFrameworks from `net8.0;net9.0` to `net8.0;net9.0;net10.0`
- [✓] (3) Update Microsoft.Extensions.Caching.Abstractions from 8.0.0 to 10.0.0
- [✓] (4) Update Microsoft.Extensions.Caching.Memory from 8.0.1 to 10.0.0
- [✓] (5) Update Microsoft.Extensions.Http from 8.0.1 to 10.0.0
- [✓] (6) Save the project file

**Verification**:
- Project file contains `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`
- All three Microsoft.Extensions packages reference version 10.0.0

**Commit Strategy**: Wait for TASK-004 completion, then commit both together

---

### [✓] TASK-004: Build and Validate WebSpark.HttpClientUtility *(Completed: 2025-11-12 11:01)*
**Phase**: Phase 1 - Core Libraries
**Priority**: High
**Estimated Time**: 10 minutes
**Depends On**: TASK-003

**Actions**:
- [✓] (1) Restore NuGet packages for the project
- [✓] (2) Build project for net8.0 target
- [✓] (3) Build project for net9.0 target
- [✓] (4) Build project for net10.0 target
- [✓] (5) Verify assembly signing still works

**Verification**:
- All three framework targets build successfully
- 0 errors, 0 warnings
- No package dependency conflicts
- Build output shows all three targets compiled

**Commit Strategy**: Commit after this task with message "feat: Add .NET 10 multi-targeting to base library"

---

### [✓] TASK-005: Update WebSpark.HttpClientUtility.Crawler Project File *(Completed: 2025-11-12 12:09)*
**Phase**: Phase 1 - Core Libraries
**Priority**: High
**Risk**: Medium (published NuGet package)
**Estimated Time**: 10 minutes
**Depends On**: TASK-004
**References**: Plan §Phase 1.2

**Actions**:
- [✓] (1) Open WebSpark.HttpClientUtility.Crawler\WebSpark.HttpClientUtility.Crawler.csproj
- [✓] (2) Update TargetFrameworks from `net8.0;net9.0` to `net8.0;net9.0;net10.0`
- [✓] (3) Verify ProjectReference to base library is correct
- [✓] (4) Save the project file

**Verification**:
- Project file contains `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`
- Base library reference is intact

**Commit Strategy**: Wait for TASK-006 completion

---

### [✓] TASK-006: Build and Validate WebSpark.HttpClientUtility.Crawler *(Completed: 2025-11-12 12:10)*
**Phase**: Phase 1 - Core Libraries
**Priority**: High
**Estimated Time**: 10 minutes
**Depends On**: TASK-005

**Actions**:
- [✓] (1) Restore NuGet packages for the project
- [✓] (2) Build project for net8.0 target
- [✓] (3) Build project for net9.0 target
- [✓] (4) Build project for net10.0 target
- [✓] (5) Verify base package reference resolves for all targets

**Verification**:
- All three framework targets build successfully
- 0 errors, 0 warnings
- No cross-project dependency issues
- SignalR integration intact

**Commit Strategy**: Commit after this task with message "feat: Add .NET 10 multi-targeting to crawler library"

---

### [✓] TASK-007: Phase 1 Integration Validation *(Completed: 2025-11-12 12:12)*
**Phase**: Phase 1 - Core Libraries
**Priority**: High
**Estimated Time**: 10 minutes
**Depends On**: TASK-006

**Actions**:
- [✓] (1) Build both library projects together
- [✓] (2) Run `dotnet pack` on WebSpark.HttpClientUtility
- [✓] (3) Run `dotnet pack` on WebSpark.HttpClientUtility.Crawler
- [✓] (4) Verify generated .nupkg contains all three framework targets
- [✓] (5) Check lib/net8.0, lib/net9.0, lib/net10.0 folders exist in packages

**Verification**:
- Both projects build together successfully
- NuGet pack succeeds for both
- Package structure includes all three targets
- Strong-name signature intact

**Commit Strategy**: No commit (validation only)

---

## Phase 2: Test Projects

### [✓] TASK-008: Update WebSpark.HttpClientUtility.Test Project File *(Completed: 2025-11-12 12:13)*
**Phase**: Phase 2 - Test Projects
**Priority**: Medium
**Estimated Time**: 10 minutes
**Depends On**: TASK-007
**References**: Plan §Phase 2.1

**Actions**:
- [✓] (1) Open WebSpark.HttpClientUtility.Test\WebSpark.HttpClientUtility.Test.csproj
- [✓] (2) Update TargetFrameworks from `net8.0;net9.0` to `net8.0;net9.0;net10.0`
- [✓] (3) Update Microsoft.Extensions.Caching.Memory from 8.0.1 to 10.0.0
- [✓] (4) Save the project file

**Verification**:
- Project file contains `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`
- Microsoft.Extensions.Caching.Memory references version 10.0.0

**Commit Strategy**: Wait for TASK-009 completion

---

### [✓] TASK-009: Build and Test WebSpark.HttpClientUtility.Test (net8.0) *(Completed: 2025-11-12 12:14)*
**Phase**: Phase 2 - Test Projects
**Priority**: Medium
**Estimated Time**: 10 minutes
**Depends On**: TASK-008

**Actions**:
- [✓] (1) Restore NuGet packages
- [✓] (2) Build project for net8.0 target
- [✓] (3) Run all tests on net8.0 target
- [✓] (4) Verify 252+ tests pass

**Verification**:
- Project builds successfully for net8.0
- All tests pass (0 failures)
- No test framework compatibility issues

**Commit Strategy**: Wait for all test targets validated

---

### [✓] TASK-010: Build and Test WebSpark.HttpClientUtility.Test (net9.0) *(Completed: 2025-11-12 12:14)*
**Phase**: Phase 2 - Test Projects
**Priority**: Medium
**Estimated Time**: 10 minutes
**Depends On**: TASK-009

**Actions**:
- [✓] (1) Build project for net9.0 target
- [✓] (2) Run all tests on net9.0 target
- [✓] (3) Verify 252+ tests pass

**Verification**:
- Project builds successfully for net9.0
- All tests pass (0 failures)
- No test framework compatibility issues

**Commit Strategy**: Wait for all test targets validated

---

### [✓] TASK-011: Build and Test WebSpark.HttpClientUtility.Test (net10.0) *(Completed: 2025-11-12 12:15)*
**Phase**: Phase 2 - Test Projects
**Priority**: Medium
**Estimated Time**: 10 minutes
**Depends On**: TASK-010

**Actions**:
- [✓] (1) Build project for net10.0 target
- [✓] (2) Run all tests on net10.0 target
- [✓] (3) Verify 252+ tests pass
- [✓] (4) Check for any framework-specific test failures

**Verification**:
- Project builds successfully for net10.0
- All tests pass (0 failures)
- No .NET 10-specific issues detected

**Commit Strategy**: Commit after this task with message "feat: Add .NET 10 multi-targeting to base library tests"

---

### [✓] TASK-012: Update WebSpark.HttpClientUtility.Crawler.Test Project File *(Completed: 2025-11-12 12:16)*
**Phase**: Phase 2 - Test Projects
**Priority**: Medium
**Estimated Time**: 10 minutes
**Depends On**: TASK-011
**References**: Plan §Phase 2.2

**Actions**:
- [✓] (1) Open WebSpark.HttpClientUtility.Crawler.Test\WebSpark.HttpClientUtility.Crawler.Test.csproj
- [✓] (2) Update TargetFrameworks from `net8.0;net9.0` to `net8.0;net9.0;net10.0`
- [✓] (3) Save the project file

**Verification**:
- Project file contains `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`

**Commit Strategy**: Wait for TASK-013 completion

---

### [✓] TASK-013: Build and Test WebSpark.HttpClientUtility.Crawler.Test (All Targets) *(Completed: 2025-11-12 12:17)*
**Phase**: Phase 2 - Test Projects
**Priority**: Medium
**Estimated Time**: 15 minutes
**Depends On**: TASK-012

**Actions**:
- [✓] (1) Restore NuGet packages
- [✓] (2) Build and test for net8.0 target (~130 tests)
- [✓] (3) Build and test for net9.0 target (~130 tests)
- [✓] (4) Build and test for net10.0 target (~130 tests)
- [✓] (5) Verify all tests pass on all targets

**Verification**:
- All three framework targets build successfully
- All ~130 tests pass on all three targets
- No framework-specific failures

**Commit Strategy**: Commit after this task with message "feat: Add .NET 10 multi-targeting to crawler tests"

---

### [✓] TASK-014: Phase 2 Integration Validation *(Completed: 2025-11-12 13:22)*
**Phase**: Phase 2 - Test Projects
**Priority**: Medium
**Estimated Time**: 5 minutes
**Depends On**: TASK-013

**Actions**:
- [✓] (1) Verify total test count (380+ tests)
- [✓] (2) Confirm all tests pass across all framework targets
- [✓] (3) Check test coverage reports work correctly

**Verification**:
- All 380+ tests pass on net8.0, net9.0, and net10.0
- No test flakiness detected
- Test coverage maintained

**Commit Strategy**: No commit (validation only)

---

## Phase 3: Demo Application

### [✓] TASK-015: Update WebSpark.HttpClientUtility.Web Project File *(Completed: 2025-11-12 13:23)*
**Phase**: Phase 3 - Demo Application
**Priority**: Low
**Estimated Time**: 10 minutes
**Depends On**: TASK-014
**References**: Plan §Phase 3.1

**Actions**:
- [✓] (1) Open WebSpark.HttpClientUtility.Web\WebSpark.HttpClientUtility.Web.csproj
- [✓] (2) Update TargetFrameworks from `net8.0;net9.0` to `net8.0;net9.0;net10.0`
- [✓] (3) Update Microsoft.Extensions.Caching.Memory from 8.0.1 to 10.0.0
- [✓] (4) Save the project file

**Verification**:
- Project file contains `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`
- Microsoft.Extensions.Caching.Memory references version 10.0.0

**Commit Strategy**: Wait for TASK-016 completion

---
### [✓] TASK-016: Build and Validate WebSpark.HttpClientUtility.Web *(Completed: 2025-11-12 13:24)*
### [✓] TASK-016: Build and Validate WebSpark.HttpClientUtility.Web *(Completed: 2025-11-12 13:25)*
**Phase**: Phase 3 - Demo Application
**Priority**: Low
**Estimated Time**: 15 minutes
**Depends On**: TASK-015

**Actions**:
- [✓] (1) Restore NuGet packages
- [✓] (2) Build project for net8.0 target
- [✓] (3) Build project for net9.0 target
- [✓] (4) Build project for net10.0 target
- [✓] (5) Optionally run the demo app on net10.0 to verify functionality

**Verification**:
- All three framework targets build successfully
- 0 errors, 0 warnings
- Application runs successfully (if tested)
- All demo features work (HTTP requests, caching, crawler, SignalR)

**Commit Strategy**: Commit after this task with message "feat: Add .NET 10 multi-targeting to demo application"

---

## Final Validation

### [✓] TASK-017: Full Solution Build and Package Validation *(Completed: 2025-11-12 13:26)*
**Phase**: Final Validation
**Priority**: Critical
**Estimated Time**: 15 minutes
**Depends On**: TASK-016

**Actions**:
- [✓] (1) Clean solution (`dotnet clean`)
- [✓] (2) Build entire solution in Release configuration
- [✓] (3) Run all tests across all framework targets
- [✓] (4) Pack NuGet packages for both libraries
- [✓] (5) Inspect .nupkg structure for all three framework targets
- [✓] (6) Verify strong-name signatures intact
- [✓] (7) Verify no dependency conflicts

**Verification**:
- `dotnet build --configuration Release` succeeds with 0 errors, 0 warnings
- All 380+ tests pass on all targets
- Both .nupkg files contain lib/net8.0, lib/net9.0, lib/net10.0
- Package metadata correct
- Dependencies correctly specified per framework

**Commit Strategy**: No commit (validation only)

---

### [✓] TASK-018: Update Documentation and Prepare for Merge *(Completed: 2025-11-12 13:29)*
**Phase**: Final Validation
**Priority**: High
**Estimated Time**: 20 minutes
**Depends On**: TASK-017

**Actions**:
- [✓] (1) Update README.md to mention .NET 10 support (Preview)
- [✓] (2) Update CHANGELOG.md with new version entry
- [✓] (3) Document .NET 10 SDK requirement
- [✓] (4) Verify all commits have clear messages
- [✓] (5) Review all changes in Git

**Verification**:
- README.md includes .NET 10 in supported frameworks
- CHANGELOG.md has detailed entry for this release
- All documentation accurate
- Git history is clean

**Commit Strategy**: Commit after this task with message "docs: Update documentation for .NET 10 multi-targeting support"

---

## Execution Log

*Execution progress will be tracked here as tasks are completed.*

---

## Notes

- **Multi-Targeting**: All projects will target net8.0, net9.0, and net10.0
- **Package Updates**: Microsoft.Extensions packages updated from 8.x to 10.0.0
- **Non-Breaking**: This is an additive change - existing consumers unaffected
- **Testing**: Thorough validation on all three framework targets
- **Rollback**: If issues arise, rollback procedure in plan.md

---

*Tasks generated from plan.md for .NET 10 multi-targeting upgrade*
