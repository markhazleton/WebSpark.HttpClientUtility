/**
 * Main entry point for WebSpark.HttpClientUtility.Web
 * Modern ES module imports with tree-shaking support
 */

// Bootstrap with Popper.js (full bundle)
import 'bootstrap';

// Import Bootstrap Icons CSS
import 'bootstrap-icons/font/bootstrap-icons.css';

// SignalR for real-time communication
import * as signalR from '@microsoft/signalr';

// Make SignalR globally available for inline scripts
window.signalR = signalR;

// Import custom site JavaScript
import './site.js';

// Import custom styles (will be processed by Vite)
import './site.css';

// eslint-disable-next-line no-console
console.log('WebSpark.HttpClientUtility.Web - Build pipeline active');
// eslint-disable-next-line no-console
console.log('SignalR loaded:', typeof window.signalR !== 'undefined');
