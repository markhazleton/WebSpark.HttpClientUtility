/**
 * Site-specific JavaScript for WebSpark.HttpClientUtility.Web
 * Custom application logic and utilities
 */

/* eslint-disable no-console */

// Log that site.js is loading (happens immediately on import)
console.log('[Site.js] Loading - timestamp:', new Date().toISOString());

// Log Bootstrap availability immediately
console.log('[Site.js] Bootstrap available:', typeof bootstrap !== 'undefined');
console.log('[Site.js] SignalR available:', typeof window.signalR !== 'undefined');

/* eslint-enable no-console */

// Initialize tooltips if Bootstrap is available
document.addEventListener('DOMContentLoaded', () => {
  /* eslint-disable no-console */
  console.log('[Site.js] DOMContentLoaded event fired');

  // Verify Bootstrap is loaded
  if (typeof bootstrap === 'undefined') {
    console.error('[Site.js] CRITICAL: Bootstrap is not loaded! Check that main.js is loading correctly.');
    return;
  }

  // Initialize Bootstrap tooltips
  const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
  if (tooltipTriggerList.length > 0) {
    [...tooltipTriggerList].map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl));
  }

  // Initialize Bootstrap popovers
  const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
  if (popoverTriggerList.length > 0) {
    [...popoverTriggerList].map((popoverTriggerEl) => new bootstrap.Popover(popoverTriggerEl));
  }

  console.log('[Site.js] Site JS initialized successfully ✓');
  /* eslint-enable no-console */
});

// Export for potential use in other modules
export function initializeBootstrapComponents() {
  // eslint-disable-next-line no-console
  console.log('Bootstrap components initialized');
}
