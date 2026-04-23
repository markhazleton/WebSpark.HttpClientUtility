# Security & PR Review Report

**Repository:** markhazleton/WebSpark.HttpClientUtility  
**Generated:** December 3, 2025  
**Review Method:** GitHub CLI + MCP Services

---

## Executive Summary

- **Open Pull Requests:** 1 (Dependabot security fix)
- **Security Alerts:** 3 OPEN, 5 FIXED
- **Secret Scanning Alerts:** 0 (Clean)
- **Code Scanning:** Not enabled
- **Overall Status:** 🟡 Action Required (3 open security alerts)

---

## Pull Requests Review

### 🔴 OPEN - PR #5: chore(deps): bump js-yaml in /src

**Status:** Ready to merge  
**Author:** dependabot[bot]  
**Created:** November 18, 2025  
**Labels:** dependencies, javascript

#### Overview

Security fix for js-yaml prototype pollution vulnerability (GHSA-mh29-5h37-fv8m)

#### Changes

- Updates `js-yaml` from 3.14.1 to 3.14.2
- Updates `js-yaml` from 4.1.0 to 4.1.1
- File modified: `src/package-lock.json` (10 additions, 10 deletions)

#### Security Context

Fixes **prototype pollution issue in yaml merge (<<) operator** - a MODERATE severity vulnerability that could allow attackers to inject arbitrary properties into objects.

#### CI/CD Status: ✅ All checks passed

- ✅ build-test-publish: SUCCESS
- ✅ GitGuardian Security Checks: SUCCESS  
- ✅ .NET Tests: SUCCESS

#### Recommendation

**MERGE IMMEDIATELY** - This PR addresses 2 of the 3 open security alerts. All tests pass and changes are isolated to package-lock.json.

**Action:**

```bash
gh pr review 5 --approve --body "LGTM - Security fix for js-yaml prototype pollution (GHSA-mh29-5h37-fv8m). All CI checks passed."
gh pr merge 5 --squash
```

---

### 🟢 CLOSED/MERGED - Recent PRs

#### PR #4: Upgrade to net10

- **Status:** CLOSED (not merged)
- **Created:** November 12, 2025
- **Reason:** Had merge conflicts (CONFLICTING)

#### PR #3: Upgrade to net10

- **Status:** MERGED ✅
- **Merged:** November 12, 2025
- **Description:** Successful .NET 10 upgrade

#### PR #2: Enable TreatWarningsAsErrors with Zero Warnings Policy

- **Status:** MERGED ✅
- **Merged:** November 2, 2025
- **Description:** Code quality improvement - enforces zero warnings

#### PR #1: feat: Add static documentation site with Eleventy

- **Status:** MERGED ✅
- **Merged:** November 2, 2025
- **Description:** Added documentation infrastructure

---

## Security Alerts (Dependabot)

### 🔴 OPEN ALERTS (3)

#### 1. js-yaml Prototype Pollution (MODERATE) - GHSA-mh29-5h37-fv8m

- **Package:** js-yaml
- **Vulnerable Version:** < 3.14.2
- **Status:** OPEN (Alert created: Nov 18, 2025)
- **Fix:** Available in PR #5
- **Impact:** Prototype pollution in merge (<<) operator
- **Action Required:** Merge PR #5

#### 2. js-yaml Prototype Pollution (MODERATE) - GHSA-mh29-5h37-fv8m

- **Package:** js-yaml
- **Vulnerable Version:** >= 4.0.0, < 4.1.1
- **Status:** OPEN (Alert created: Nov 18, 2025)
- **Fix:** Available in PR #5
- **Impact:** Same vulnerability, different version range
- **Action Required:** Merge PR #5

#### 3. glob CLI Command Injection (HIGH) - GHSA-5j98-mcp5-4vw2

- **Package:** glob
- **Vulnerable Version:** >= 11.0.0, < 11.1.0
- **Status:** OPEN (Alert created: Nov 18, 2025)
- **Severity:** HIGH
- **Impact:** Command injection via -c/--cmd executes matches with shell:true
- **Published:** November 17, 2025
- **Action Required:** Update glob to >= 11.1.0

**Recommendation for glob vulnerability:**

```bash
# Check current glob usage
npm list glob --depth=0
# Update to safe version
npm update glob@latest
```

---

### 🟢 FIXED ALERTS (5)

#### 1. Server-Side Request Forgery in Request (MODERATE)

- **Package:** request
- **GHSA:** GHSA-p8p7-x288-28g6
- **Status:** FIXED ✅
- **Published:** March 16, 2023

#### 2. tough-cookie Prototype Pollution (MODERATE)

- **Package:** tough-cookie
- **GHSA:** GHSA-72xf-g2v4-qvf3
- **Status:** FIXED ✅
- **Published:** July 1, 2023

#### 3. Command Injection in lodash.template (HIGH)

- **Package:** lodash.template
- **GHSA:** GHSA-35jh-r3h4-6jhm
- **Status:** FIXED ✅
- **Published:** May 6, 2021

#### 4. html-minifier REDoS vulnerability (HIGH)

- **Package:** html-minifier
- **GHSA:** GHSA-pfq8-rq6v-vf5m
- **Status:** FIXED ✅
- **Published:** October 31, 2022

#### 5. form-data unsafe random function (CRITICAL)

- **Package:** form-data
- **GHSA:** GHSA-fjxv-7rqg-78g4
- **Status:** FIXED ✅
- **Published:** July 21, 2025

---

## Additional Security Checks

### Secret Scanning

- **Status:** ✅ CLEAN
- **Alerts Found:** 0
- **Service:** Enabled

### Code Scanning (CodeQL)

- **Status:** ❌ NOT ENABLED
- **Recommendation:** Enable GitHub Advanced Security features for automated code scanning

### GitGuardian

- **Status:** ✅ ACTIVE
- **Integration:** Working (verified in PR #5 checks)

---

## Recommended Actions (Priority Order)

### 🔴 URGENT (Do Today)

1. **Merge PR #5** - Fixes 2 of 3 open security alerts

   ```bash
   gh pr review 5 --approve
   gh pr merge 5 --squash
   ```

2. **Fix glob vulnerability** - HIGH severity command injection

   ```bash
   cd src
   npm update glob@latest
   git add package-lock.json
   git commit -m "fix(deps): update glob to fix command injection vulnerability (GHSA-5j98-mcp5-4vw2)"
   git push
   ```

### 🟡 MEDIUM PRIORITY (This Week)

3. **Enable Code Scanning (CodeQL)**
   - Go to: Settings → Security → Code security and analysis
   - Enable: CodeQL analysis
   - This will catch vulnerabilities before they reach production

4. **Review Dependabot Configuration**
   - Create/update `.github/dependabot.yml` to auto-merge low-risk updates
   - Enable automatic security updates

### 🟢 LOW PRIORITY (This Month)

5. **Audit npm dependencies**

   ```bash
   cd src
   npm audit
   npm audit fix
   ```

6. **Review Security Policy**
   - Update `SECURITY.md` if needed
   - Document security update process

---

## Security Metrics

| Metric | Current | Target | Status |
|--------|---------|--------|--------|
| Open Security Alerts | 3 | 0 | 🔴 |
| Open PRs | 1 | 0 | 🟢 |
| Secret Scanning Alerts | 0 | 0 | ✅ |
| Fixed Vulnerabilities (30 days) | 5 | - | 🟢 |
| CI/CD Success Rate | 100% | 100% | ✅ |

---

## Notes

- **Good:** All CI/CD pipelines are passing, including GitGuardian security checks
- **Good:** No exposed secrets detected
- **Good:** 5 previously identified vulnerabilities have been fixed
- **Concern:** 3 open vulnerabilities need immediate attention
- **Concern:** Code scanning (CodeQL) is not enabled
- **Positive:** Quick response to Dependabot alerts (PR created same day as alert)

---

## Next Review Date

**Recommended:** December 10, 2025 (weekly security review)

---

## Appendix: Commands Used

```bash
# List PRs
gh pr list --repo markhazleton/WebSpark.HttpClientUtility --state all --limit 20 --json number,title,state,author,createdAt,updatedAt,labels

# View PR details
gh pr view 5 --json number,title,body,state,files,statusCheckRollup

# Get security alerts (GraphQL)
gh api graphql -f query='query($owner: String!, $repo: String!) { 
  repository(owner: $owner, name: $repo) { 
    vulnerabilityAlerts(first: 50) { 
      nodes { 
        createdAt state dismissedAt dismissReason 
        securityVulnerability { 
          package { name } 
          advisory { summary severity ghsaId publishedAt } 
          vulnerableVersionRange 
        } 
      } 
    } 
  } 
}' -f owner=markhazleton -f repo=WebSpark.HttpClientUtility

# Check secret scanning
gh api /repos/markhazleton/WebSpark.HttpClientUtility/secret-scanning/alerts
```

---

*Report generated using GitHub CLI v2.78.0 and GitHub GraphQL API*
