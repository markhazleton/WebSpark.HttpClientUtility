import { defineConfig } from 'vite';
import path from 'path';
import fs from 'fs';

export default defineConfig({
  root: 'ClientApp',
  base: '/dist/',
  
  build: {
    outDir: path.resolve(__dirname, 'wwwroot/dist'),
    emptyOutDir: true,
    manifest: true,
    sourcemap: true,
    
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'ClientApp/src/main.js'),
        site: path.resolve(__dirname, 'ClientApp/src/site.css')
      },
      
      output: {
        entryFileNames: 'js/[name].[hash].js',
        chunkFileNames: 'js/[name].[hash].js',
        assetFileNames: (assetInfo) => {
          const info = assetInfo.name.split('.');
          const extType = info[info.length - 1];
          
          if (/css/i.test(extType)) {
            return 'css/[name].[hash][extname]';
          }
          if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(extType)) {
            return 'img/[name].[hash][extname]';
          }
          if (/woff|woff2|eot|ttf|otf/i.test(extType)) {
            return 'fonts/[name].[hash][extname]';
          }
          
          return 'assets/[name].[hash][extname]';
        }
      }
    },
    
    // Optimization settings
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true
      }
    },
    
    // Chunk splitting for better caching
    chunkSizeWarningLimit: 1000,
    
    // Target modern browsers (ES2020+)
    target: 'es2020'
  },
  
  server: {
    port: 5173,
    strictPort: false,
    hmr: {
      protocol: 'ws',
      host: 'localhost'
    }
  },
  
  plugins: [
    // Only use static copy plugin if public directory has files
    ...(fs.existsSync(path.resolve(__dirname, 'ClientApp/public')) && 
        fs.readdirSync(path.resolve(__dirname, 'ClientApp/public')).length > 1
      ? [viteStaticCopy({
          targets: [
            {
              src: path.resolve(__dirname, 'ClientApp/public/*'),
              dest: '.'
            }
          ]
        })]
      : [])
  ],
  
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'ClientApp/src'),
      '~bootstrap': path.resolve(__dirname, 'node_modules/bootstrap')
    }
  }
});
