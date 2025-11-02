import fetch from 'node-fetch';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const CACHE_FILE = path.join(__dirname, 'nuget-cache.json');
const API_URL = 'https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility';

function formatNumber(value) {
  if (!value) return "0";
  const num = parseInt(value);
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toLocaleString();
}

export default async function() {
  try {
    console.log('Fetching NuGet package data...');
    const response = await fetch(API_URL);
    const data = await response.json();
    
    if (data.data && data.data.length > 0) {
      const packageData = data.data[0];
      
      // Cache successful response
      const cacheData = {
        ...packageData,
        cachedAt: new Date().toISOString()
      };
      fs.writeFileSync(CACHE_FILE, JSON.stringify(cacheData, null, 2));
      console.log('✓ NuGet data fetched successfully');
      
      return {
        version: packageData.version,
        downloads: packageData.totalDownloads,
        displayDownloads: formatNumber(packageData.totalDownloads),
        description: packageData.description,
        projectUrl: packageData.projectUrl,
        lastUpdate: new Date().toISOString(),
        cached: false
      };
    }
  } catch (error) {
    console.warn('⚠ NuGet API failed, using cached data:', error.message);
    
    if (fs.existsSync(CACHE_FILE)) {
      const cached = JSON.parse(fs.readFileSync(CACHE_FILE, 'utf8'));
      return {
        version: cached.version,
        downloads: cached.totalDownloads,
        displayDownloads: formatNumber(cached.totalDownloads),
        description: cached.description,
        projectUrl: cached.projectUrl,
        lastUpdate: cached.cachedAt || 'Unknown',
        cached: true,
        cacheTimestamp: new Date(cached.cachedAt).toLocaleDateString("en-US", { 
          year: "numeric", 
          month: "short", 
          day: "numeric" 
        })
      };
    }
    
    // Ultimate fallback
    return {
      version: "1.4.0",
      downloads: 0,
      displayDownloads: "0",
      description: "A production-ready HttpClient wrapper for .NET with resilience, caching, and telemetry.",
      cached: true,
      error: true
    };
  }
}
