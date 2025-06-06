// Dark Mode Controller for ASP.NET Core
class DarkModeController {
    constructor() {
        this.init();
    }

    init() {
        // Lấy theme từ localStorage hoặc mặc định là light
        this.currentTheme = localStorage.getItem('theme') || 'light';

        // Detect system preference nếu chưa có setting
        if (!localStorage.getItem('theme')) {
            this.currentTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }

        // Apply theme
        this.applyTheme(this.currentTheme);

        // Create toggle button
        this.createToggleButton();

        // Listen for system theme changes
        this.listenForSystemChanges();

        // Update button state
        this.updateToggleButton();
    }

    createToggleButton() {
        // Kiểm tra xem button đã tồn tại chưa
        if (document.querySelector('.theme-toggle')) return;

        const toggleButton = document.createElement('button');
        toggleButton.className = 'theme-toggle';
        toggleButton.innerHTML = `
            <span class="theme-icon">🌓</span>
            <span class="theme-text">Dark Mode</span>
        `;

        toggleButton.addEventListener('click', () => this.toggleTheme());

        // Thêm vào body
        document.body.appendChild(toggleButton);
    }

    updateToggleButton() {
        const button = document.querySelector('.theme-toggle');
        if (!button) return;

        const icon = button.querySelector('.theme-icon');
        const text = button.querySelector('.theme-text');

        if (this.currentTheme === 'dark') {
            icon.textContent = '☀️';
            text.textContent = 'Light Mode';
            button.setAttribute('aria-label', 'Switch to light mode');
        } else {
            icon.textContent = '🌙';
            text.textContent = 'Dark Mode';
            button.setAttribute('aria-label', 'Switch to dark mode');
        }
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);

        // Update meta theme-color for mobile browsers
        let metaThemeColor = document.querySelector('meta[name=theme-color]');
        if (!metaThemeColor) {
            metaThemeColor = document.createElement('meta');
            metaThemeColor.name = 'theme-color';
            document.head.appendChild(metaThemeColor);
        }

        metaThemeColor.content = theme === 'dark' ? '#121212' : '#ffffff';

        // Dispatch custom event for other components
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: theme }
        }));
    }

    toggleTheme() {
        this.currentTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(this.currentTheme);
        this.updateToggleButton();

        // Save to localStorage
        localStorage.setItem('theme', this.currentTheme);

        // Animate the transition
        this.animateThemeTransition();
    }

    animateThemeTransition() {
        // Add smooth transition class
        document.body.style.transition = 'background-color 0.3s ease, color 0.3s ease';

        // Remove transition after animation
        setTimeout(() => {
            document.body.style.transition = '';
        }, 300);
    }

    listenForSystemChanges() {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

        mediaQuery.addEventListener('change', (e) => {
            // Only auto-switch if user hasn't manually set a preference
            if (!localStorage.getItem('theme-user-preference')) {
                this.currentTheme = e.matches ? 'dark' : 'light';
                this.applyTheme(this.currentTheme);
                this.updateToggleButton();
            }
        });
    }

    // Public methods for external use
    setTheme(theme) {
        if (theme === 'light' || theme === 'dark') {
            this.currentTheme = theme;
            this.applyTheme(theme);
            this.updateToggleButton();
            localStorage.setItem('theme', theme);
            localStorage.setItem('theme-user-preference', 'true');
        }
    }

    getTheme() {
        return this.currentTheme;
    }

    // Reset to system preference
    resetToSystem() {
        localStorage.removeItem('theme');
        localStorage.removeItem('theme-user-preference');
        this.currentTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        this.applyTheme(this.currentTheme);
        this.updateToggleButton();
    }
}

// Utility functions for ASP.NET Core integration
const DarkModeUtils = {
    // Initialize dark mode when DOM is ready
    init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                window.darkModeController = new DarkModeController();
            });
        } else {
            window.darkModeController = new DarkModeController();
        }
    },

    // For Razor Pages/Views - call this in your layout
    initForAspNet() {
        // Prevent flash of wrong theme
        const savedTheme = localStorage.getItem('theme') ||
            (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
        document.documentElement.setAttribute('data-theme', savedTheme);

        // Initialize controller
        this.init();
    },

    // Handle form submissions in dark mode
    preserveThemeOnFormSubmit() {
        document.addEventListener('submit', () => {
            // Ensure theme persists after form submissions/redirects
            sessionStorage.setItem('theme-temp', window.darkModeController?.getTheme() || 'light');
        });

        // Restore theme after page load
        const tempTheme = sessionStorage.getItem('theme-temp');
        if (tempTheme) {
            localStorage.setItem('theme', tempTheme);
            sessionStorage.removeItem('theme-temp');
        }
    },

    // For AJAX requests - preserve theme in partial views
    updatePartialViews() {
        document.addEventListener('DOMContentLoaded', () => {
            // Re-apply theme to dynamically loaded content
            const observer = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                        // Theme is automatically applied via CSS variables
                        // But you can add custom logic here for specific components
                    }
                });
            });

            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        });
    }
};

// Auto-initialize
DarkModeUtils.initForAspNet();

// Export for use in other modules (if using ES6 modules)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { DarkModeController, DarkModeUtils };
}