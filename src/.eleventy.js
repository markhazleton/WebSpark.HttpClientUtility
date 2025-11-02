export default function(eleventyConfig) {
  // Ignore cache file from watch to prevent infinite rebuild loop
  eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
  
  // Set server options
  eleventyConfig.setServerOptions({
    showAllHosts: true
  });
  
  // Copy assets to output
  eleventyConfig.addPassthroughCopy("assets");
  eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": "favicon.ico" });
  eleventyConfig.addPassthroughCopy({ ".nojekyll": ".nojekyll" });
  eleventyConfig.addPassthroughCopy({ "robots.txt": "robots.txt" });
  
  // Add relativePath filter for environment-independent asset paths
  eleventyConfig.addFilter("relativePath", function(path) {
    // Get the current page URL (e.g., "/features/" or "/")
    const pageUrl = this.page?.url || "/";
    
    // If at root level, no ../ needed
    if (pageUrl === "/" || pageUrl === "/index.html") {
      return path.startsWith("/") ? path.slice(1) : path;
    }
    
    // For pages in subdirectories, add ../ prefix
    return path.startsWith("/") ? ".." + path : "../" + path;
  });
  
  // Add filters
  eleventyConfig.addFilter("formatNumber", function(value) {
    if (!value) return "0";
    const num = parseInt(value);
    if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
    if (num >= 1000) return (num / 1000).toFixed(1) + "K";
    return num.toLocaleString();
  });
  
  eleventyConfig.addFilter("formatDate", function(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", { 
      year: "numeric", 
      month: "short", 
      day: "numeric" 
    });
  });
  
  eleventyConfig.addFilter("dateISO", function(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toISOString();
  });
  
  // Return configuration
  return {
    dir: {
      input: ".",
      output: "../docs",
      includes: "_includes",
      data: "_data"
    },
    templateFormats: ["md", "njk", "html"],
    markdownTemplateEngine: "njk",
    htmlTemplateEngine: "njk"
    // No pathPrefix needed - using relative paths for portability
  };
}
