<template>
  <div id="app" class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow-sm border-b border-gray-200">
      <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <!-- Logo and Title -->
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 bg-primary-600 rounded-lg flex items-center justify-center">
              <i class="fas fa-tasks text-white text-sm"></i>
            </div>
            <div>
              <h1 class="text-xl font-bold text-gray-900">TaskManager</h1>
              <p class="text-xs text-gray-500">Efficient Task Management</p>
            </div>
          </div>

          <!-- Header Actions -->
          <div class="flex items-center gap-4">
            <!-- Statistics Summary -->
            <div class="hidden md:flex items-center gap-4 text-sm text-gray-600">
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

            <!-- Theme Toggle (Future Enhancement) -->
            <button
              class="p-2 text-gray-500 hover:text-gray-700 transition-colors duration-200"
              title="Toggle theme (coming soon)"
              disabled
            >
              <i class="fas fa-moon"></i>
            </button>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading Overlay -->
      <div
        v-if="taskStore.loading.isLoading && taskStore.tasks.length === 0"
        class="fixed inset-0 bg-black bg-opacity-25 flex items-center justify-center z-40"
      >
        <div class="bg-white rounded-lg p-6 shadow-xl">
          <div class="flex items-center gap-3">
            <i class="fas fa-spinner fa-spin text-primary-600 text-xl"></i>
            <span class="text-gray-700">Loading TaskManager...</span>
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
    <footer class="bg-white border-t border-gray-200 mt-16">
      <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <div class="flex flex-col md:flex-row justify-between items-center gap-4">
          <div class="text-sm text-gray-600">
            <span>&copy; 2025 TaskManager. Built with </span>
            <span class="text-primary-600 font-medium">Vue 3</span>
            <span> and </span>
            <span class="text-primary-600 font-medium">.NET 8</span>
          </div>
          
          <div class="flex items-center gap-4 text-sm text-gray-500">
            <a
              href="http://localhost:5001/swagger/"
              target="_blank"
              class="hover:text-primary-600 transition-colors duration-200"
            >
              <i class="fas fa-book mr-1"></i>
              Swagger
            </a>
            <div class="text-xs text-gray-400">
              v1.0.0
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
import { useTaskStore } from '@/stores/taskStore'
import TaskList from '@/components/TaskList.vue'
import TaskForm from '@/components/TaskForm.vue'
import NotificationManager from '@/components/NotificationManager.vue'
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import type { Task, CreateTaskDto, UpdateTaskDto } from '@/types'

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
  // Initialize the store
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

</style>

