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

    return true;
}

// Alert System
function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type}`;
    alertDiv.textContent = message;

    // Remove any existing alerts
    const existingAlerts = document.querySelectorAll('.alert');
    existingAlerts.forEach(alert => alert.remove());

    document.body.appendChild(alertDiv);

    setTimeout(() => {
        alertDiv.remove();
    }, 3000);
}

// Initialize all features
document.addEventListener('DOMContentLoaded', function () {
    // Form event listeners
    const loginForm = document.querySelector('form');
    if (loginForm && window.location.href.includes('login.html')) {
        loginForm.addEventListener('submit', function (e) {
            if (!validateLoginForm(e)) {
                e.preventDefault();
            }
        });
    }

    const registerForm = document.querySelector('form');
    if (registerForm && window.location.href.includes('Register')) {
        registerForm.addEventListener('submit', function (e) {
            if (!validateRegisterForm(e)) {
                e.preventDefault();
            }
        });
    }
});

// Real-time system monitoring and other existing functionality remains the same...


// Real-time system monitoring
function updateSystemMonitor() {
    fetch('/Home/GetSystemStatus')
        .then(response => response.json())
        .then(data => {
            document.querySelectorAll('.stat-value').forEach(element => {
                const key = element.getAttribute('data-key');
                if (data[key] !== undefined) {
                    element.textContent = data[key];
                    if (key === 'Status') {
                        element.className = `stat-value ${data[key] === 'SECURE' ? 'text-success' : 'text-danger'}`;
                    }
                }
            });
        })
        .catch(error => console.error('Error:', error));
}

// Update every 5 seconds
if (document.getElementById('systemMonitorWidget')) {
    setInterval(updateSystemMonitor, 5000);
}

// Add form validation
document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.querySelector('form');
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

            if (!emailRegex.test(email)) {
                showAlert('Please enter a valid email address', 'error');
                return;
            }

            if (password.length < 6) {
                showAlert('Password must be at least 6 characters', 'error');
                return;
            }

            this.submit();
        });
    }
});

// Alert system
function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type}`;
    alertDiv.textContent = message;
    document.body.appendChild(alertDiv);

    setTimeout(() => {
        alertDiv.remove();
    }, 3000);
}