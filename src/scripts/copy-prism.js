import { readFileSync, writeFileSync, mkdirSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Ensure directories exist
const assetsDir = join(__dirname, '../assets');
const cssDir = join(assetsDir, 'css');
const jsDir = join(assetsDir, 'js');

mkdirSync(cssDir, { recursive: true });
mkdirSync(jsDir, { recursive: true });

// Copy Prism core
const prismCorePath = join(__dirname, '../node_modules/prismjs/prism.js');
const prismCoreContent = readFileSync(prismCorePath, 'utf8');
let jsContent = prismCoreContent;

// Add language support
const languages = ['csharp', 'javascript', 'json', 'powershell', 'bash', 'markup'];
for (const lang of languages) {
  try {
    const langPath = join(__dirname, `../node_modules/prismjs/components/prism-${lang}.js`);
    const langContent = readFileSync(langPath, 'utf8');
    jsContent += '\n' + langContent;
  } catch (err) {
    console.warn(`Language ${lang} not found, skipping`);
  }
}

// Write combined JS
writeFileSync(join(jsDir, 'prism.min.js'), jsContent);

// Copy CSS theme (Tomorrow Night)
const cssPath = join(__dirname, '../node_modules/prismjs/themes/prism-tomorrow.css');
const cssContent = readFileSync(cssPath, 'utf8');
writeFileSync(join(cssDir, 'prism-tomorrow.css'), cssContent);

console.log('✓ Prism.js files copied successfully');
console.log(`  - JavaScript: ${languages.length} languages included`);
console.log(`  - CSS: Tomorrow Night theme`);
