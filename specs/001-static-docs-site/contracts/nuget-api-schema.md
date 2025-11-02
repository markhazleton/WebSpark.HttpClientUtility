# Contract: NuGet API Schema

**Endpoint**: `https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility`  
**Method**: GET  
**Authentication**: None (public API)  
**Rate Limit**: No documented limit (use responsibly)

## Response Structure

### Successful Response (HTTP 200)

```json
{
  "@context": {
    "@vocab": "http://schema.nuget.org/schema#",
    "@base": "https://api.nuget.org/v3/registration5-gz-semver2/"
  },
  "totalHits": 1,
  "data": [
    {
      "@id": "https://api.nuget.org/v3/registration5-gz-semver2/webspark.httpclientutility/index.json",
      "@type": "Package",
      "registration": "https://api.nuget.org/v3/registration5-gz-semver2/webspark.httpclientutility/index.json",
      "id": "WebSpark.HttpClientUtility",
      "version": "1.4.0",
      "description": "A production-ready HttpClient wrapper for .NET 8+ with resilience, caching, telemetry, and web crawling capabilities.",
      "summary": "",
      "title": "WebSpark.HttpClientUtility",
      "iconUrl": "https://api.nuget.org/v3-flatcontainer/webspark.httpclientutility/1.4.0/icon",
      "licenseUrl": "https://licenses.nuget.org/MIT",
      "projectUrl": "https://github.com/markhazleton/WebSpark.HttpClientUtility",
      "tags": [
        "http",
        "httpclient",
        "resilience",
        "caching",
        "telemetry",
        "polly",
        "web-crawler"
      ],
      "authors": [
        "Mark Hazleton"
      ],
      "totalDownloads": 15234,
      "verified": false,
      "packageTypes": [
        {
          "name": "Dependency"
        }
      ],
      "versions": [
        {
          "version": "1.0.0",
          "@id": "https://api.nuget.org/v3/registration5-gz-semver2/webspark.httpclientutility/1.0.0.json"
        },
        {
          "version": "1.1.0",
          "@id": "https://api.nuget.org/v3/registration5-gz-semver2/webspark.httpclientutility/1.1.0.json"
        },
        {
          "version": "1.4.0",
          "@id": "https://api.nuget.org/v3/registration5-gz-semver2/webspark.httpclientutility/1.4.0.json"
        }
      ]
    }
  ]
}
```

## Field Definitions

### Root Level

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `@context` | Object | Yes | JSON-LD context (ignore for our purposes) |
| `totalHits` | Integer | Yes | Number of packages matched (should be 1 for exact ID search) |
| `data` | Array | Yes | Array of package results (first element is our package) |

### Package Object (data[0])

| Field | Type | Required | Description | Usage |
|-------|------|----------|-------------|-------|
| `id` | String | Yes | Package ID (case-insensitive) | Verify match: "WebSpark.HttpClientUtility" |
| `version` | String | Yes | Current/latest version (SemVer) | Display on homepage, installation instructions |
| `description` | String | Yes | Package description | Display on homepage hero section |
| `title` | String | No | Display title (usually same as id) | Use as fallback for display name |
| `authors` | Array[String] | Yes | Package authors | Display in footer or about page |
| `totalDownloads` | Integer | Yes | Cumulative downloads across all versions | Display on homepage (formatted: "15.2K") |
| `projectUrl` | String | No | Project homepage URL | Link to GitHub repository |
| `licenseUrl` | String | No | License information URL | Link in footer |
| `iconUrl` | String | No | Package icon URL | Display as logo (if available) |
| `tags` | Array[String] | No | Package tags/keywords | Use for metadata/SEO |
| `verified` | Boolean | Yes | Whether package is verified | Badge display (future enhancement) |
| `versions` | Array[Object] | No | All available versions | Version history page (future enhancement) |

## Data Extraction Contract

### Required Fields for Build

The build process MUST extract these fields:

```javascript
// In src/_data/nuget.js
const packageData = response.data[0];

return {
  version: packageData.version,           // REQUIRED: "1.4.0"
  downloads: packageData.totalDownloads,  // REQUIRED: 15234
  description: packageData.description,   // REQUIRED: Package description
  projectUrl: packageData.projectUrl,     // OPTIONAL: GitHub URL
  licenseUrl: packageData.licenseUrl,     // OPTIONAL: License URL
  authors: packageData.authors,           // OPTIONAL: Array of author names
  tags: packageData.tags                  // OPTIONAL: Array of tags
};
```

### Computed Fields

Additional fields computed from API response:

```javascript
{
  // Format download count for display
  displayDownloads: formatNumber(packageData.totalDownloads), // "15.2K"
  
  // Last fetch timestamp
  lastUpdate: new Date().toISOString(), // "2025-11-02T10:30:00Z"
  
  // Cache status
  cached: false, // true if loaded from cache, false if fresh from API
  
  // Error flag
  error: false,  // true if API failed and using ultimate fallback
  
  // Human-readable cache timestamp
  cacheTimestamp: formatDate(new Date()) // "Nov 2, 2025"
}
```

## Error Handling

### HTTP Status Codes

| Status | Meaning | Handling |
|--------|---------|----------|
| 200 | Success | Parse response, cache, and use |
| 404 | Package not found | Log error, use cached data or fallback |
| 429 | Rate limited | Wait and retry once, then use cache |
| 500 | Server error | Log error, use cached data |
| Network error | Connection failed | Use cached data immediately |

### Error Response Structure

```json
{
  "error": {
    "code": "PackageNotFound",
    "message": "The package 'WebSpark.HttpClientUtility' could not be found."
  }
}
```

## Caching Strategy

### Cache File Structure

**Location**: `/src/_data/nuget-cache.json`

**Format**:
```json
{
  "version": "1.4.0",
  "totalDownloads": 15234,
  "description": "A production-ready HttpClient wrapper...",
  "projectUrl": "https://github.com/markhazleton/WebSpark.HttpClientUtility",
  "licenseUrl": "https://licenses.nuget.org/MIT",
  "authors": ["Mark Hazleton"],
  "tags": ["http", "httpclient", "resilience", "caching"],
  "cachedAt": "2025-11-02T10:30:00Z",
  "fetchedFrom": "https://azuresearch-usnc.nuget.org/query"
}
```

### Cache Logic

```javascript
// Pseudo-code for cache strategy
async function fetchNuGetData() {
  try {
    // 1. Attempt API fetch
    const response = await fetch(API_URL);
    const data = await response.json();
    
    // 2. Validate response
    if (data.totalHits > 0 && data.data[0]) {
      const packageData = data.data[0];
      
      // 3. Cache successful response
      writeToCache(packageData);
      
      // 4. Return fresh data
      return {
        ...packageData,
        cached: false,
        lastUpdate: new Date().toISOString()
      };
    }
  } catch (error) {
    console.warn('NuGet API fetch failed:', error.message);
  }
  
  // 5. Fallback to cache
  if (cacheExists()) {
    const cached = readFromCache();
    return {
      ...cached,
      cached: true,
      lastUpdate: cached.cachedAt
    };
  }
  
  // 6. Ultimate fallback (hardcoded values)
  return {
    version: "1.4.0",
    downloads: 0,
    description: "A production-ready HttpClient wrapper",
    cached: true,
    error: true
  };
}
```

## API Alternatives

### Primary API (Recommended)

**NuGet Search API v3**:
```
GET https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility
```

**Pros**:
- Fast (global CDN)
- Returns download counts
- No authentication required

**Cons**:
- Search API (not always immediately updated)

### Alternative API

**NuGet Package Metadata API**:
```
GET https://api.nuget.org/v3/registration5-gz-semver2/webspark.httpclientutility/index.json
```

**Pros**:
- More authoritative (direct package metadata)
- Always up-to-date

**Cons**:
- Slower (more data returned)
- No total download count (only per-version downloads)
- Requires aggregation

**Recommendation**: Use Search API (primary) with metadata API as fallback if needed in future.

## Rate Limiting

**NuGet API Rate Limits**:
- No documented hard limits for reasonable use
- Recommended: 1 request per build (typical: 1-10 builds/day)
- Caching reduces load automatically

**Rate Limit Headers** (may be returned):
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1699564800
```

**Handling**:
```javascript
if (response.headers.get('x-ratelimit-remaining') < 10) {
  console.warn('Approaching rate limit, consider caching longer');
}
```

## Validation Rules

### Response Validation

```javascript
function validatePackageData(data) {
  // Must have data array with at least one result
  if (!data.data || data.data.length === 0) {
    throw new Error('No package found in API response');
  }
  
  const pkg = data.data[0];
  
  // Version must be valid SemVer
  if (!pkg.version || !/^\d+\.\d+\.\d+/.test(pkg.version)) {
    throw new Error(`Invalid version format: ${pkg.version}`);
  }
  
  // Downloads must be non-negative integer
  if (typeof pkg.totalDownloads !== 'number' || pkg.totalDownloads < 0) {
    throw new Error(`Invalid download count: ${pkg.totalDownloads}`);
  }
  
  // ID must match expected package name (case-insensitive)
  if (pkg.id.toLowerCase() !== 'webspark.httpclientutility') {
    throw new Error(`Unexpected package ID: ${pkg.id}`);
  }
  
  return true;
}
```

## Testing

### Manual API Test

```bash
# Test API endpoint
curl "https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility" | jq .

# Verify fields
curl -s "https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility" | jq '.data[0] | {version, totalDownloads, description}'
```

### Build-Time Test

```javascript
// In src/_data/nuget.js
module.exports = async function() {
  const data = await fetchNuGetData();
  
  // Log for debugging
  console.log('NuGet data fetched:', {
    version: data.version,
    downloads: data.downloads,
    cached: data.cached,
    error: data.error
  });
  
  return data;
};
```

## Security Considerations

**HTTPS Only**:
- API endpoint MUST use HTTPS
- Verify TLS certificate

**No Secrets Required**:
- Public API, no API keys needed
- No authentication headers

**Data Sanitization**:
```javascript
// Sanitize description for HTML output
function sanitizeDescription(desc) {
  return desc
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .substring(0, 500); // Limit length
}
```

## Related Contracts

- [eleventy-config.md](./eleventy-config.md) - How data is consumed in templates
- [data-model.md](../data-model.md) - Data structure documentation
- [package-json.md](./package-json.md) - Build dependencies
