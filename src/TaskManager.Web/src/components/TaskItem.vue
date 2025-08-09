<template>
  <div 
    class="group bg-white rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-all duration-200 p-4"
    :class="{ 'opacity-75': task.isCompleted }"
  >
    <div class="flex items-start gap-3">
      <!-- Completion Checkbox -->
      <button
        @click="$emit('toggle', task.id)"
        class="flex-shrink-0 mt-1 w-5 h-5 rounded border-2 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-1"
        :class="checkboxClasses"
        :aria-label="task.isCompleted ? 'Mark as incomplete' : 'Mark as complete'"
      >
        <i v-if="task.isCompleted" class="fas fa-check text-white text-xs"></i>
      </button>

      <!-- Task Content -->
      <div class="flex-1 min-w-0">
        <!-- Title and Priority -->
        <div class="flex items-center gap-2 mb-1">
          <h3 
            class="font-medium text-gray-900 truncate"
            :class="{ 'line-through text-gray-500': task.isCompleted }"
          >
            {{ task.title }}
          </h3>
          
          <!-- Priority Badge -->
          <span 
            class="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium"
            :class="priorityConfig.badgeColor"
          >
            <i :class="`fas ${priorityConfig.icon}`" class="text-xs"></i>
            {{ task.priorityText }}
          </span>
        </div>

        <!-- Description -->
        <p 
          v-if="task.description" 
          class="text-sm text-gray-600 mb-2 line-clamp-2"
          :class="{ 'line-through': task.isCompleted }"
        >
          {{ task.description }}
        </p>

        <!-- Metadata -->
        <div class="flex items-center justify-between text-xs text-gray-500">
          <div class="flex items-center gap-4">
            <!-- Category -->
            <div v-if="task.categoryName" class="flex items-center gap-1">
              <div 
                class="w-2 h-2 rounded-full"
                :style="{ backgroundColor: task.categoryColor || '#6B7280' }"
              ></div>
              <span>{{ task.categoryName }}</span>
            </div>

            <!-- Created Date -->
            <div class="flex items-center gap-1">
              <i class="fas fa-calendar-alt"></i>
              <span>{{ formatDate(task.createdAt) }}</span>
            </div>

            <!-- Updated Date (if different from created) -->
            <div 
              v-if="task.updatedAt !== task.createdAt" 
              class="flex items-center gap-1"
            >
              <i class="fas fa-edit"></i>
              <span>Updated {{ formatDate(task.updatedAt) }}</span>
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
            <button
              @click="$emit('edit', task)"
              class="p-1 text-gray-400 hover:text-blue-600 transition-colors duration-200"
              title="Edit task"
            >
              <i class="fas fa-edit text-sm"></i>
            </button>
            
            <button
              @click="$emit('delete', task.id)"
              class="p-1 text-gray-400 hover:text-red-600 transition-colors duration-200"
              title="Delete task"
            >
              <i class="fas fa-trash text-sm"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Task } from '@/types'
import { PRIORITY_CONFIG } from '@/types'

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
  'border-green-500 bg-green-500': props.task.isCompleted,
  'border-gray-300 bg-white hover:border-green-400': !props.task.isCompleted
}))

const priorityConfig = computed(() => PRIORITY_CONFIG[props.task.priority as keyof typeof PRIORITY_CONFIG])

// Utility functions
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffInHours = Math.abs(now.getTime() - date.getTime()) / (1000 * 60 * 60)

  if (diffInHours < 24) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  } else if (diffInHours < 24 * 7) {
    return date.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' })
  } else {
    return date.toLocaleDateString([], { year: 'numeric', month: 'short', day: 'numeric' })
  }
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
