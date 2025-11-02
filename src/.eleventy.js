export default function(eleventyConfig) {
  // Ignore cache file from watch to prevent infinite rebuild loop
  eleventyConfig.watchIgnores.add("./_data/nuget-cache.json");
  
  // Determine if production build
  const isProduction = process.env.ELEVENTY_ENV === "production";
  const prefix = isProduction ? "WebSpark.HttpClientUtility" : "";
  
  console.log(`🔧 Build mode: ${isProduction ? 'PRODUCTION' : 'DEVELOPMENT'}`);
  console.log(`🔧 PathPrefix will be: ${isProduction ? '/WebSpark.HttpClientUtility/' : '/'}`);
  
  // Set server options for dev mode
  if (!isProduction) {
    eleventyConfig.setServerOptions({
      showAllHosts: true
    });
  }
  
  // Copy assets through to output - use prefix for production, root for dev
  if (prefix) {
    eleventyConfig.addPassthroughCopy({ "assets": `${prefix}/assets` });
    eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": `${prefix}/favicon.ico` });
  } else {
    eleventyConfig.addPassthroughCopy("assets");
    eleventyConfig.addPassthroughCopy({ "assets/images/favicon.ico": "favicon.ico" });
  }
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
    // Use pathPrefix only for production (GitHub Pages needs it)
    pathPrefix: isProduction ? "/WebSpark.HttpClientUtility/" : "/"
  };
}
