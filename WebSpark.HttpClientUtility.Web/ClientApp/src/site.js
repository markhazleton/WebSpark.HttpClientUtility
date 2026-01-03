/**
 * Site-specific JavaScript for WebSpark.HttpClientUtility.Web
 * Custom application logic and utilities
 */

// Initialize tooltips if Bootstrap is available
document.addEventListener('DOMContentLoaded', () => {
  // Initialize Bootstrap tooltips
  const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
  if (tooltipTriggerList.length > 0 && typeof bootstrap !== 'undefined') {
    [...tooltipTriggerList].map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl));
  }

  // Initialize Bootstrap popovers
  const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
  if (popoverTriggerList.length > 0 && typeof bootstrap !== 'undefined') {
    [...popoverTriggerList].map((popoverTriggerEl) => new bootstrap.Popover(popoverTriggerEl));
  }

  // eslint-disable-next-line no-console
  console.log('WebSpark.HttpClientUtility.Web - Site JS initialized');
});

// Export for potential use in other modules
export function initializeBootstrapComponents() {
  // eslint-disable-next-line no-console
  console.log('Bootstrap components initialized');
}
