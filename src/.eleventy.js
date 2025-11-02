export default function(eleventyConfig) {
  // Ignore cache file from watch to prevent infinite rebuild loop
  eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
  
  // Determine if production build
  const isProduction = process.env.ELEVENTY_ENV === "production";
  
  console.log(`🔧 Build mode: ${isProduction ? 'PRODUCTION' : 'DEVELOPMENT'}`);
  console.log(`🔧 PathPrefix will be: ${isProduction ? '/WebSpark.HttpClientUtility/' : '/'}`);
  
  // Set server options
  eleventyConfig.setServerOptions({
    showAllHosts: true
  });
  
  // Copy assets to output
  eleventyConfig.addPassthroughCopy("assets");
  eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": "favicon.ico" });
  eleventyConfig.addPassthroughCopy({ ".nojekyll": ".nojekyll" });
  eleventyConfig.addPassthroughCopy({ "robots.txt": "robots.txt" });
  
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
    htmlTemplateEngine: "njk",
    // GitHub Pages serves repo at /WebSpark.HttpClientUtility/ subdirectory
    pathPrefix: isProduction ? "/WebSpark.HttpClientUtility/" : "/"
  };
}
