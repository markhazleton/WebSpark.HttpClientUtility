import fetch from 'node-fetch';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const CACHE_FILE = path.join(__dirname, 'nuget-cache.json');
const API_URL = 'https://azuresearch-usnc.nuget.org/query?q=packageid:WebSpark.HttpClientUtility';
const VERSION_FILE = path.join(__dirname, '..', '..', 'Directory.Build.props');
const DESCRIPTION_FILE = path.join(__dirname, '..', '..', 'WebSpark.HttpClientUtility', 'WebSpark.HttpClientUtility.csproj');

function formatNumber(value) {
  if (!value) return "0";
  const num = parseInt(value);
  if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  return num.toLocaleString();
}

function parseVersion(version) {
  return String(version || '0.0.0')
    .split('.')
    .slice(0, 3)
    .map((v) => Number.parseInt(v, 10) || 0);
}

function isVersionGreater(left, right) {
  const a = parseVersion(left);
  const b = parseVersion(right);
  for (let i = 0; i < 3; i++) {
    if (a[i] > b[i]) return true;
    if (a[i] < b[i]) return false;
  }
  return false;
}

function readLocalVersion() {
  try {
    const file = fs.readFileSync(VERSION_FILE, 'utf8');
    const match = file.match(/<Version>([^<]+)<\/Version>/i);
    return match?.[1]?.trim() || null;
  } catch {
    return null;
  }
}

function readLocalDescription() {
  try {
    const file = fs.readFileSync(DESCRIPTION_FILE, 'utf8');
    const match = file.match(/<Description>([\s\S]*?)<\/Description>/i);
    return match?.[1]?.replace(/\s+/g, ' ').trim() || null;
  } catch {
    return null;
  }
}

export default async function() {
  const localVersion = readLocalVersion();
  const localDescription = readLocalDescription();
  const cachedFileData = fs.existsSync(CACHE_FILE)
    ? JSON.parse(fs.readFileSync(CACHE_FILE, 'utf8'))
    : null;

  try {
    console.log('Fetching NuGet package data...');
    const response = await fetch(API_URL);
    const data = await response.json();
    
    if (data.data && data.data.length > 0) {
      const packageData = data.data[0];
      const useLocalVersion = localVersion && isVersionGreater(localVersion, packageData.version);
      const effectiveVersion = useLocalVersion ? localVersion : packageData.version;
      const effectiveDescription = useLocalVersion
        ? (localDescription || cachedFileData?.description || packageData.description)
        : packageData.description;
      
      // Cache successful response
      const cacheData = {
        ...packageData,
        version: effectiveVersion,
        description: effectiveDescription,
        cachedAt: new Date().toISOString()
      };
      fs.writeFileSync(CACHE_FILE, JSON.stringify(cacheData, null, 2));
      console.log('✓ NuGet data fetched successfully');
      
      return {
        version: effectiveVersion,
        downloads: packageData.totalDownloads,
        displayDownloads: formatNumber(packageData.totalDownloads),
        description: effectiveDescription,
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
      version: localVersion || "1.4.0",
      downloads: 0,
      displayDownloads: "0",
      description: "A production-ready HttpClient wrapper for .NET with resilience, caching, and telemetry.",
      cached: true,
      error: true
    };
  }
}
