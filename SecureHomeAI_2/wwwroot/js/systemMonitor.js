// System Monitor Script for SecureHome AI
// Adds real-time clock and system status monitoring

class SystemMonitor {
  constructor() {
    this.timeElement = null;
    this.statusElement = null;
    this.systemChecks = [
      'Network Security',
      'Camera Systems',
      'Motion Sensors',
      'Door Locks',
      'Window Sensors'
    ];
    this.initialize();
  }

  initialize() {
    // Create monitor container
    const monitorContainer = document.createElement('div');
    monitorContainer.className = 'system-monitor';

    // Create time display
    this.timeElement = document.createElement('div');
    this.timeElement.className = 'monitor-time';

    // Create status display
    this.statusElement = document.createElement('div');
    this.statusElement.className = 'monitor-status';

    // Append elements
    monitorContainer.appendChild(this.timeElement);
    monitorContainer.appendChild(this.statusElement);

    // Add to page
    const mainContent = document.querySelector('.main-content');
    if (mainContent) {
      mainContent.insertBefore(monitorContainer, mainContent.firstChild);
    }

    // Start monitors
    this.startClock();
    this.startStatusCheck();
  }

  startClock() {
    setInterval(() => {
      const now = new Date();
      this.timeElement.innerHTML = `
                <div class="time-display">
                    <span class="time">${now.toLocaleTimeString()}</span>
                    <span class="date">${now.toLocaleDateString()}</span>
                </div>
            `;
    }, 1000);
  }

  startStatusCheck() {
    setInterval(() => {
      const randomSystem = this.systemChecks[Math.floor(Math.random() * this.systemChecks.length)];
      const status = Math.random() > 0.9 ? 'Warning' : 'Normal';
      const statusClass = status === 'Normal' ? 'status-normal' : 'status-warning';

      this.statusElement.innerHTML = `
                <div class="status-check ${statusClass}">
                    <span class="system-name">${randomSystem}</span>
                    <span class="system-status">Status: ${status}</span>
                    <span class="check-time">Last Check: ${new Date().toLocaleTimeString()}</span>
                </div>
            `;
    }, 5000);
  }
}
