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
    console.warn('[Site.js] Bootstrap is not loaded; skipping tooltip/popover initialization.');
  } else {
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
  }

  console.log('[Site.js] Site JS initialized successfully ✓');

  initializeBatchExecutionDemo();
  /* eslint-enable no-console */
});

function initializeBatchExecutionDemo() {
  const startButton = document.getElementById('startRunButton');
  const input = document.getElementById('batchInput');
  const statusPanel = document.getElementById('statusPanel');
  const resultPanel = document.getElementById('resultPanel');

  if (!startButton || !input || !statusPanel || !resultPanel) {
    return;
  }

  if (startButton.dataset.batchBound === 'true') {
    return;
  }

  startButton.dataset.batchBound = 'true';

  let activeRunId = null;
  let pollTimer = null;

  const setText = (id, text) => {
    const element = document.getElementById(id);
    if (element) {
      element.textContent = text;
    }
  };

  async function pollRun() {
    if (!activeRunId) {
      return;
    }

    const response = await fetch(`/BatchExecution/runs/${encodeURIComponent(activeRunId)}`);
    if (!response.ok) {
      return;
    }

    const data = await response.json();
    setText('runId', data.runId);
    setText('runStatus', data.status);
    setText('runProgress', `${data.completedCount}/${data.totalPlannedCount}`);
    setText('successCount', `${data.statistics?.successCount ?? 0}`);
    setText('failureCount', `${data.statistics?.failureCount ?? 0}`);
    setText('p95', `${data.statistics?.p95Milliseconds ?? 0}`);

    if (data.status === 'Completed' || data.status === 'Cancelled' || data.status === 'Failed') {
      clearInterval(pollTimer);
      pollTimer = null;
      resultPanel.classList.remove('d-none');

      const resultJson = document.getElementById('resultJson');
      if (resultJson) {
        resultJson.textContent = JSON.stringify(data, null, 2);
      }
    }
  }

  startButton.addEventListener('click', async () => {
    const originalText = startButton.textContent;
    startButton.disabled = true;
    startButton.textContent = 'Starting...';

    let payload;
    try {
      payload = JSON.parse(input.value);
    } catch {
      statusPanel.classList.remove('d-none');
      setText('runStatus', 'Invalid JSON configuration');
      startButton.disabled = false;
      startButton.textContent = originalText;
      return;
    }

    try {
      const response = await fetch('/BatchExecution/runs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      const data = await response.json();
      if (!response.ok) {
        statusPanel.classList.remove('d-none');
        setText('runStatus', data.error || 'Unable to start run.');
        return;
      }

      activeRunId = data.runId;
      statusPanel.classList.remove('d-none');
      resultPanel.classList.add('d-none');
      setText('runId', activeRunId);
      setText('runStatus', data.status);
      setText('runProgress', `0/${data.totalPlannedCount}`);

      if (pollTimer) {
        clearInterval(pollTimer);
      }

      await pollRun();
      pollTimer = setInterval(pollRun, 1000);
    } catch (error) {
      statusPanel.classList.remove('d-none');
      setText('runStatus', `Unable to start run: ${error?.message ?? 'Unknown error'}`);
    } finally {
      startButton.disabled = false;
      startButton.textContent = originalText;
    }
  });
}

// Export for potential use in other modules
export function initializeBootstrapComponents() {
  // eslint-disable-next-line no-console
  console.log('Bootstrap components initialized');
}
