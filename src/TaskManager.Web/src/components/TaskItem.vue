<template>
  <div 
    class="group bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 hover:shadow-md transition-all duration-200 p-3 relative overflow-hidden"
    :class="{ 
      'opacity-90 bg-gray-50 dark:bg-gray-700': task.isCompleted,
      'border-red-200 shadow-red-50': task.isOverdue && !task.isCompleted,
      'border-orange-200 shadow-orange-50': task.isDueSoon && !task.isCompleted,
      'border-green-200 shadow-green-50': task.isCompleted
    }"
  >
    <!-- Priority Indicator Bar -->
    <div 
      class="absolute top-0 left-0 w-1 h-full rounded-r-lg"
      :class="priorityConfig.barColor"
    ></div>

    <!-- Removed overlapping due status badge - now shown inline with other info -->

    <div class="flex items-start gap-3">
      <!-- Enhanced Checkbox -->
      <div class="flex-shrink-0 relative mt-0.5">
        <button
          @click="$emit('toggle', task.id)"
          class="relative w-5 h-5 rounded border-2 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-primary-500/30 hover:scale-105"
          :class="checkboxClasses"
          :aria-label="task.isCompleted ? 'Mark as incomplete' : 'Mark as complete'"
        >
          <!-- Checkmark Icon -->
          <div 
            v-if="task.isCompleted"
            class="absolute inset-0 flex items-center justify-center"
          >
            <svg
              class="w-3 h-3 text-white"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="3"
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>
          
          <!-- Hover Effect -->
          <div 
            v-if="!task.isCompleted"
            class="absolute inset-0 rounded bg-green-500 opacity-0 group-hover:opacity-20 transition-opacity duration-200"
          ></div>
        </button>
      </div>

      <!-- Task Content -->
      <div class="flex-1 min-w-0">
        <!-- Top Row: Category + Title + Status + Priority -->
        <div class="flex items-center gap-2 mb-1">
          <!-- Category Badge -->
          <span 
            v-if="task.categoryName"
            class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-xs font-medium text-white flex-shrink-0"
            :style="{ backgroundColor: task.categoryColor || '#6B7280' }"
            :title="task.categoryName"
          >
            <div class="w-1.5 h-1.5 rounded-sm bg-white/40"></div>
            {{ task.categoryName }}
          </span>

          <!-- Title -->
          <h3 
            class="font-medium text-sm text-gray-900 dark:text-white leading-tight flex-1 min-w-0 truncate"
            :class="{ 
              'line-through text-gray-500 dark:text-gray-400': task.isCompleted,
              'text-red-700 dark:text-red-400': task.isOverdue && !task.isCompleted,
              'text-orange-700 dark:text-orange-400': task.isDueSoon && !task.isCompleted
            }"
          >
            {{ task.title }}
          </h3>

          <!-- Complete Badge -->
          <span 
            v-if="task.isCompleted"
            class="inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-200 flex-shrink-0"
          >
            <i class="fas fa-check mr-1 text-xs"></i>
            Done
          </span>

          <!-- Priority Badge -->
          <span 
            class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-xs font-medium flex-shrink-0"
            :class="priorityConfig.badgeColor"
          >
            <!-- Custom Priority SVG Icon -->
            <div class="w-3 h-3 flex items-center justify-center">
              <PriorityIcon :priority="task.priority" />
            </div>
            <span class="hidden sm:inline">{{ task.priorityText }}</span>
          </span>

          <!-- Due Date Badge (if present and urgent) -->
          <span 
            v-if="task.dueDate && (task.isOverdue || task.isDueSoon)"
            class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-xs font-medium flex-shrink-0"
            :class="dueStatusConfig.badgeColor"
          >
            <i :class="dueStatusConfig.icon" class="text-xs"></i>
            <span>{{ formatDueDateShort(new Date(task.dueDate)) }}</span>
          </span>
        </div>

        <!-- Description -->
        <p 
          v-if="task.description" 
          class="text-xs text-gray-600 dark:text-gray-400 mb-2 line-clamp-1"
          :class="{ 'line-through text-gray-400 dark:text-gray-500': task.isCompleted }"
        >
          {{ task.description }}
        </p>

        <!-- Compact Info Row -->
        <div class="flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-gray-500 dark:text-gray-400">
          <!-- Due Date (for non-urgent dates) -->
          <div 
            v-if="task.dueDate && !task.isOverdue && !task.isDueSoon"
            class="flex items-center gap-1 text-gray-600 dark:text-gray-300 flex-shrink-0"
          >
            <i class="fas fa-calendar-alt text-xs"></i>
            <span>Due {{ formatDueDateShort(new Date(task.dueDate)) }}</span>
          </div>

          <!-- Estimate -->
          <div 
            v-if="task.estimatedHours"
            class="flex items-center gap-1 text-blue-600 dark:text-blue-400 flex-shrink-0"
          >
            <i class="fas fa-clock text-xs"></i>
            <span>{{ task.estimatedHours }}h</span>
          </div>

          <!-- Notification Status -->
          <div 
            v-if="task.notificationsEnabled && task.dueDate"
            class="flex items-center gap-1 text-purple-600 dark:text-purple-400 flex-shrink-0"
            title="Notifications enabled"
          >
            <i class="fas fa-bell text-xs"></i>
          </div>

          <!-- Creation Date -->
          <div class="flex items-center gap-1 text-gray-400 dark:text-gray-500">
            <i class="fas fa-plus text-xs"></i>
            <span class="hidden sm:inline">{{ formatDate(task.createdAt) }}</span>
            <span class="sm:hidden">{{ formatDateShort(task.createdAt) }}</span>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex-shrink-0 flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
        <button
          @click="$emit('edit', task)"
          class="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded transition-all duration-200"
          title="Edit task"
        >
          <i class="fas fa-edit text-xs"></i>
        </button>
        
        <button
          @click="$emit('delete', task.id)"
          class="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition-all duration-200"
          title="Delete task"
        >
          <i class="fas fa-trash text-xs"></i>
        </button>
      </div>
    </div>

    <!-- Completion Celebration Effect -->
    <div 
      v-if="task.isCompleted"
      class="absolute top-2 right-12 opacity-60"
    >
      <div class="animate-pulse text-green-500">
        <i class="fas fa-check-circle text-lg"></i>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Task } from '../types'
import PriorityIcon from './PriorityIcon.vue'

// Priority configuration with new chevron/dash design
const PRIORITY_CONFIG = {
  1: {
    label: 'Low',
    color: 'text-blue-600 dark:text-blue-400',
    bgColor: 'bg-blue-100 dark:bg-blue-900',
    badgeColor: 'bg-blue-100 text-blue-700 border border-blue-300 dark:bg-blue-900/50 dark:text-blue-300 dark:border-blue-600',
    chevrons: '⏷⏷⏷', // Three downward triangles
    customIcon: true
  },
  2: {
    label: 'Medium', 
    color: 'text-amber-600 dark:text-amber-400',
    bgColor: 'bg-amber-100 dark:bg-amber-900',
    badgeColor: 'bg-amber-100 text-amber-700 border border-amber-300 dark:bg-amber-900/50 dark:text-amber-300 dark:border-amber-600',
    dashes: '—⁠—⁠—', // Three horizontal dashes
    customIcon: true
  },
  3: {
    label: 'High',
    color: 'text-red-600 dark:text-red-400',
    bgColor: 'bg-red-100 dark:bg-red-900',
    badgeColor: 'bg-red-100 text-red-700 border border-red-300 dark:bg-red-900/50 dark:text-red-300 dark:border-red-600',
    chevrons: '⏶⏶⏶', // Three upward triangles
    customIcon: true
  }
}

// Props
interface Props {
  task: Task
}

const props = defineProps<Props>()

// Emits
interface Emits {
  toggle: [id: number]
  edit: [task: Task]
  delete: [id: number]
}

defineEmits<Emits>()

// Computed properties
const checkboxClasses = computed(() => ({
  'border-green-500 bg-green-500 shadow-lg shadow-green-500/30': props.task.isCompleted,
  'border-gray-300 bg-white hover:border-green-400 hover:shadow-md': !props.task.isCompleted
}))

const priorityConfig = computed(() => {
  const config = PRIORITY_CONFIG[props.task.priority as 1 | 2 | 3] || PRIORITY_CONFIG[1]
  return {
    ...config,
    barColor: {
      1: 'bg-blue-500',     // Low priority - blue
      2: 'bg-amber-500',    // Medium priority - amber
      3: 'bg-red-500'       // High priority - red
    }[props.task.priority] || 'bg-blue-500'
  }
})

const dueStatusConfig = computed(() => {
  if (!props.task.dueDate) {
    return { icon: '', text: '', textColor: '', badgeColor: '', cardColor: '', iconBg: '', badge: '' }
  }

  const dueDate = new Date(props.task.dueDate)
  
  if (props.task.isOverdue) {
    return {
      icon: 'fas fa-exclamation-triangle',
      text: `Overdue ${formatDueDate(dueDate)}`,
      textColor: 'text-red-700',
      badgeColor: 'bg-red-100 text-red-700 border border-red-200',
      cardColor: 'bg-red-50 border-red-200',
      iconBg: 'bg-red-100 text-red-600',
      badge: 'OVERDUE'
    }
  } else if (props.task.isDueSoon) {
    return {
      icon: 'fas fa-clock',
      text: `Due ${formatDueDate(dueDate)}`,
      textColor: 'text-orange-700',
      badgeColor: 'bg-orange-100 text-orange-700 border border-orange-200',
      cardColor: 'bg-orange-50 border-orange-200',
      iconBg: 'bg-orange-100 text-orange-600',
      badge: 'DUE SOON'
    }
  } else {
    return {
      icon: 'fas fa-calendar-alt',
      text: `Due ${formatDueDate(dueDate)}`,
      textColor: 'text-gray-700',
      badgeColor: 'bg-gray-100 text-gray-700 border border-gray-200',
      cardColor: 'bg-gray-50 border-gray-200',
      iconBg: 'bg-gray-100 text-gray-600',
      badge: 'SCHEDULED'
    }
  }
})

// Utility functions
function formatDueDate(date: Date): string {
  const now = new Date()
  const diffInHours = (date.getTime() - now.getTime()) / (1000 * 60 * 60)
  
  if (Math.abs(diffInHours) < 2) {
    return `${Math.abs(diffInHours) < 1 ? 'now' : 'in ' + Math.round(Math.abs(diffInHours)) + 'h'}`
  } else if (diffInHours < 24 && diffInHours > 0) {
    return `today ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
  } else if (diffInHours < 48 && diffInHours > 0) {
    return `tomorrow ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
  } else if (diffInHours < 0 && diffInHours > -24) {
    return `${Math.abs(Math.round(diffInHours))}h ago`
  } else {
    return date.toLocaleDateString([], { 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}



function formatDate(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffInHours = Math.abs(now.getTime() - date.getTime()) / (1000 * 60 * 60)

  if (diffInHours < 1) {
    return 'just now'
  } else if (diffInHours < 24) {
    return `${Math.round(diffInHours)}h ago`
  } else if (diffInHours < 24 * 7) {
    return date.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' })
  } else {
    return date.toLocaleDateString([], { year: 'numeric', month: 'short', day: 'numeric' })
  }
}

function formatDateShort(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffInDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24))
  
  if (diffInDays === 0) return 'Today'
  if (diffInDays === 1) return '1d'
  if (diffInDays < 7) return `${diffInDays}d`
  if (diffInDays < 30) return `${Math.floor(diffInDays / 7)}w`
  
  return date.toLocaleDateString('en-US', { month: 'numeric', day: 'numeric' })
}

function formatDueDateShort(date: Date): string {
  // Fix timezone issue by treating the date as local timezone without time
  const localDate = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  
  const diffInDays = Math.ceil((localDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24))
  
  if (diffInDays === 0) return 'Today'
  if (diffInDays === 1) return 'Tomorrow'
  if (diffInDays === -1) return 'Yesterday'
  if (diffInDays > 1 && diffInDays <= 7) return `${diffInDays}d`
  if (diffInDays < -1 && diffInDays >= -7) return `${Math.abs(diffInDays)}d ago`
  
  // For dates further out, show the actual date
  return localDate.toLocaleDateString([], { month: 'short', day: 'numeric' })
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-clamp: 2; /* Standard property for compatibility */
}

/* Custom focus ring for accessibility */
.focus\:ring-3:focus {
  --tw-ring-offset-shadow: var(--tw-ring-inset) 0 0 0 var(--tw-ring-offset-width) var(--tw-ring-offset-color);
  --tw-ring-shadow: var(--tw-ring-inset) 0 0 0 calc(3px + var(--tw-ring-offset-width)) var(--tw-ring-color);
  box-shadow: var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow, 0 0 #0000);
}

/* Smooth hover animations */
.group:hover .transition-all {
  transform: translateY(-1px);
}

/* Completion celebration */
@keyframes celebration {
  0% { transform: scale(1) rotate(0deg); }
  50% { transform: scale(1.1) rotate(5deg); }
  100% { transform: scale(1) rotate(0deg); }
}

.completion-celebration {
  animation: celebration 0.6s ease-in-out;
}
</style>