type ToastType = 'success' | 'error' | 'info' | 'warning';

interface ToastOptions {
  duration?: number;
  position?:
    | 'top-right'
    | 'top-center'
    | 'top-left'
    | 'bottom-right'
    | 'bottom-center'
    | 'bottom-left';
}

/**
 * Simple toast notification utility
 * Creates temporary notification messages in the UI
 */
class ToastManager {
  private container: HTMLDivElement | null = null;

  private ensureContainer() {
    if (!this.container) {
      this.container = document.createElement('div');
      this.container.id = 'toast-container';
      this.container.className = 'fixed top-4 right-4 z-50 flex flex-col gap-2';
      document.body.appendChild(this.container);
    }
    return this.container;
  }

  private show(message: string, type: ToastType, options: ToastOptions = {}) {
    const container = this.ensureContainer();
    const duration = options.duration || 3000;

    // Create toast element
    const toast = document.createElement('div');
    toast.className = `
      flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg text-white
      transform transition-all duration-300 ease-in-out
      max-w-sm animate-slideIn
      ${type === 'success' ? 'bg-green-600' : ''}
      ${type === 'error' ? 'bg-red-600' : ''}
      ${type === 'info' ? 'bg-blue-600' : ''}
      ${type === 'warning' ? 'bg-yellow-600' : ''}
    `.trim();

    // Icon based on type
    const icons = {
      success: '',
      error: '',
      info: 'ℹ',
      warning: '',
    };

    toast.innerHTML = `
      <div class="flex-shrink-0 w-5 h-5 flex items-center justify-center font-bold text-lg">
        ${icons[type]}
      </div>
      <div class="flex-1 text-sm font-medium">${message}</div>
      <button class="flex-shrink-0 text-white hover:text-gray-200 transition-colors" onclick="this.parentElement.remove()">
        <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path>
        </svg>
      </button>
    `;

    container.appendChild(toast);

    // Auto remove after duration
    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateX(100%)';
      setTimeout(() => toast.remove(), 300);
    }, duration);
  }

  success(message: string, options?: ToastOptions) {
    this.show(message, 'success', options);
  }

  error(message: string, options?: ToastOptions) {
    this.show(message, 'error', options);
  }

  info(message: string, options?: ToastOptions) {
    this.show(message, 'info', options);
  }

  warning(message: string, options?: ToastOptions) {
    this.show(message, 'warning', options);
  }
}

// Export singleton instance
export const toast = new ToastManager();
