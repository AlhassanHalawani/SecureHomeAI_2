// Main JavaScript file for SecureHome AI
// Author: [Your Name]
// Date: October 29, 2024

// Form Validation
function validateLoginForm(event) {
  event.preventDefault();

  const email = document.getElementById('email').value;
  const password = document.getElementById('password').value;
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailRegex.test(email)) {
    showAlert('Please enter a valid email address', 'error');
    return false;
  }

  if (password.length < 8) {
    showAlert('Password must be at least 8 characters long', 'error');
    return false;
  }

  showAlert('Login successful!', 'success');
  return true;
}

// Registration Form Validation
function validateRegisterForm(event) {
  event.preventDefault();

  const fullname = document.getElementById('fullname').value;
  const email = document.getElementById('email').value;
  const password = document.getElementById('password').value;
  const confirmPassword = document.getElementById('confirm-password').value;
  const address = document.getElementById('address').value;

  if (fullname.length < 3) {
    showAlert('Name must be at least 3 characters long', 'error');
    return false;
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) {
    showAlert('Please enter a valid email address', 'error');
    return false;
  }

  if (password.length < 8) {
    showAlert('Password must be at least 8 characters long', 'error');
    return false;
  }

  if (password !== confirmPassword) {
    showAlert('Passwords do not match', 'error');
    return false;
  }

  if (address.length < 5) {
    showAlert('Please enter a valid address', 'error');
    return false;
  }

  showAlert('Registration successful!', 'success');
  return true;
}

// Dynamic Security Status Updates
function updateSecurityStatus() {
  const statuses = [
    { status: 'SECURE', class: 'status-safe', message: 'All systems operational' },
    { status: 'ALERT', class: 'status-danger', message: 'Unusual activity detected' },
    { status: 'WARNING', class: 'status-warning', message: 'System check required' }
  ];

  const statusElement = document.querySelector('.card .status-safe');
  if (statusElement) {
    const randomStatus = statuses[Math.floor(Math.random() * statuses.length)];
    statusElement.textContent = randomStatus.status;
    statusElement.className = randomStatus.class;
    document.querySelector('.card p:last-child').textContent = 'Last checked: ' + new Date().toLocaleTimeString();
  }
}

// Dynamic Network Activity
function updateNetworkActivity() {
  const connectedDevices = Math.floor(Math.random() * 20) + 5;
  const suspiciousActivities = Math.floor(Math.random() * 3);

  const networkCard = document.querySelector('.card:nth-child(2)');
  if (networkCard) {
    networkCard.innerHTML = `
            <h2>Network Activity</h2>
            <p>Connected Devices: ${connectedDevices}</p>
            <p>Suspicious Activities: ${suspiciousActivities}</p>
        `;
  }
}

// Alert System
function showAlert(message, type) {
  const alertDiv = document.createElement('div');
  alertDiv.className = `alert alert-${type}`;
  alertDiv.textContent = message;

  document.body.appendChild(alertDiv);

  setTimeout(() => {
    alertDiv.remove();
  }, 3000);
}

// Interactive Image Gallery for About Page
function initializeImageGallery() {
  const securityImages = document.querySelectorAll('.analytics-image');
  securityImages.forEach(image => {
    image.addEventListener('click', function() {
      this.classList.toggle('expanded');
    });
  });
}

// Initialize all features
document.addEventListener('DOMContentLoaded', function() {
  // Form event listeners
  const loginForm = document.querySelector('form');
  if (loginForm && window.location.href.includes('login.html')) {
    loginForm.addEventListener('submit', validateLoginForm);
  }

  const registerForm = document.querySelector('form');
  if (registerForm && window.location.href.includes('register.html')) {
    registerForm.addEventListener('submit', validateRegisterForm);
  }

  // Initialize image gallery on about page
  if (window.location.href.includes('about.html')) {
    initializeImageGallery();
  }

  // Update security status periodically on home page
  if (window.location.href.includes('index.html') || window.location.href.endsWith('/')) {
    setInterval(updateSecurityStatus, 5000);
    setInterval(updateNetworkActivity, 7000);
  }
});
