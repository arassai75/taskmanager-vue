import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'

// Create Vue app
const app = createApp(App)

// Create and use Pinia store
const pinia = createPinia()
app.use(pinia)

// Global error handler
app.config.errorHandler = (err, instance, info) => {
  console.error('Global error:', err)
  console.error('Component instance:', instance)
  console.error('Error info:', info)
  
  // In production, you might want to send this to an error reporting service
  if (import.meta.env.PROD) {
    // Example: Send to error reporting service
    // errorReportingService.captureException(err, { extra: { info } })
  }
}

// Global warning handler (development only)
if (import.meta.env.DEV) {
  app.config.warnHandler = (msg, _instance, trace) => {
    console.warn('Vue warning:', msg)
    console.warn('Component trace:', trace)
  }
}

// Mount the app
app.mount('#app')

// Service worker registration (future enhancement)
if ('serviceWorker' in navigator && import.meta.env.PROD) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then((registration) => {
        console.log('SW registered: ', registration)
      })
      .catch((registrationError) => {
        console.log('SW registration failed: ', registrationError)
      })
  })
}

// Log app information
console.log('🚀 TaskManager Vue App started')
console.log('📝 Environment:', import.meta.env.MODE)
console.log('🔗 API Base URL:', import.meta.env.VITE_API_BASE_URL || '/api')

// Performance monitoring (development only)
if (import.meta.env.DEV) {
  // Log component performance
  app.config.performance = true
  
  // Monitor long tasks
  if ('PerformanceObserver' in window) {
    const observer = new PerformanceObserver((list) => {
      list.getEntries().forEach((entry) => {
        if (entry.duration > 50) {
          console.warn(`Long task detected: ${entry.duration}ms`)
        }
      })
    })
    
    observer.observe({ entryTypes: ['longtask'] })
  }
}
