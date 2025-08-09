<template>
  <div class="space-y-4">
    <!-- Header with Statistics -->
    <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <!-- Statistics Cards -->
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 flex-1">
          <div class="text-center">
            <div class="text-2xl font-bold text-blue-600">{{ taskStore.taskCounts.total }}</div>
            <div class="text-sm text-gray-600">Total Tasks</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-yellow-600">{{ taskStore.taskCounts.active }}</div>
            <div class="text-sm text-gray-600">Active</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-green-600">{{ taskStore.taskCounts.completed }}</div>
            <div class="text-sm text-gray-600">Completed</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-red-600">{{ taskStore.taskCounts.highPriority }}</div>
            <div class="text-sm text-gray-600">High Priority</div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex gap-2">
          <button
            @click="$emit('create')"
            class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 transition-colors duration-200"
          >
            <i class="fas fa-plus"></i>
            Add Task
          </button>
          
          <button
            @click="refreshTasks"
            class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 transition-colors duration-200"
            :disabled="taskStore.loading.isLoading"
          >
            <i class="fas fa-refresh" :class="{ 'fa-spin': taskStore.loading.isLoading }"></i>
            Refresh
          </button>
        </div>
      </div>
    </div>

    <!-- Filters and Search -->
    <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
      <div class="flex flex-col lg:flex-row gap-4">
        <!-- Search Input -->
        <div class="flex-1">
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <i class="fas fa-search text-gray-400"></i>
            </div>
            <input
              v-model="searchTerm"
              type="text"
              placeholder="Search tasks by title, description, or category..."
              class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
            />
            <button
              v-if="searchTerm"
              @click="searchTerm = ''"
              class="absolute inset-y-0 right-0 pr-3 flex items-center"
            >
              <i class="fas fa-times text-gray-400 hover:text-gray-600"></i>
            </button>
          </div>
        </div>

        <!-- Filter Controls -->
        <div class="flex flex-wrap gap-2">
          <!-- Priority Filter -->
          <select
            v-model="selectedPriority"
            class="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 text-sm"
          >
            <option :value="undefined">All Priorities</option>
            <option :value="3">🔺 High Priority</option>
            <option :value="2">➖ Medium Priority</option>
            <option :value="1">🔽 Low Priority</option>
          </select>

          <!-- Category Filter -->
          <select
            v-model="selectedCategory"
            class="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 text-sm"
          >
            <option :value="undefined">All Categories</option>
            <option :value="1">📋 General</option>
            <option :value="2">💼 Work</option>
            <option :value="3">👤 Personal</option>
            <option :value="4">🚨 Urgent</option>
          </select>

          <!-- Completion Status Toggle -->
          <label class="flex items-center gap-2 px-3 py-2 bg-gray-50 rounded-md cursor-pointer">
            <input
              v-model="showCompleted"
              type="checkbox"
              class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            />
            <span class="text-sm text-gray-700">Show Completed</span>
          </label>

          <!-- Clear Filters -->
          <button
            v-if="hasActiveFilters"
            @click="clearFilters"
            class="px-3 py-2 text-sm text-gray-600 hover:text-gray-800 transition-colors duration-200"
          >
            <i class="fas fa-times mr-1"></i>
            Clear
          </button>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="taskStore.loading.isLoading && taskStore.filteredTasks.length === 0" class="text-center py-12">
      <div class="inline-flex items-center gap-3 text-gray-600">
        <i class="fas fa-spinner fa-spin text-xl"></i>
        <span>Loading tasks...</span>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="taskStore.loading.error" class="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
      <div class="text-red-600 mb-2">
        <i class="fas fa-exclamation-triangle text-xl"></i>
      </div>
      <h3 class="text-lg font-medium text-red-900 mb-2">Error Loading Tasks</h3>
      <p class="text-red-700 mb-4">{{ taskStore.loading.error }}</p>
      <button
        @click="refreshTasks"
        class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-red-700 bg-red-100 border border-red-300 rounded-md hover:bg-red-200 transition-colors duration-200"
      >
        <i class="fas fa-refresh"></i>
        Try Again
      </button>
    </div>

    <!-- Empty State -->
    <div v-else-if="taskStore.filteredTasks.length === 0" class="text-center py-12">
      <div class="text-gray-400 mb-4">
        <i class="fas fa-tasks text-4xl"></i>
      </div>
      <h3 class="text-lg font-medium text-gray-900 mb-2">
        {{ hasActiveFilters ? 'No tasks match your filters' : 'No tasks yet' }}
      </h3>
      <p class="text-gray-600 mb-6">
        {{ hasActiveFilters 
            ? 'Try adjusting your search criteria or filters.' 
            : 'Create your first task to get started with task management.' 
        }}
      </p>
      <button
        v-if="!hasActiveFilters"
        @click="$emit('create')"
        class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 transition-colors duration-200"
      >
        <i class="fas fa-plus"></i>
        Create Your First Task
      </button>
      <button
        v-else
        @click="clearFilters"
        class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 transition-colors duration-200"
      >
        <i class="fas fa-times"></i>
        Clear Filters
      </button>
    </div>

    <!-- Task List -->
    <div v-else class="space-y-3">
      <!-- Sort Options -->
      <div class="flex justify-between items-center">
        <div class="text-sm text-gray-600">
          {{ taskStore.filteredTasks.length }} {{ taskStore.filteredTasks.length === 1 ? 'task' : 'tasks' }}
          {{ hasActiveFilters ? '(filtered)' : '' }}
        </div>
        
        <select
          v-model="sortOption"
          class="text-sm border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="createdAt:desc">Newest First</option>
          <option value="createdAt:asc">Oldest First</option>
          <option value="priority:desc">High Priority First</option>
          <option value="priority:asc">Low Priority First</option>
          <option value="title:asc">Title A-Z</option>
          <option value="title:desc">Title Z-A</option>
        </select>
      </div>

      <!-- Tasks -->
      <TransitionGroup
        name="task-list"
        tag="div"
        class="space-y-3"
      >
        <TaskItem
          v-for="task in sortedTasks"
          :key="task.id"
          :task="task"
          @toggle="$emit('toggle', $event)"
          @edit="$emit('edit', $event)"
          @delete="$emit('delete', $event)"
        />
      </TransitionGroup>
    </div>

    <!-- Load More Button (for future pagination) -->
    <div v-if="false" class="text-center">
      <button
        class="px-6 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 transition-colors duration-200"
      >
        Load More Tasks
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useTaskStore } from '@/stores/taskStore'
import TaskItem from './TaskItem.vue'
import type { Task } from '@/types'

// Emits
interface Emits {
  create: []
  edit: [task: Task]
  toggle: [id: number]
  delete: [id: number]
}

defineEmits<Emits>()

// Store
const taskStore = useTaskStore()

// Reactive state
const searchTerm = ref('')
const selectedPriority = ref<number | undefined>(undefined)
const selectedCategory = ref<number | undefined>(undefined)
const showCompleted = ref(true)
const sortOption = ref('createdAt:desc')

// Note: Access store properties directly to maintain reactivity

const hasActiveFilters = computed(() => 
  searchTerm.value.trim() !== '' || 
  selectedPriority.value !== undefined || 
  selectedCategory.value !== undefined || 
  !showCompleted.value
)

const sortedTasks = computed(() => {
  const [field, direction] = sortOption.value.split(':') as [keyof Task, 'asc' | 'desc']
  
  return [...taskStore.filteredTasks].sort((a, b) => {
    const aValue = a[field]
    const bValue = b[field]
    
    // Handle undefined values
    if (aValue == null && bValue == null) return 0
    if (aValue == null) return direction === 'asc' ? -1 : 1
    if (bValue == null) return direction === 'asc' ? 1 : -1
    
    if (aValue < bValue) return direction === 'asc' ? -1 : 1
    if (aValue > bValue) return direction === 'asc' ? 1 : -1
    return 0
  })
})

// Watch for filter changes and update store
watch(
  [searchTerm, selectedPriority, selectedCategory, showCompleted],
  ([search, priority, category, completed]) => {
    taskStore.updateFilters({
      searchTerm: search,
      selectedPriority: priority,
      selectedCategory: category,
      showCompleted: completed
    })
  },
  { immediate: true }
)

// Methods
function refreshTasks() {
  taskStore.fetchTasks(showCompleted.value)
}

function clearFilters() {
  searchTerm.value = ''
  selectedPriority.value = undefined
  selectedCategory.value = undefined
  showCompleted.value = true
  taskStore.resetFilters()
}
</script>

<style scoped>
/* Transition animations for task list */
.task-list-enter-active,
.task-list-leave-active {
  transition: all 0.3s ease;
}

.task-list-enter-from {
  opacity: 0;
  transform: translateY(-20px);
}

.task-list-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

.task-list-move {
  transition: transform 0.3s ease;
}
</style>
