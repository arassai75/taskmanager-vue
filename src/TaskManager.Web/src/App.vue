<template>
  <div id="app" class="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors duration-300 flex flex-col">
    <!-- Header -->
    <header class="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 transition-colors duration-300">
      <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <!-- Logo and Title -->
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 bg-primary-600 rounded-lg flex items-center justify-center">
              <i class="fas fa-tasks text-white text-sm"></i>
            </div>
            <div>
              <h1 class="text-xl font-bold text-gray-900 dark:text-white">TaskManager</h1>
              <p class="text-xs text-gray-500 dark:text-gray-400">Efficient Task Management</p>
            </div>
          </div>

          <!-- Header Actions -->
          <div class="flex items-center gap-4">

            <!-- Statistics Summary -->
            <div class="hidden md:flex items-center gap-4 text-sm text-gray-600 dark:text-gray-300">
              <div class="flex items-center gap-1">
                <span class="w-2 h-2 bg-yellow-500 rounded-full"></span>
                <span>{{ taskStore.taskCounts.active }} Active</span>
              </div>
              <div class="flex items-center gap-1">
                <span class="w-2 h-2 bg-green-500 rounded-full"></span>
                <span>{{ taskStore.taskCounts.completed }} Completed</span>
              </div>
              <div v-if="taskStore.taskCounts.highPriority > 0" class="flex items-center gap-1">
                <span class="w-2 h-2 bg-red-500 rounded-full"></span>
                <span>{{ taskStore.taskCounts.highPriority }} High Priority</span>
              </div>
            </div>

            <!-- Theme Toggle -->
            <button
              @click="toggleTheme"
              class="relative p-2 rounded-lg bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 transition-colors duration-200"
              title="Toggle theme"
            >
              <!-- Sun icon for light mode -->
              <i 
                v-if="currentTheme === 'dark'"
                class="fas fa-sun text-yellow-500 text-lg"
              ></i>
              <!-- Moon icon for dark mode -->
              <i 
                v-else
                class="fas fa-moon text-gray-600 text-lg"
              ></i>
            </button>

          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1 max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8 dark:text-gray-100 w-full">
      <!-- Loading Overlay -->
      <div
        v-if="taskStore.loading.isLoading && taskStore.tasks.length === 0"
        class="fixed inset-0 bg-black bg-opacity-25 flex items-center justify-center z-40"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg p-6 shadow-xl">
          <div class="flex items-center gap-3">
            <i class="fas fa-spinner fa-spin text-primary-600 text-xl"></i>
            <span class="text-gray-700 dark:text-gray-300">Loading TaskManager...</span>
          </div>
        </div>
      </div>

      <!-- Task Management Interface -->
      <div class="space-y-6">
        <!-- Create/Edit Task Form -->
        <div v-if="showForm" class="animate-fade-in">
          <TaskForm
            :task="editingTask"
            :is-submitting="formSubmitting"
            @submit="handleTaskSubmit"
            @cancel="cancelForm"
          />
        </div>

        <!-- Task List -->
        <TaskList
          @create="showCreateForm"
          @edit="showEditForm"
          @toggle="handleToggleTask"
          @delete="handleDeleteTask"
        />
      </div>
    </main>

    <!-- Footer -->
    <footer class="mt-auto bg-gradient-to-r from-gray-900 via-gray-800 to-gray-900 dark:from-gray-950 dark:via-gray-900 dark:to-gray-950 border-t border-gray-200 dark:border-gray-700 transition-colors duration-300">
      <!-- Decorative top border -->
      <div class="h-1 bg-gradient-to-r from-primary-500 via-purple-500 to-primary-600"></div>
      
      <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8 text-center md:text-left">
          <!-- Brand Section -->
          <div class="space-y-3">
            <div class="flex items-center justify-center md:justify-start gap-2">
              <div class="w-8 h-8 bg-gradient-to-br from-primary-500 to-purple-600 rounded-lg flex items-center justify-center">
                <i class="fas fa-tasks text-white text-sm"></i>
              </div>
              <h3 class="text-xl font-bold text-white">TaskManager</h3>
            </div>
            <p class="text-gray-300 text-sm leading-relaxed">
              Efficient task management for productive teams.
            </p>
            <div class="flex items-center justify-center md:justify-start gap-1 text-xs text-gray-400">
              <i class="fas fa-rocket text-primary-400"></i>
              <span>Version 1.1.0</span>
            </div>
          </div>

          <!-- Tech Stack -->
          <div class="space-y-3">
            <h4 class="text-sm font-semibold text-white uppercase tracking-wider">Built With</h4>
            <div class="space-y-2">
              <div class="flex items-center justify-center md:justify-start gap-2 text-sm text-gray-300">
                <div class="w-4 h-4 bg-green-500 rounded flex items-center justify-center">
                  <i class="fab fa-vuejs text-white text-xs"></i>
                </div>
                <span class="font-medium text-green-400">Vue 3</span>
                <span class="text-gray-500">+</span>
                <span class="text-blue-400">TypeScript</span>
              </div>
              <div class="flex items-center justify-center md:justify-start gap-2 text-sm text-gray-300">
                <div class="w-4 h-4 bg-purple-600 rounded flex items-center justify-center">
                  <i class="fab fa-microsoft text-white text-xs"></i>
                </div>
                <span class="font-medium text-purple-400">.NET 9</span>
                <span class="text-gray-500">+</span>
                <span class="text-blue-400">SQLite</span>
              </div>
              <div class="flex items-center justify-center md:justify-start gap-2 text-sm text-gray-300">
                <div class="w-4 h-4 bg-cyan-500 rounded flex items-center justify-center">
                  <i class="fas fa-wind text-white text-xs"></i>
                </div>
                <span class="font-medium text-cyan-400">Tailwind CSS</span>
              </div>
            </div>
          </div>

          <!-- Links & Stats -->
          <div class="space-y-3">
            <h4 class="text-sm font-semibold text-white uppercase tracking-wider">Quick Links</h4>
            <div class="space-y-2">
              <a
                href="http://localhost:5001/swagger/index.html"
                target="_blank"
                class="flex items-center justify-center md:justify-start gap-2 text-sm text-gray-300 hover:text-primary-400 transition-colors duration-200 group"
              >
                <i class="fas fa-book text-primary-500 group-hover:text-primary-400"></i>
                <span>API Documentation</span>
                <i class="fas fa-external-link-alt text-xs opacity-50"></i>
              </a>
              <div class="flex items-center justify-center md:justify-start gap-2 text-sm text-gray-300">
                <i class="fas fa-server text-green-500"></i>
                <span>API Status: </span>
                <div class="flex items-center gap-2">
                <div class="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
                <span class="text-xs">Live</span>
              </div>
              </div>
              <div class="flex items-center justify-center md:justify-start gap-2 text-sm text-gray-300">
                <i class="fas fa-tasks text-blue-500"></i>
                <span>Tasks Managed: </span>
                <span class="text-blue-400 font-medium">{{ taskStore.taskCounts.total }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Bottom Bar -->
        <div class="mt-8 pt-6 border-t border-gray-700 dark:border-gray-600">
          <div class="flex flex-col sm:flex-row justify-between items-center gap-4 text-sm">
            <div class="text-gray-400">
              <span>&copy; 2025 TaskManager. | Ali Rassai</span>
            </div>
          </div>
        </div>
      </div>
    </footer>

    <!-- Notification Manager -->
    <NotificationManager />

    <!-- Confirmation Modal (for delete actions) -->
    <ConfirmationModal
      v-if="showDeleteConfirmation"
      title="Delete Task"
      :message="`Are you sure you want to delete '${taskToDelete?.title}'? This action cannot be undone.`"
      confirm-text="Delete"
      confirm-variant="danger"
      @confirm="confirmDelete"
      @cancel="cancelDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useTaskStore } from './stores/taskStore'
import TaskList from './components/TaskList.vue'
import TaskForm from './components/TaskForm.vue'
import NotificationManager from './components/NotificationManager.vue'
import ConfirmationModal from './components/ConfirmationModal.vue'
import type { Task, CreateTaskDto, UpdateTaskDto } from './types'

// Store
const taskStore = useTaskStore()
// Note: Don't destructure reactive store values as it breaks reactivity

// Form state
const showForm = ref(false)
const editingTask = ref<Task | undefined>(undefined)
const formSubmitting = ref(false)

// Delete confirmation state
const showDeleteConfirmation = ref(false)
const taskToDelete = ref<Task | undefined>(undefined)

// Theme management
const currentTheme = ref<'light' | 'dark'>('light')

// Initialize theme on app load
onMounted(() => {
  // Check localStorage first, then system preference
  const savedTheme = localStorage.getItem('theme') as 'light' | 'dark' | null
  const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  
  currentTheme.value = savedTheme || systemTheme
  applyTheme(currentTheme.value)
  
  // Listen for system theme changes
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
    if (!localStorage.getItem('theme')) {
      currentTheme.value = e.matches ? 'dark' : 'light'
      applyTheme(currentTheme.value)
    }
  })
})

function toggleTheme() {
  currentTheme.value = currentTheme.value === 'light' ? 'dark' : 'light'
  applyTheme(currentTheme.value)
  localStorage.setItem('theme', currentTheme.value)
}

function applyTheme(theme: 'light' | 'dark') {
  if (theme === 'dark') {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
}

// Methods
function showCreateForm() {
  editingTask.value = undefined
  showForm.value = true
}

function showEditForm(task: Task) {
  editingTask.value = task
  showForm.value = true
}

function cancelForm() {
  showForm.value = false
  editingTask.value = undefined
  formSubmitting.value = false
}

async function handleTaskSubmit(taskData: CreateTaskDto | UpdateTaskDto) {
  formSubmitting.value = true
  
  try {
    if (editingTask.value) {
      // Update existing task
      await taskStore.updateTask(editingTask.value.id, taskData as UpdateTaskDto)
    } else {
      // Create new task
      await taskStore.createTask(taskData as CreateTaskDto)
    }
    
    // Close form on success
    cancelForm()
  } catch (error) {
    // Error is handled by the store and notifications are shown
    console.error('Failed to submit task:', error)
  } finally {
    formSubmitting.value = false
  }
}

async function handleToggleTask(taskId: number) {
  await taskStore.toggleTask(taskId)
}

function handleDeleteTask(taskId: number) {
  const task = taskStore.getTaskById(taskId)
  if (task) {
    taskToDelete.value = task
    showDeleteConfirmation.value = true
  }
}

async function confirmDelete() {
  if (taskToDelete.value) {
    await taskStore.deleteTask(taskToDelete.value.id)
  }
  cancelDelete()
}

function cancelDelete() {
  showDeleteConfirmation.value = false
  taskToDelete.value = undefined
}

// Keyboard shortcuts
function handleKeyboard(event: KeyboardEvent) {
  // Ctrl/Cmd + N = New task
  if ((event.ctrlKey || event.metaKey) && event.key === 'n') {
    event.preventDefault()
    showCreateForm()
  }
  
  // Escape = Cancel form
  if (event.key === 'Escape') {
    if (showForm.value) {
      cancelForm()
    } else if (showDeleteConfirmation.value) {
      cancelDelete()
    }
  }
}

// Lifecycle
onMounted(() => {
  console.log('🎯 App.vue onMounted() called')
  // Initialize the store
  console.log('🔧 Calling taskStore.initialize()...')
  taskStore.initialize()
  
  // Set up keyboard shortcuts
  document.addEventListener('keydown', handleKeyboard)
  
  // Cleanup
  return () => {
    document.removeEventListener('keydown', handleKeyboard)
  }
})
</script>

<style scoped>
/* Custom animations */
.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Custom scrollbar for the main content */
:deep(.scroll-smooth) {
  scroll-behavior: smooth;
}

/* Focus styles for accessibility */
:deep(*:focus) {
  outline: 2px solid #3b82f6;
  outline-offset: 2px;
}

/* Print styles */
@media print {
  header,
  footer,
  .no-print {
    display: none !important;
  }
  
  .print-break-inside-avoid {
    break-inside: avoid;
  }
}
</style>

