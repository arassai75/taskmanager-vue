<template>
  <Teleport to="body">
    <div class="fixed top-4 right-4 z-50 space-y-2 max-w-sm">
      <TransitionGroup
        name="notification"
        tag="div"
        class="space-y-2"
      >
        <div
          v-for="notification in notifications"
          :key="notification.id"
          class="bg-white rounded-lg shadow-lg border border-gray-200 p-4 min-w-80"
          :class="notificationClasses(notification.type)"
        >
          <!-- Header -->
          <div class="flex items-start justify-between gap-3">
            <div class="flex items-start gap-3 flex-1 min-w-0">
              <!-- Icon -->
              <div 
                class="flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center text-white text-sm"
                :class="iconClasses(notification.type)"
              >
                <i :class="iconName(notification.type)"></i>
              </div>

              <!-- Content -->
              <div class="flex-1 min-w-0">
                <h4 class="text-sm font-medium text-gray-900 truncate">
                  {{ notification.title }}
                </h4>
                <p class="mt-1 text-sm text-gray-600 break-words">
                  {{ notification.message }}
                </p>
              </div>
            </div>

            <!-- Close Button -->
            <button
              @click="removeNotification(notification.id)"
              class="flex-shrink-0 text-gray-400 hover:text-gray-600 transition-colors duration-200"
              aria-label="Dismiss notification"
            >
              <i class="fas fa-times text-sm"></i>
            </button>
          </div>

          <!-- Progress Bar (for timed notifications) -->
          <div
            v-if="notification.duration && notification.duration > 0"
            class="mt-3 w-full bg-gray-200 rounded-full h-1"
          >
            <div
              class="h-1 rounded-full transition-all duration-100 ease-linear"
              :class="progressBarClasses(notification.type)"
              :style="{ width: getProgressWidth(notification) + '%' }"
            ></div>
          </div>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useTaskStore } from '@/stores/taskStore'
import type { NotificationMessage } from '@/types'

// Store
const taskStore = useTaskStore()
const { notifications, removeNotification } = taskStore

// Track notification start times for progress bars
const notificationTimers = ref<Map<string, { startTime: number; interval: number }>>(new Map())

// Computed classes for different notification types
function notificationClasses(type: NotificationMessage['type']): string {
  const baseClasses = 'border-l-4'
  
  switch (type) {
    case 'success':
      return `${baseClasses} border-green-500 bg-green-50`
    case 'error':
      return `${baseClasses} border-red-500 bg-red-50`
    case 'warning':
      return `${baseClasses} border-yellow-500 bg-yellow-50`
    case 'info':
      return `${baseClasses} border-blue-500 bg-blue-50`
    default:
      return `${baseClasses} border-gray-500 bg-gray-50`
  }
}

function iconClasses(type: NotificationMessage['type']): string {
  switch (type) {
    case 'success':
      return 'bg-green-500'
    case 'error':
      return 'bg-red-500'
    case 'warning':
      return 'bg-yellow-500'
    case 'info':
      return 'bg-blue-500'
    default:
      return 'bg-gray-500'
  }
}

function iconName(type: NotificationMessage['type']): string {
  switch (type) {
    case 'success':
      return 'fas fa-check'
    case 'error':
      return 'fas fa-exclamation-triangle'
    case 'warning':
      return 'fas fa-exclamation'
    case 'info':
      return 'fas fa-info'
    default:
      return 'fas fa-bell'
  }
}

function progressBarClasses(type: NotificationMessage['type']): string {
  switch (type) {
    case 'success':
      return 'bg-green-500'
    case 'error':
      return 'bg-red-500'
    case 'warning':
      return 'bg-yellow-500'
    case 'info':
      return 'bg-blue-500'
    default:
      return 'bg-gray-500'
  }
}

function getProgressWidth(notification: NotificationMessage): number {
  if (!notification.duration || notification.duration <= 0) {
    return 100
  }

  const timer = notificationTimers.value.get(notification.id)
  if (!timer) {
    return 100
  }

  const elapsed = Date.now() - timer.startTime
  const progress = Math.max(0, 100 - (elapsed / notification.duration) * 100)
  
  return progress
}

// Watch for new notifications and set up timers
function setupNotificationTimer(notification: NotificationMessage) {
  if (!notification.duration || notification.duration <= 0) {
    return
  }

  const startTime = Date.now()
  const interval = setInterval(() => {
    const elapsed = Date.now() - startTime
    
    if (elapsed >= notification.duration!) {
      clearInterval(interval)
      notificationTimers.value.delete(notification.id)
      removeNotification(notification.id)
    }
  }, 100) // Update every 100ms for smooth progress bar

  notificationTimers.value.set(notification.id, { startTime, interval })
}

// Clean up timers when notifications are removed
function cleanupTimer(notificationId: string) {
  const timer = notificationTimers.value.get(notificationId)
  if (timer) {
    clearInterval(timer.interval)
    notificationTimers.value.delete(notificationId)
  }
}

// Watch for notification changes
let previousNotificationIds = new Set<string>()

function watchNotifications() {
  const currentNotificationIds = new Set(notifications.map((n: any) => n.id))
  
  // Set up timers for new notifications
  for (const notification of notifications) {
    if (!previousNotificationIds.has(notification.id)) {
      setupNotificationTimer(notification)
    }
  }
  
  // Clean up timers for removed notifications
  for (const id of previousNotificationIds) {
    if (!currentNotificationIds.has(id)) {
      cleanupTimer(id)
    }
  }
  
  previousNotificationIds = currentNotificationIds
}

// Set up reactive watching
onMounted(() => {
  // Use requestAnimationFrame for smooth updates
  let animationFrameId: number
  
  function updateNotifications() {
    watchNotifications()
    animationFrameId = requestAnimationFrame(updateNotifications)
  }
  
  updateNotifications()
  
  onUnmounted(() => {
    if (animationFrameId) {
      cancelAnimationFrame(animationFrameId)
    }
    
    // Clean up all timers
    for (const [id] of notificationTimers.value) {
      cleanupTimer(id)
    }
  })
})
</script>

<style scoped>
/* Notification animations */
.notification-enter-active,
.notification-leave-active {
  transition: all 0.3s ease;
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(100%) scale(0.9);
}

.notification-leave-to {
  opacity: 0;
  transform: translateX(100%) scale(0.9);
}

.notification-move {
  transition: transform 0.3s ease;
}

/* Custom scrollbar for overflow */
.notification-content {
  max-height: 200px;
  overflow-y: auto;
}

.notification-content::-webkit-scrollbar {
  width: 4px;
}

.notification-content::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 2px;
}

.notification-content::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 2px;
}

.notification-content::-webkit-scrollbar-thumb:hover {
  background: #a1a1a1;
}
</style>
